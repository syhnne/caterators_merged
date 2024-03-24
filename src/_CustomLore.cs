using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Drawing.Text;
using RWCustom;
using Noise;
using System.Collections.Generic;
using MoreSlugcats;
using BepInEx.Logging;
using Smoke;
using Random = UnityEngine.Random;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using System.Runtime.CompilerServices;
using Menu;
using static Caterators_merged.srs.OxygenMaskModules;

namespace Caterators_merged;



public class CustomLore
{



    public static CustomDeathPersistentSaveData DPSaveData;

    public static void Apply()
    {
        
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.Update += MSCRoomSpecificScript_GourmandEnding_Update;

        
        On.RainWorldGame.GoToRedsGameOver += RainWorldGame_GoToRedsGameOver;
        On.RainWorldGame.BeatGameMode += RainWorldGame_BeatGameMode;
        On.Room.Loaded += Room_Loaded;


        On.Menu.SlugcatSelectMenu.SlugcatPage.AddAltEndingImage += SlugcatPage_AddAltEndingImage;
        On.Menu.SlideShow.ctor += SlideShow_ctor;


        On.SaveState.ctor += SaveState_ctor;
        On.DeathPersistentSaveData.SaveToString += DeathPersistentSaveData_SaveToString;
        On.DeathPersistentSaveData.FromString += DeathPersistentSaveData_FromString;

        // OxygenMask
        On.AbstractPhysicalObject.UsesAPersistantTracker += AbstractPhysicalObject_UsesAPersistantTracker;
        On.SaveState.SaveToString += SaveState_SaveToString;

        fp.CustomLore.Apply();
        srs.CustomLore.Apply();
    }


    // 复活月姐
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        if (self.IsStorySession && Enums.IsCaterator(self.StoryCharacter) && self.StoryCharacter != Enums.FPname)
        {
            // self.GetStorySession.saveState.miscWorldSaveData.moonRevived = true;
            self.GetStorySession.saveState.miscWorldSaveData.moonHeartRestored = true;
            self.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken = true;
        }
    }










    // 防止玩家用特殊手段归乡（别想在酒吧点炒饭
    private static void MSCRoomSpecificScript_GourmandEnding_Update(On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.orig_Update orig, MSCRoomSpecificScript.OE_GourmandEnding self, bool eu)
    {
        if (self.room.game.IsStorySession && Enums.IsCaterator(self.room.game.StoryCharacter)) return;
        orig(self, eu);
    }




    private static void RainWorldGame_BeatGameMode(On.RainWorldGame.orig_BeatGameMode orig, RainWorldGame game, bool standardVoidSea)
    {
        orig(game, standardVoidSea);
        if (game.IsStorySession && game.GetStorySession.saveState.saveStateNumber == Enums.FPname)
        {
            fp.CustomLore.RainWorldGame_BeatGameMode(game, standardVoidSea);
        }
    }





    // 所有结局都在这，但altending和飞升还有那个beatgamemode
    private static void RainWorldGame_GoToRedsGameOver(On.RainWorldGame.orig_GoToRedsGameOver orig, RainWorldGame self)
    {
        if (self.IsStorySession && self.GetStorySession.saveState.saveStateNumber == Enums.FPname)
        {
            fp.CustomLore.RainWorldGame_GoToRedsGameOver(self);
        }
        orig(self);
    }




    private static void SlugcatPage_AddAltEndingImage(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_AddAltEndingImage orig, SlugcatSelectMenu.SlugcatPage self)
    {
        if (self.slugcatNumber == Enums.FPname)
        {
            fp.CustomLore.SlugcatPage_AddAltEndingImage(self);
        }
        else { orig(self); }
    }


    private static void SlideShow_ctor(On.Menu.SlideShow.orig_ctor orig, Menu.SlideShow self, ProcessManager manager, Menu.SlideShow.SlideShowID slideShowID)
    {
        orig(self, manager, slideShowID);
        fp.CustomLore.SlideShow_ctor(self, manager, slideShowID);
    }



    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        if (self.game != null && self.game.IsStorySession && Enums.IsCaterator(self.game.StoryCharacter)) 
        {
            if (self.game.StoryCharacter == Enums.FPname)
            {
                fp.CustomLore.AddRoomSpecificScripts(self);
            }
            else if (self.game.StoryCharacter == Enums.SRSname && self.abstractRoom.name == "SS_AI" && DPSaveData != null && !DPSaveData.OxygenMaskTaken)
            {
                Plugin.Log("Add OxygenMask");

                AbstractPhysicalObject abstr = new OxygenMaskAbstract(self.game.world, new WorldCoordinate(self.abstractRoom.index, -1, -1, 0), self.game.GetNewID(), 3);
                abstr.destroyOnAbstraction = true;
                self.abstractRoom.AddEntity(abstr);
                abstr.RealizeInRoom();
                (abstr.realizedObject as OxygenMask).firstChunk.pos = new Vector2(300f, 300f);
            }
        }
        

        orig(self);

    }




    private static void SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
    {
        orig(self, saveStateNumber, progression);
        if (DPSaveData == null)
        {
            DPSaveData = new(saveStateNumber);
        }
        else
        {
            DPSaveData.ClearData(saveStateNumber);
        }
    }

    private static string DeathPersistentSaveData_SaveToString(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
    {
        string result = orig(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);

        result = DPSaveData.SaveToString(result);
        Plugin.LogStat("DPSaveData:", result);
        return result;
    }

    static private void DeathPersistentSaveData_FromString(On.DeathPersistentSaveData.orig_FromString orig, DeathPersistentSaveData self, string s)
    {
        orig(self, s);

        DPSaveData.FromString(self.unrecognizedSaveStrings);
        List<string> ToRemove = DPSaveData.saveStrings;

        for (int k = 0; k < self.unrecognizedSaveStrings.Count; k++)
        {
            foreach (string str in ToRemove)
            {
                if (self.unrecognizedSaveStrings[k].Contains(str))
                {
                    self.unrecognizedSaveStrings.RemoveAt(k);
                }
            }
        }

    }



    #region OxygenMask

    private static bool AbstractPhysicalObject_UsesAPersistantTracker(On.AbstractPhysicalObject.orig_UsesAPersistantTracker orig, AbstractPhysicalObject abs)
    {
        if (abs is OxygenMaskAbstract)
        {
            return true;
        }
        return orig(abs);
    }


    private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        string result = orig(self);
        Plugin.Log("SaveState:", result);
        return result;

    }

    #endregion



}

﻿using System;
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
using static Caterators_by_syhnne.srs.OxygenMaskModules;
using SlugBase.SaveData;
using SlugBase;

namespace Caterators_by_syhnne._public;



public class CustomLore
{




    public static void Apply()
    {

        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.Update += MSCRoomSpecificScript_GourmandEnding_Update;

        On.RainWorldGame.Win += RainWorldGame_Win;
        On.RainWorldGame.GoToRedsGameOver += RainWorldGame_GoToRedsGameOver;
        On.RainWorldGame.BeatGameMode += RainWorldGame_BeatGameMode;
        On.Room.Loaded += Room_Loaded;


        

        // 由于我把剧情砍了，这个可以去掉了。。
        On.SlugcatStats.SlugcatUnlocked += SlugcatStats_SlugcatUnlocked;

        // OxygenMask
        On.AbstractPhysicalObject.UsesAPersistantTracker += AbstractPhysicalObject_UsesAPersistantTracker;

        
        srs.CustomLore.Apply();
        moon.CustomLore.Apply();
    }


    // 复活月姐
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        if (self.IsStorySession && self.IsCaterator() && self.StoryCharacter != Enums.FPname)
        {
            // self.GetStorySession.saveState.miscWorldSaveData.moonRevived = true;
            self.GetStorySession.saveState.miscWorldSaveData.moonHeartRestored = true;
            self.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken = true;
        }
        if (self.IsStorySession && self.StoryCharacter == Enums.test)
        {
            self.GetStorySession.saveState.hasRobo = true;
        }
        // 这不会影响别的存档罢（心虚）会的话我就删了
        else if (self.IsStorySession && self.StoryCharacter == Enums.Moonname)
        {
            self.GetStorySession.saveState.deathPersistentSaveData.ripMoon = true;
        }
    }




    // 修改未解锁的角色描述
    // 本想直接改游戏的，但不知道那mod启用顺序咋回事，我一动slugbase就卡bug，那就直接改slugbase数据吧（
    // 但 这玩意儿刷新的及时吗
    private static bool SlugcatStats_SlugcatUnlocked(On.SlugcatStats.orig_SlugcatUnlocked orig, SlugcatStats.Name i, RainWorld rainWorld)
    {
        if (!Enums.IsCaterator(i))
        {
            return orig(i, rainWorld);
        }
        else if (Options.DevMode.Value || !Options.EnableCampaign.Value)
        {
            return true;
        }
        else if (i == Enums.NSHname)
        {
            if (rainWorld.GetMiscProgression().beaten_fp || rainWorld.GetMiscProgression().beaten_srs)
            {
                return true;
            }
            if (SlugBaseCharacter.TryGet(i, out var chara))
            {
                chara.Description = "Clear the game as FP or SRS to unlock.";
            }
            return false;
        }
        else if (i == Enums.Moonname)
        {
            if (rainWorld.GetMiscProgression().beaten_nsh)
            {
                return true;
            }
            if (SlugBaseCharacter.TryGet(i, out var chara))
            {
                chara.Description = "Clear the game as NSH to unlock.";
            }
            return false;
        }
        return i == Enums.FPname || i == Enums.SRSname || orig(i, rainWorld);
    }







    // 防止玩家用特殊手段归乡（别想在酒吧点炒饭
    private static void MSCRoomSpecificScript_GourmandEnding_Update(On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.orig_Update orig, MSCRoomSpecificScript.OE_GourmandEnding self, bool eu)
    {
        if (self.room.game.IsStorySession && self.room.game.IsCaterator()) return;
        orig(self, eu);
    }





    // 防止玩家在循环耗尽的时候正常睡觉
    // 并且保存数据
    private static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished, bool fromWarpPoint)
    {
        if (self.manager.upcomingProcess != null) return;
        foreach (AbstractCreature player in self.Players)
        {
            if (player.realizedCreature != null && Plugin.playerModules.TryGetValue(player.realizedCreature as Player, out var module))
            {
                if (module.nshInventory != null)
                {
                    self.GetDeathPersistent().NSHInventoryStrings = new string[4];
                    self.GetDeathPersistent().NSHInventoryStrings[player.ID.number] = module.nshInventory.CycleEndSave();
                    Plugin.Log("inventory save:", self.GetDeathPersistent().NSHInventoryStrings[player.ID.number]);
                }
                if (module.swarmerManager != null)
                {
                    int? sw = module.swarmerManager.CycleEndSave();
                    if (sw != null)
                    {
                        sw = sw > moon.MoonSwarmer.SwarmerManager.maxSwarmer ? sw : Math.Min((int)sw + 1, moon.MoonSwarmer.SwarmerManager.maxSwarmer);
                        self.GetDeathPersistent().MoonHasSwarmers = (int)sw;
                        self.GetMiscProgression().MoonSwarmers = (int)sw;
                        Plugin.Log("moonswarmer save:", self.GetDeathPersistent().MoonHasSwarmers);
                    }
                }
            }
        }


        if (self.IsStorySession && self.StoryCharacter == Enums.FPname)
        {
            fp.CustomLore.RainWorldGame_Win(self, malnourished);
        }
        orig(self, malnourished, fromWarpPoint);
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




    


    


    /// <summary>
    /// aka AddRoomSpecificScripts
    /// </summary>
    /// <param name="self"></param>
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        if (self.game != null && self.game.IsStorySession && self.game.IsCaterator())
        {
            if (self.game.StoryCharacter == Enums.FPname)
            {
                fp.CustomLore.Room_Loaded(self);
            }
            else if (self.game.StoryCharacter == Enums.SRSname)
            {
                srs.CustomLore.Room_Loaded(self);
            }
            else if (self.game.StoryCharacter == Enums.NSHname)
            {
                nsh.CustomLore.Room_Loaded(self);
            }
            else if (self.game.StoryCharacter == Enums.Moonname)
            {
                moon.CustomLore.Room_Loaded(self);
            }
        }


        orig(self);

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

    #endregion



}

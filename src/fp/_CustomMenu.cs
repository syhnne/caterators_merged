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

namespace Caterators_by_syhnne.fp;

public class CustomMenu
{

    public static void Apply()
    {
        On.Menu.StoryGameStatisticsScreen.CommunicateWithUpcomingProcess += Menu_StoryGameStatisticsScreen_CommunicateWithUpcomingProcess;
        On.Menu.SlugcatSelectMenu.UpdateStartButtonText += Menu_SlugcatSelectMenu_UpdateStartButtonText;
        On.Menu.SlugcatSelectMenu.ContinueStartedGame += Menu_SlugcatSelectMenu_ContinueStartedGame;
        // IL.SaveState.LoadGame += SaveState_LoadGame;
    }


    private static void SaveState_LoadGame(ILContext il)
    {
        Plugin.Log("SaveState_LoadGame hooked");
        ILCursor c = new ILCursor(il);
        // 我敲 不会真是他导致的罢 我怎么没见这个函数挂上去呢
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Brfalse_S),
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            Plugin.Log("Match successfully! - SaveState_LoadGame - 1326");
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate<Func<int, RainWorldGame, int>>((redsCycles, game) =>
            {
                return (game != null && game.IsStorySession && game.StoryCharacter == Enums.FPname) ? Plugin.Cycles : redsCycles;
            });
        }
    }










    // 用于看完统计数据后回到主界面
    private static void Menu_StoryGameStatisticsScreen_CommunicateWithUpcomingProcess(On.Menu.StoryGameStatisticsScreen.orig_CommunicateWithUpcomingProcess orig, StoryGameStatisticsScreen self, MainLoopProcess nextProcess)
    {
        orig(self, nextProcess);
        if (nextProcess is SlugcatSelectMenu && (RainWorld.lastActiveSaveSlot == Enums.FPname))
        {
            SlugcatSelectMenu menu = nextProcess as SlugcatSelectMenu;
            // 本来他调用的是一个ComingFromRedsStatistics。但我完全可以直接写这里，没必要修改那个函数。
            menu.slugcatPageIndex = menu.indexFromColor(Enums.FPname);
            menu.UpdateSelectedSlugcatInMiscProg();
        }
    }







    // 用来把“继续”改成“数据统计”
    private static void Menu_SlugcatSelectMenu_UpdateStartButtonText(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, SlugcatSelectMenu self)
    {
        orig(self);
        if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == Enums.FPname)
        {
            if (self.saveGameData[Enums.FPname] == null) return;


            bool redsDeath = self.GetSaveGameData(self.slugcatPageIndex).redsDeath;
            bool altEnding = self.GetSaveGameData(self.slugcatPageIndex).altEnding;
            bool ascended = self.GetSaveGameData(self.slugcatPageIndex).ascended;
            int cycles = self.GetSaveGameData(self.slugcatPageIndex).cycle;
            Plugin.Log("MAIN MENU ascended:" + ascended + "  altEnding: " + altEnding + "  redsDeath: " + redsDeath);
            if ((!altEnding && cycles > Plugin.Cycles) || redsDeath || (!altEnding && ascended))
            {
                self.startButton.menuLabel.text = self.Translate("STATISTICS");
            }
            if (self.restartChecked)
            {
                self.startButton.menuLabel.text = self.Translate("DELETE SAVE").Replace(" ", "\r\n");
            }

        }
    }



    // 用于打开统计界面
    private static void Menu_SlugcatSelectMenu_ContinueStartedGame(On.Menu.SlugcatSelectMenu.orig_ContinueStartedGame orig, SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter)
    {
        if (storyGameCharacter == Enums.FPname && self.saveGameData[storyGameCharacter] != null)
        {
            bool redsDeath = self.GetSaveGameData(self.slugcatPageIndex).redsDeath;
            bool altEnding = self.GetSaveGameData(self.slugcatPageIndex).altEnding;
            bool ascended = self.GetSaveGameData(self.slugcatPageIndex).ascended;
            int cycles = self.GetSaveGameData(self.slugcatPageIndex).cycle;
            if ((!altEnding && !ascended && cycles > Plugin.Cycles) || redsDeath)
            {
                self.redSaveState = self.manager.rainWorld.progression.GetOrInitiateSaveState(Enums.FPname, null, self.manager.menuSetup, false);
                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
                self.PlaySound(SoundID.MENU_Switch_Page_Out);
                return;
            }

        }
        orig(self, storyGameCharacter);
    }




}

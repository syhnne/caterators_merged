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


// TODO: 你先别急，让我先急，我想改点东西，玩家时间结束了也可以玩，只不过画面很阴间，约等于没法玩
// TODO: 用roomsettings把监视者投影那个补回来。其实我怀疑补不回来，这玩意儿不是我随便想加就能加的。实在不行的话我记得哪个模组有这个功能来着
// https://github.com/Rain-World-Modding/RegionKit/blob/main/docs/CustomProjections.md
internal class CustomLore
{

    public static MenuScene.SceneID altEndingScene = new MenuScene.SceneID("AltEnding_fp");
    public static SlideShow.SlideShowID altEndingSlideshow = new SlideShow.SlideShowID("Slideshow_AltEnding_fpSlug");
    public static MenuScene.SceneID altEndingSlideshow_scene1 = new MenuScene.SceneID("Slideshow_AltEnding1_fpSlug");




    
    public static void Room_Loaded(Room self)
    {
        if (self.abstractRoom.name == "SS_AI")
        {
            if (self.abstractRoom.firstTimeRealized && self.game.GetStorySession.saveState.cycleNumber == 0
                && !self.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
            {
                Plugin.Log("AddRoomSpecificScript: SS_AI start cutscene");
                self.AddObject(new roomScript.FPstartCutscene(self));
            }
            if (!self.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
            {
                Plugin.Log("AddRoomSpecificScript: SS_AI ending");
                self.AddObject(new roomScript.FPaltEndingCutscene(self));
            }
        }
    }



    public static void SlugcatPage_AddAltEndingImage(SlugcatSelectMenu.SlugcatPage self)
    {
        self.imagePos = new Vector2(683f, 484f);
        self.sceneOffset = default(Vector2);
        self.slugcatDepth = 1f;
        self.sceneOffset.x -= (1366f - self.menu.manager.rainWorld.options.ScreenSize.x) / 2f;
        self.slugcatImage = new InteractiveMenuScene(self.menu, self, altEndingScene);
        self.subObjects.Add(self.slugcatImage);
    }





    public static void SlideShow_ctor(Menu.SlideShow self, ProcessManager manager, Menu.SlideShow.SlideShowID slideShowID)
    {
        if (slideShowID == altEndingSlideshow)
        {
            self.slideShowID = slideShowID;
            self.pages.Add(new Page(self, null, "main", 0));
            self.playList = new List<SlideShow.Scene>();
            if (manager.musicPlayer != null)
            {
                self.waitForMusic = "RW_Outro_Theme_B";
                self.stall = true;
                manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
            }
            self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
            self.playList.Add(new SlideShow.Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_1, self.ConvertTime(0, 1, 20), self.ConvertTime(0, 4, 0), self.ConvertTime(0, 16, 2)));
            self.playList.Add(new SlideShow.Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_3, self.ConvertTime(0, 17, 21), self.ConvertTime(0, 18, 10), self.ConvertTime(0, 32, 2)));
            self.playList.Add(new SlideShow.Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_4, self.ConvertTime(0, 33, 21), self.ConvertTime(0, 34, 10), self.ConvertTime(0, 50, 0)));
            self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(0, 53, 0), self.ConvertTime(0, 53, 0), self.ConvertTime(0, 57, 0)));
            for (int num8 = 1; num8 < self.playList.Count; num8++)
            {
                self.playList[num8].startAt -= 1.1f;
                self.playList[num8].fadeInDoneAt -= 1.1f;
                self.playList[num8].fadeOutStartAt -= 1.1f;
            }
            self.processAfterSlideShow = ProcessManager.ProcessID.Statistics;
            self.preloadedScenes = new SlideShowMenuScene[self.playList.Count];
            for (int i = 0; i < self.preloadedScenes.Length; i++)
            {
                self.preloadedScenes[i] = new SlideShowMenuScene(self, self.pages[0], self.playList[i].sceneID);
                self.preloadedScenes[i].Hide();
            }
            // Plugin.Log("slideshow:", slideShowID, self.current, self.scene);
            self.current = 0;
            manager.RemoveLoadingLabel();
            self.NextScene();

        }
    }






    // 防止玩家在循环耗尽的时候正常睡觉
    // 并且保存数据
    public static void RainWorldGame_Win(RainWorldGame self, bool malnourished)
    {
        SaveState save = self.GetStorySession.saveState;
        self.GetDeathPersistent().CyclesFromLastEnterSSAI++;
        Plugin.Log("RainWorldGame_Win: cycle: " + save.cycleNumber, "CyclesFromLastEnterSSAI:", self.GetDeathPersistent().CyclesFromLastEnterSSAI);

        if (!save.deathPersistentSaveData.altEnding && save.cycleNumber >= Plugin.Cycles)
        {
            Plugin.Log("FPslug Game Over !!! cycle:" + save.cycleNumber);
            save.deathPersistentSaveData.redsDeath = true;
            save.deathPersistentSaveData.ascended = false;
            self.GoToRedsGameOver();
            return;
        }
    }




    public static void RainWorldGame_GoToRedsGameOver(RainWorldGame self)
    {
        Plugin.Log("RainWorldGame_GoToRedsGameOver:");
        Plugin.Log("redsDeath:", self.GetStorySession.saveState.deathPersistentSaveData.redsDeath.ToString());
        Plugin.Log("altEnding:", self.GetStorySession.saveState.deathPersistentSaveData.altEnding.ToString());
        Plugin.Log("ascended:", self.GetStorySession.saveState.deathPersistentSaveData.ascended.ToString());
        // self.manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.redsDeath = true;

        // 怪事。redsDeath死活挂不上，底下那ilhook也没写错，但log是一点反应都没有。剩下两个结局就没毛病。事已至此，只能启动planB了！
        // 我大概想通了，就是那个ilhook挂不上去导致的。但至于ilhook为啥挂不上去，我毫无头绪，整个互联网上没有多少人能告诉我那个match到底要咋写，摆烂吧。

        if (self.GetStorySession.saveState.deathPersistentSaveData.altEnding)
        {
            self.manager.nextSlideshow = altEndingSlideshow;
            self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
            return;
        }

        // if (self.GetStorySession.saveState.deathPersistentSaveData.redsDeath)
        // 能调用到这个函数，不是结局就是死了（大概
        else
        {
            if (self.manager.upcomingProcess != null) return;
            self.manager.musicPlayer?.FadeOutAllSongs(20f);
            if (ModManager.CoopAvailable)
            {
                int num = 0;
                using IEnumerator<Player> enumerator = (from x in self.session.game.Players
                                                        select x.realizedCreature as Player).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Player player = enumerator.Current;
                    self.GetStorySession.saveState.AppendCycleToStatistics(player, self.GetStorySession, true, num);
                    num++;
                }
            }
            else self.GetStorySession.saveState.AppendCycleToStatistics(self.Players[0].realizedCreature as Player, self.GetStorySession, true, 0);

            // 一个阴间小技巧。既然主界面读不到数据，那么就读雨循环吧，读到负数+没结局+没飞升，就是gameover了。
            self.GetStorySession.saveState.SessionEnded(self, true, false);

            self.manager.rainWorld.progression.SaveWorldStateAndProgression(false);
            self.manager.statsAfterCredits = true;
            // 准备加点slideshow，这样玩家才知道自己已经寄啦
            // 算了，不要了 但我得想个办法改一下统计界面的那个图

        }
    }


    public static void RainWorldGame_BeatGameMode(RainWorldGame game, bool standardVoidSea)
    {
        if (standardVoidSea)
        {
            Plugin.Log("Beat Game Mode(void sea ending) : ", (game.GetStorySession.saveState?.ToString()));
            game.GetStorySession.saveState.deathPersistentSaveData.ascended = true;
            // game.rainWorld.progression.miscProgressionData.beaten_（） = true;
            if (ModManager.CoopAvailable)
            {
                int count = 0;
                using IEnumerator<Player> enumerator = (from x in game.GetStorySession.game.Players
                                                        select x.realizedCreature as Player).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Player player = enumerator.Current;
                    game.GetStorySession.saveState.AppendCycleToStatistics(player, game.GetStorySession, true, count);
                    count++;
                }
            }
            else
            {
                game.GetStorySession.saveState.AppendCycleToStatistics(game.Players[0].realizedCreature as Player, game.GetStorySession, true, 0);
            }

            return;
        }

        string roomName = "SS_AI";
        Plugin.Log("Beat Game Mode(alt ending) : ", (game.GetStorySession.saveState?.ToString()));
        // game.rainWorld.progression.miscProgressionData.beaten_（） = true;
        // 下面这个不要了，不出意外的话打完真结局会有类似功能
        // 没事了 那个被我砍了
        game.GetStorySession.saveState.deathPersistentSaveData.karmaCap = 9;
        game.GetStorySession.saveState.deathPersistentSaveData.karma = 9;
        game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
        game.GetStorySession.saveState.BringUpToDate(game);
        AbstractCreature abstractCreature = game.FirstAlivePlayer;
        abstractCreature ??= game.FirstAnyPlayer;
        game.GetStorySession.saveState.AppendCycleToStatistics(abstractCreature.realizedCreature as Player, game.GetStorySession, false, 0);
        RainWorldGame.ForceSaveNewDenLocation(game, roomName, false);
    }











    /////////////////////////////////////////// SPECIFIC /////////////////////////////////////////




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
    // 对于没打真结局的玩家来说，飞升了和死了一样，都不能再点开了
    private static void Menu_SlugcatSelectMenu_ContinueStartedGame(On.Menu.SlugcatSelectMenu.orig_ContinueStartedGame orig, SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter)
    {
        if (storyGameCharacter == Enums.FPname)
        {
            if (self.saveGameData[storyGameCharacter] == null) return;

            bool redsDeath = self.GetSaveGameData(self.slugcatPageIndex).redsDeath;
            bool altEnding = self.GetSaveGameData(self.slugcatPageIndex).altEnding;
            bool ascended = self.GetSaveGameData(self.slugcatPageIndex).ascended;
            int cycles = self.GetSaveGameData(self.slugcatPageIndex).cycle;
            if ((!altEnding && cycles > Plugin.Cycles) || redsDeath || (!altEnding && ascended))
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






















































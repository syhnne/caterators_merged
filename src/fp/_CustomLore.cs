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





    public static void AddRoomSpecificScripts(Room self)
    {
        if (self.abstractRoom.name == "SS_AI")
        {
            if (self.abstractRoom.firstTimeRealized && self.game.GetStorySession.saveState.cycleNumber == 0
                && !self.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
            {
                Plugin.Log("AddRoomSpecificScript: SS_AI start cutscene");
                self.AddObject(new SS_PebblesStartCutscene(self));
            }
            if (!self.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
            {
                Plugin.Log("AddRoomSpecificScript: SS_AI ending");
                self.AddObject(new SS_PebblesAltEnding(self));
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





































// public class OE_GourmandEnding : UpdatableAndDeletable 改的这个
// 这应该是正经结局，好了，那么问题来了，fp猫猫是怎么把自己整活的。回头我得想想，现在的内容是只要进了这个房间且掉在地板上（y<400f？）过几秒就触发结局
internal class SS_PebblesAltEnding : UpdatableAndDeletable
{
    public bool endingTriggered;
    public int endingTriggerTime;
    private Player foundPlayer;
    private bool setController;
    public FadeOut fadeOut;
    private bool doneFinalSave;

    internal SS_PebblesAltEnding(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        // 测试的时候用这个粗略判断一下，防止刚一开始就结束了
        if (!room.game.IsStorySession
            || room.game.StoryCharacter != Enums.FPname
            || !room.game.GetStorySession.saveState.miscWorldSaveData.EverMetMoon) return;

        if (!ModManager.CoopAvailable)
        {
            if (foundPlayer == null && room.game.Players.Count > 0 && room.game.Players[0].realizedCreature != null && room.game.Players[0].realizedCreature.room == room)
            {
                foundPlayer = (room.game.Players[0].realizedCreature as Player);
            }
            if (foundPlayer == null || foundPlayer.inShortcut || room.game.Players[0].realizedCreature.room != room)
            {
                return;
            }
        }
        else
        {
            if (foundPlayer == null && room.PlayersInRoom.Count > 0 && room.PlayersInRoom[0] != null && room.PlayersInRoom[0].room == room)
            {
                foundPlayer = room.PlayersInRoom[0];
            }
            if (foundPlayer == null || foundPlayer.inShortcut || foundPlayer.room != room)
            {
                return;
            }
            room.game.cameras[0].EnterCutsceneMode(foundPlayer.abstractCreature, RoomCamera.CameraCutsceneType.Oracle);
        }
        if (foundPlayer.firstChunk.pos.y < 500f && !setController)
        {
            Plugin.Log("Ending cutscene timer:", endingTriggerTime);
            RainWorld.lockGameTimer = true;
            // 应该没必要控制玩家行为。。
            // setController = true;
            // foundPlayer.controller = new EndingController(this);
        }
        if (foundPlayer.firstChunk.pos.y < 500f && !endingTriggered)
        {
            endingTriggerTime++;
            if (endingTriggerTime > 20)
            {
                endingTriggered = true;
                // 这是不是过场动画？
                room.game.manager.sceneSlot = room.game.StoryCharacter;

                if (fadeOut == null)
                {
                    fadeOut = new FadeOut(room, Color.black, 200f, false);
                    room.AddObject(fadeOut);
                }
            }
        }
        if (fadeOut != null && fadeOut.IsDoneFading() && !doneFinalSave)
        {
            Plugin.Log("fpslugcat Alt Ending !!!");
            // 这句话对我来说没用吧
            room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts = 0;
            // 好吧我想到了。在这里挂altending，还是在那个函数里判断吧
            room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
            room.game.GoToRedsGameOver();
            RainWorldGame.BeatGameMode(room.game, false);
            doneFinalSave = true;
        }




    }


}






// 呃，这应该是开头播放的一段动画之类，总之先空出来。。。真正的难点要开始了
// 搜索：public class DS_RIVSTARTcutscene : UpdatableAndDeletable
public class SS_PebblesStartCutscene : UpdatableAndDeletable
{
    private int timer;
    private new Room room;
    private Player player;


    public SS_PebblesStartCutscene(Room room)
    {
        timer = 0;
        this.room = room;
        if (room.game != null && room.game.Players != null && room.game.Players[0].realizedCreature != null)
        {
            player = room.game.Players[0].realizedCreature as Player;
        }
    }


    public override void Update(bool eu)
    {
        // 如果玩家离开过演算室，就不会再播放动画了
        if (player == null)
        {
            if (room.game != null && room.game.Players != null && room.game.Players[0].realizedCreature != null)
            {
                player = room.game.Players[0].realizedCreature as Player;
            }
        }
        else if (!player.stillInStartShelter) { return; }




        base.Update(eu);

        if (timer == 10)
        {
            Plugin.Log("START CUTSCENE room effects");
            if (room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) == null)
            {
                room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.DarkenLights, 0f, false));
            }
            if (room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) == null)
            {
                room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Darkness, 0f, false));
            }
            if (room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
            {
                room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 0f, false));
            }

            // 找到fp并给他一个初速度，避免玩家一开局看到难绷的画面（
            Oracle oracle = null;
            for (int j = 0; j < room.physicalObjects.Length; j++)
            {
                for (int k = 0; k < room.physicalObjects[j].Count; k++)
                {
                    if (room.physicalObjects[j][k] is Oracle)
                    {
                        oracle = (room.physicalObjects[j][k] as Oracle);
                        break;
                    }
                }
                if (oracle != null)
                {
                    break;
                }
            }
            if (oracle != null && oracle.ID == Oracle.OracleID.SS)
            {
                oracle.firstChunk.vel += Vector2.right;
            }
        }

        AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
        if (room.game.IsStorySession && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room && room.game.GetStorySession.saveState.cycleNumber == 0)
        {
            Player player = firstAlivePlayer.realizedCreature as Player;

            // 不知道这个会不会有bug，碰见问题先把他注释了
            player.objectInStomach = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null, new WorldCoordinate(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID());
            if (timer <= 110)
            {
                player.SuperHardSetPosition(new Vector2(569.7f, 643.5f));
            }
            if (timer == 110)
            {
                Plugin.Log("START CUTSCENE player enter");
                player.mainBodyChunk.vel = new Vector2(0f, -2f);
                player.Stun(60);
            }
        }
        // 怎么播放这个也没声音啊（恼
        if (timer >= 80 && timer < 110)
        {
            room.PlaySound(SoundID.Player_Tick_Along_In_Shortcut, new Vector2(569.7f, 643.5f));
        }
        if (timer == 110)
        {
            room.PlaySound(SoundID.Player_Exit_Shortcut, new Vector2(569.7f, 643.5f));
        }

        if (timer == 180)
        {
            // 屏幕怎么不晃啊（恼
            // 晃啊！tnnd，为什么不晃！！
            for (int i = 0; i < room.game.cameras.Length; i++)
            {
                if (room.game.cameras[i].room == room && !room.game.cameras[i].AboutToSwitchRoom)
                {
                    room.game.cameras[i].ScreenMovement(null, Vector2.zero, 15f);
                }
            }
        }
        if (this.timer > 180 && this.timer < 260 && this.timer % 16 == 0)
        {
            room.ScreenMovement(null, new Vector2(0f, 0f), 2.5f);
            for (int j = 0; j < 6; j++)
            {
                if (Random.value < 0.5f)
                {
                    room.AddObject(new OraclePanicDisplay.PanicIcon(new Vector2((float)Random.Range(230, 740), (float)Random.Range(100, 620))));
                }
            }
        }

        if (timer == 340)
        {
            room.AddObject(new TestSprite(new Vector2(300, 500), 6, 2f));
        }
        if (timer == 350)
        {
            room.AddObject(new TestSprite(new Vector2(300, 360), 12, 1.5f));
        }
        if (timer == 360)
        {
            room.AddObject(new TestSprite(new Vector2(300, 300), 15, 1.5f));
        }


        if (timer == 640)
        {
            room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Press G and up&down arrow keys to adjust the gravity in room."), 140, 500, true, true);
            // Plugin.Log("total time: ", room.game.GetStorySession.saveState.totTime);
        }

        if (timer >= 800)
        {
            Destroy();
            return;
        }
        // Plugin.Log("start cutscene timer: ", timer);
        timer++;
    }



}








public class TestSprite : CosmeticSprite
{

    private bool visible = true;
    private int num;
    private float scale;

    public TestSprite(Vector2 position, int num, float scale)
    {
        pos = position;
        this.num = num;
        this.scale = scale;
    }




    public override void Update(bool eu)
    {
        timer++;

        if (timer >= 190)
        {
            Destroy();
        }
        base.Update(eu);
    }






    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[num];
        FSprite[] glyphs =
        {
            new("BigGlyph0", true),
            new("BigGlyph1", true),
            new("BigGlyph2", true),
            new("BigGlyph3", true),
            new("BigGlyph4", true),
            new("BigGlyph5", true),
            new("BigGlyph6", true),
            new("BigGlyph7", true),
            new("BigGlyph8", true),
            new("BigGlyph9", true),
            new("BigGlyph10", true),
            new("BigGlyph11", true),
            new("BigGlyph12", true)
        };
        System.Random r = new();
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            int randint = r.Next(0, glyphs.Length);
            // Plugin.Log("sprites: ", i, " sp: ", randint);
            sLeaser.sprites[i] = glyphs[randint];
            sLeaser.sprites[i].color = new Color(0f, 0f, 0f);
            sLeaser.sprites[i].isVisible = true;
            sLeaser.sprites[i].scale = scale;

        }
        // 世界未解之谜：为什么有的会显示不出来
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
    }






    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (timer > 160 && timer % 8 < 4)
        {
            visible = false;
        }
        else { visible = true; }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].x = pos.x - camPos.x + (20 * scale * i);
            sLeaser.sprites[i].y = pos.y - camPos.y;
            sLeaser.sprites[i].isVisible = visible;

        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }


    public override void Destroy()
    {
        base.Destroy();
    }


    // Token: 0x040041BE RID: 16830
    public int timer;

    // Token: 0x040041BF RID: 16831
    public float circleScale;
}
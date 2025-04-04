﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using RWCustom;
using MoreSlugcats;
using Fisobs.Core;
using Caterators_by_syhnne.srs;
using Caterators_by_syhnne.nsh;
using EffExt;
using System.Net.Configuration;
using static Caterators_by_syhnne.srs.OxygenMaskModules;
using DevInterface;
using Menu;
using SlugBase;
using JollyCoop;
using JollyCoop.JollyMenu;
using Caterators_by_syhnne.fp;

// TODO: 阿西吧 我还得检查探险模式有没有bug 太难顶了
// 这个mod的大部分工程量都在于防止有人在酒吧里点炒饭。。。（

namespace Caterators_by_syhnne;





// 今天是2025年2月6日，我已经半年没打开过这玩意了，本来只有我和上帝能看懂的代码，现在只有上帝能看懂了

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
class Plugin : BaseUnityPlugin
{

    public const string MOD_ID = "syhnne.caterators";
    public const string MOD_NAME = "Caterators_alpha";
    public const string MOD_VERSION = "0.1.1";

    public static new ManualLogSource Logger { get; internal set; }
    public static ConditionalWeakTable<Player, _public.PlayerModule> playerModules = new ConditionalWeakTable<Player, _public.PlayerModule>();
    public static Plugin instance;
    public Options configOptions;

    internal const bool ShowLogs = true;

    private bool _inited;

    public static uint TickCount = 0;


    #region 暂存区
    // 正常人基本不会在这种地方存数据，但我不是正常人，我是菜狗，让让我吧
    public static readonly int Cycles = 21;
    public static readonly int MaxFood = 8;
    public static readonly int MinFood = 5;
    public int MinFoodNow = MinFood;

    public string[] nshInventoryList = new string[4];
    #endregion

    public void OnEnable()
    {
        try
        {
            Logger = base.Logger;
            instance = this;
            
            // 虽然不知道这个wrapinit到底有什么魔力，但是我删了它之后整个游戏每一行挂了hook的代码都会给我报错，说我没权限访问。。
            // 既然跑起来了，还是不要动了罢
            // 说起来这大概就是整个mod最古老的一行代码了 它从slugbase给的模板就已经存在于此 一直保留到如今。。
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;



            Content.Register(new OxygenMaskModules.OxygenMaskFisob());
            Content.Register(new ReviveSwarmerModules.ReviveSwarmerFisob());
            Content.Register(new moon.MoonSwarmer.MoonSwarmerCritob());


            On.RainWorldGame.Update += RainWorldGame_Update;
            On.RainWorldGame.ctor += RainWorldGame_ctor;


            // On.Menu.SlugcatSelectMenu.ctor += SlugcatSelectMenu_ctor;
            On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
            On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += JollyPlayerSelector_Update;
            On.Menu.SlugcatSelectMenu.RefreshJollySummary += SlugcatSelectMenu_RefreshJollySummary;

            On.World.GetNode += World_GetNode;
            On.RegionState.AdaptRegionStateToWorld += RegionState_AdaptRegionStateToWorld;

            On.MoreSlugcats.SlugNPCAI.WantsToEatThis += SlugNPCAI_WantsToEatThis;


            _public.PlayerHooks.Apply();
            _public.PlayerGraphicsModule.Apply();
            _public.CustomLore.Apply();
            _public.SLOracleHooks.Apply();
            _public.SSRoomEffects.Apply();
            _public.DeathPreventHooks.Apply();
            _public.SSOracleHooks.Apply();
            CustomSaveData.Apply();
            fp.ShelterSS_AI.Apply();
            fp.Daddy.CreatureRelationship.Apply();


        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }




    


    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (!_inited)
        {
            _inited = true;
            try
            {
                // 好好好 设置界面终于复活了
                configOptions = new Options();
                MachineConnector.SetRegisteredOI(Plugin.MOD_ID, configOptions);
                Plugin.Log("syhnne.caterators INIT");
            }
            catch (Exception e)
            {
                Plugin.LogException(e);
            }

        }
    }



    // 在onmodsinit上面读不到slugbase数据，只能写这儿了。这个应该可以保证，在这里加上去的所有函数都在slugbase方法之后执行，所以要是改那个The估计也可以在这改（？
    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        On.Menu.SlugcatSelectMenu.SetSlugcatColorOrder += SlugcatSelectMenu_SetSlugcatColorOrder;
        _public.CustomMenu.Apply();
    }




    private void LoadResources(RainWorld rainWorld)
    {
        Futile.atlasManager.LoadAtlas("atlases/fp_tail_2");
        Futile.atlasManager.LoadAtlas("atlases/fp_head");
        // Futile.atlasManager.LoadAtlas("atlases/fp_arm");
        //Futile.atlasManager.LoadImage("overseerHolograms/PebblesSlugHologram");
        Futile.atlasManager.LoadAtlas("atlases/srs_head");
        Futile.atlasManager.LoadAtlas("atlases/srs_tail");
        Futile.atlasManager.LoadAtlas("atlases/nsh_head");
        Futile.atlasManager.LoadAtlas("atlases/moon_head");
        Futile.atlasManager.LoadAtlas("atlases/moon_dot");
    }



    // 太狼狈了，以后我的日志要寄生在jollylog里面了，因为这个Debug.log不知道为啥压根不干活
    // 不过能输出日志总归是好的……
    public static void Log(params object[] text)
    {
        if (!ShowLogs) return;
        string prefix = "[syhnne.caterators] : ";
        string log = TickCount.ToString() + " | ";
        foreach (object s in text)
        {
            log += s.ToString();
            log += " ";
        }
        Debug.Log(prefix + log);
        JollyCoop.JollyCustom.Log(prefix + log);
        Logger.LogMessage(log);
    }


    /// <summary>
    /// 用这个来输出日志，方便统一管理
    /// </summary>
    /// <param name="ex"></param>
    public static void LogException(Exception ex)
    {
        if (!ShowLogs || Logger == null) return;
        Plugin.Log("An exception has occured!");
        Plugin.Logger.LogError(ex);
    }


















    private static bool SlugNPCAI_WantsToEatThis(On.MoreSlugcats.SlugNPCAI.orig_WantsToEatThis orig, SlugNPCAI self, PhysicalObject obj)
    {
        return orig(self, obj) && obj is not moon.MoonSwarmer.MoonSwarmer;
    }


    private static bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
    {
        return orig(i) || i == Enums.test;
    }



    // 去掉The
    private static void SlugcatSelectMenu_RefreshJollySummary(On.Menu.SlugcatSelectMenu.orig_RefreshJollySummary orig, SlugcatSelectMenu self)
    {
        orig(self);
        for (int i = 0; i < self.playerSummaries.Count; i++)
        {
            SlugcatStats.Name name = JollyCustom.SlugClassMenu(i, self.colorFromIndex(self.slugcatPageIndex));
            if (Enums.IsCaterator(name))
            {
                self.playerSummaries[i].text = self.Translate(SlugcatStats.getSlugcatName(name));
            }
        }
    }


    // 去掉The
    private static void JollyPlayerSelector_Update(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyPlayerSelector self)
    {
        orig(self);
        SlugcatStats.Name name = JollyCustom.SlugClassMenu(self.index, self.dialog.currentSlugcatPageName);
        if (Enums.IsCaterator(name))
        {
            self.classButton.menuLabel.text = self.menu.Translate(SlugcatStats.getSlugcatName(name));
        }
    }




    // 防止选中剧情角色之后又在设置里面关掉devmode引发问题
    private static void SlugcatSelectMenu_ctor(On.Menu.SlugcatSelectMenu.orig_ctor orig, SlugcatSelectMenu self, ProcessManager manager)
    {

        SlugcatStats.Name n = manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
        if (!Options.DevMode.Value && n != null && Enums.IsCaterator(n))
        {
            manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = SlugcatStats.Name.White;
        }

        orig(self, manager);

    }




    // 用来去掉剧情（

    private static void SlugcatSelectMenu_SetSlugcatColorOrder(On.Menu.SlugcatSelectMenu.orig_SetSlugcatColorOrder orig, SlugcatSelectMenu self)
    {
        orig(self);


        List<SlugcatStats.Name> newNames = new();
        foreach (SlugcatStats.Name n in self.slugcatColorOrder)
        {
            if (!Enums.IsCaterator(n) && n != Enums.test) { newNames.Add(n); }
            
        }

        newNames.Add(Enums.FPname);
        newNames.Add(Enums.SRSname);
        newNames.Add(Enums.NSHname);
        newNames.Add(Enums.Moonname);
        if (ShowLogs)
        {
            newNames.Add(Enums.test);
        }


        self.slugcatColorOrder = newNames;


        for (int i = 0; i < self.slugcatColorOrder.Count; i++)
        {
            if (self.slugcatColorOrder[i] == self.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat)
            {
                self.slugcatPageIndex = i;
                return;
            }
        }


        
    }


    

















    private void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        Plugin.TickCount = 0;
    }



    // 一些我的开发者模式专用按键
    private void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        try
        {
            orig(self);
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
            throw;
        }

        if (!self.GamePaused)
        {
            Plugin.TickCount++;
        }


        if (Options.DevMode.Value && self.devToolsActive)
        {
            // 按Y生成一个复活用的神经元
            if (Input.GetKeyDown(KeyCode.Y) && self.Players[0] != null && self.Players[0].realizedObject != null)
            {
                AbstractPhysicalObject abstr = new ReviveSwarmerModules.ReviveSwarmerAbstract(self.world, self.Players[0].pos, self.GetNewID(), true);
                // abstr.destroyOnAbstraction = true;
                self.Players[0].Room.AddEntity(abstr);
                abstr.RealizeInRoom();
                abstr.realizedObject.firstChunk.pos = self.Players[0].realizedObject.firstChunk.pos;
                Plugin.Log("spawn ReviveSwarmer");
            }

            // 按U时停，用于修改roomSettings
            if (Input.GetKeyDown(KeyCode.U))
            {
                self.rivuletEpilogueRainPause = !self.rivuletEpilogueRainPause;
                Plugin.Log("ZA WARUDO");
                Plugin.Log("--rain cycle progression:", self.world.rainCycle.CycleProgression);
            }

            // 按T生成一个月姐的神经元 
            // 求你了 别卡bug了
            if (Input.GetKeyDown(KeyCode.T))
            {
                foreach (AbstractCreature p in self.Players)
                {
                    if (p.realizedObject != null && p.realizedCreature.room != null && Plugin.playerModules.TryGetValue(p.realizedCreature as Player, out var module) && module.swarmerManager != null)
                    {
                        module.swarmerManager.SpawnSwarmer();
                    }
                }
            }

            // 输出所有神经元的位置坐标
            if (Input.GetKeyDown(KeyCode.H))
            {
                foreach (AbstractCreature p in self.Players)
                {
                    if (p.realizedObject != null && p.realizedCreature.room != null && Plugin.playerModules.TryGetValue(p.realizedCreature as Player, out var module) && module.swarmerManager != null)
                    {
                        module.swarmerManager.LogAllSwarmersData();
                    }
                }
            }

            // 传送至玩家
            if (Input.GetKeyDown(KeyCode.J))
            {
                foreach (AbstractCreature p in self.Players)
                {
                    if (p.realizedObject != null && p.realizedCreature.room != null && Plugin.playerModules.TryGetValue(p.realizedCreature as Player, out var module) && module.swarmerManager != null)
                    {
                        module.swarmerManager.tryingToTeleport = true;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                foreach (AbstractCreature p in self.Players)
                {
                    if (p.realizedObject != null && p.realizedCreature.room != null)
                    {
                        Room r = p.realizedCreature.room;
                        string s = " --------- UADs IN ROOM " + r.abstractRoom.name + ": ";
                        foreach (UpdatableAndDeletable uad in r.updateList)
                        {
                            s += uad.ToString() + " | ";
                        }
                        Plugin.Log(s);
                    }
                }
            }
        }


    }






    // 绷不住了，直到最后我也不知道到底是谁在给这个函数传非法参数。
    // 我随便给他返回了一个空的坐标，想着这样我就能从报错信息里知道调用方是谁，结果他不吱声了。
    // 总之他跑起来了，就这样吧
    private AbstractRoomNode World_GetNode(On.World.orig_GetNode orig, World self, WorldCoordinate c)
    {
        // Plugin.Log("GetNode - room nodes:", self.GetAbstractRoom(c.room).nodes.Length, "abstractnode:", c.abstractNode);
        if (c.abstractNode > self.GetAbstractRoom(c.room).nodes.Length || c.abstractNode < 0)
        {
            // Plugin.Log("!!!!!!!!");
            return new AbstractRoomNode();
        }
        return orig(self, c);
    }


    // 无奈出此下策，不知道会不会引发其他问题
    // 似乎没毛病，姑且当它修好了罢
    private static void RegionState_AdaptRegionStateToWorld(On.RegionState.orig_AdaptRegionStateToWorld orig, RegionState self, int playerShelter, int activeGate)
    {
        try
        {
            orig(self, playerShelter, activeGate);
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
        }
    }



}



public static class sCustom
{
    public static bool IsGrabbedBy(this PhysicalObject obj, Creature crit)
    {
        if (obj.grabbedBy.Count <= 0) return false;
        foreach (var g in obj.grabbedBy)
        {
            if (g.grabber != null && g.grabber == crit) return true;
        }
        return false;
    }


    public static bool IsGrabbingAnything(this Creature crit)
    {
        if (crit.grasps.Count() == 0) return false;
        foreach (var g in crit.grasps)
        {
            if (g != null && g.grabbed != null) return true;
        }
        return false;
    }


    // 这个函数应该是存在的罢，为什么我调用不到
    // 那我只能瞎写了，我也不知道这对不对啊
    public static Vector2 SlerpVec(Vector2 a, Vector2 b, float t)
    {
        return Custom.DegToVec(SlerpDeg(a, b, t));
    }

    public static float SlerpDeg(Vector2 a, Vector2 b, float t)
    {
        return Mathf.Lerp(Custom.VecToDeg(a), Custom.VecToDeg(b), Mathf.Clamp01(t));
    }


}
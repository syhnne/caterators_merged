using System;
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

// TODO: 阿西吧 我还得检查探险模式有没有bug 太难顶了
// 这个mod的大部分工程量都在于防止有人在酒吧里点炒饭。。。（

namespace Caterators_by_syhnne;


// TODO: 解决一下食肉猫没法把队友尸体扛回家的问题




// 天呐，好大的工作量啊（倒地不起）

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
class Plugin : BaseUnityPlugin
{

    public const string MOD_ID = "syhnne.caterators";
    public const string MOD_NAME = "Caterators (alpha ver.)";
    public const string MOD_VERSION = "0.1.0";

    public static new ManualLogSource Logger { get; internal set; }
    public static ConditionalWeakTable<Player, _public.PlayerModule> playerModules = new ConditionalWeakTable<Player, _public.PlayerModule>();
    public static Plugin instance;
    public Options configOptions;

    internal const bool ShowLogs = true;

    private bool _inited;


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
            
            On.Menu.SlugcatSelectMenu.ctor += SlugcatSelectMenu_ctor;
            On.World.GetNode += World_GetNode;


            _public.PlayerHooks.Apply();
            _public.PlayerGraphicsModule.Apply();
            _public.CustomLore.Apply();
            _public.SLOracleHooks.Apply();
            _public.SSRoomEffects.Apply();
            _public.DeathPreventHooks.Apply();
            CustomSaveData.Apply();
            fp.ShelterSS_AI.Apply();

            // 鉴于雨世界更新之后报错一声不吭，连日志都不输出了，现把这一项放在最后面充当报错警告。如果点进游戏发现fp复活了，说明前面的部分有问题。
            _public.SSOracleHooks.Apply();

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
        string log = "[syhnne.caterators] : ";
        foreach (object s in text)
        {
            log += s.ToString();
            log += " ";
        }
        Debug.Log(log);
        JollyCoop.JollyCustom.Log(log);
    }


    /// <summary>
    /// 用这个来输出日志，方便统一管理
    /// </summary>
    /// <param name="ex"></param>
    public static void LogException(Exception ex)
    {
        if (!ShowLogs || Logger == null) return;
        Plugin.Logger.LogError(ex);
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

        // 只在开发者模式下解锁剧情
        if (Options.DevMode.Value)
        {
            newNames.Add(Enums.FPname);
            newNames.Add(Enums.SRSname);
            newNames.Add(Enums.NSHname);
            newNames.Add(Enums.Moonname);
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



}




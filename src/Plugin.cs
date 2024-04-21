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

// TODO: 阿西吧 我还得检查探险模式有没有bug 太难顶了
// 这个mod的大部分工程量都在于防止有人在酒吧里点炒饭。。。（

namespace Caterators_by_syhnne;







// 天呐，好大的工作量啊（倒地不起）

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
class Plugin : BaseUnityPlugin
{
    // 破案了，registerOI挂不上是因为我modinfo里面写的跟这个不一样（汗
    // 啊，为什么一样了还是不行？？
    public const string MOD_ID = "syhnne.caterators";
    public const string MOD_NAME = "Caterators (alpha ver.)";
    public const string MOD_VERSION = "0.1.0";

    public static new ManualLogSource Logger { get; internal set; }
    public static ConditionalWeakTable<Player, _public.PlayerModule> playerModules = new ConditionalWeakTable<Player, _public.PlayerModule>();
    public static Plugin instance;
    public ConfigOptions configOptions;

    internal const bool ShowLogs = true;
    internal const bool DevMode = true;


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
            configOptions = new ConfigOptions();
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, configOptions);
            MachineConnector._RefreshOIs();


            Content.Register(new OxygenMaskModules.OxygenMaskFisob());
            Content.Register(new ReviveSwarmerModules.ReviveSwarmerFisob());
            Content.Register(new moon.MoonSwarmer.MoonSwarmerCritob());


            On.RainWorldGame.Update += RainWorldGame_Update;
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
    
    private void LoadResources(RainWorld rainWorld)
    {
        try
        {
            Futile.atlasManager.LoadAtlas("atlases/fp_tail");
            Futile.atlasManager.LoadAtlas("atlases/fp_head");
            Futile.atlasManager.LoadAtlas("atlases/fp_arm");
            Futile.atlasManager.LoadImage("overseerHolograms/PebblesSlugHologram");
            Futile.atlasManager.LoadAtlas("atlases/srs_head");
            Futile.atlasManager.LoadAtlas("atlases/srs_tail");
            Futile.atlasManager.LoadAtlas("atlases/nsh_head");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }



    // 太狼狈了，以后我的日志要寄生在jollylog里面了，因为这个Debug.log不知道为啥压根不干活
    // 不过能输出日志总归是好的……
    public static void Log(params object[] text)
    {
        string log = "[syhnne.caterators] : ";
        foreach (object s in text)
        {
            log += s.ToString();
            log += " ";
        }
        Debug.Log(log);
        JollyCoop.JollyCustom.Log(log);
    }


    public static void LogException(Exception ex)
    {
        if (!ShowLogs || Logger == null) return;
        Plugin.Logger.LogError(ex);
    }





    // 一些我的开发者模式专用按键
    private void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        if (DevMode && self.devToolsActive)
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




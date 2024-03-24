using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Runtime.CompilerServices;
using BepInEx.Logging;

namespace Caterators_merged;







// 天呐，好大的工作量啊（倒地不起）

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "syhnne.caterators";
    public const string MOD_NAME = "Caterators";
    public const string MOD_VERSION = "0.1.0";

    public static new ManualLogSource Logger { get; internal set; }
    public static ConditionalWeakTable<Player, PlayerModule> playerModules = new ConditionalWeakTable<Player, PlayerModule>();
    public static Plugin instance;
    public ConfigOptions option;

    internal static readonly bool ShowLogs = true;
    internal static readonly bool DevMode = true;

    #region player_ctor
    // 以下自定义属性会覆盖slugbase的属性，我的建议是别改，现在json文件里已经没有饱食度数据了，但我不知道有了会导致什么后果
    public static readonly int Cycles = 21;
    public static readonly int MaxFood = 8;
    public static readonly int MinFood = 5;
    public int MinFoodNow = MinFood;
    #endregion

    public void OnEnable()
    {
        instance = this;
        option = new ConfigOptions();
        On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
        MachineConnector.SetRegisteredOI(MOD_ID, option);

        On.Player.ctor += Player_ctor;
        On.World.GetNode += World_GetNode;
        PlayerHooks.Apply();
        PlayerGraphicsModule.Apply();
        CustomLore.Apply();
        SLOracleHooks.Apply();
        SSRoomEffects.Apply();
        fp.ShelterSS_AI.Apply();

        // 鉴于雨世界更新之后报错一声不吭，连日志都不输出了，现把这一项放在最后面充当报错警告。如果点进游戏发现fp复活了，说明前面的部分有问题。
        SSOracleHooks.Apply();
    }
    
    private void LoadResources(RainWorld rainWorld)
    {
        Debug.Log("log output test log output test log output test log output test log output test log output test log output test log output test log output test log output test log output test log output test log output test log output test why isn't it working");
        Futile.atlasManager.LoadAtlas("atlases/fp_tail");
        Futile.atlasManager.LoadAtlas("atlases/fp_head");
        Futile.atlasManager.LoadAtlas("atlases/fp_arm");
        Futile.atlasManager.LoadImage("overseerHolograms/PebblesSlugHologram");
        Futile.atlasManager.LoadAtlas("atlases/srs_head");
        Futile.atlasManager.LoadAtlas("atlases/srs_tail");
        Futile.atlasManager.LoadAtlas("atlases/nsh_head");
    }


    /// <summary>
    /// 输出日志。搜索的时候带上后面的冒号
    /// </summary>
    /// <param name="text"></param>
    public static void Log(params object[] text)
    {
        if (ShowLogs)
        {
            string log = "";
            foreach (object s in text)
            {
                log += s.ToString();
                log += " ";
            }
            Debug.Log("[syhnne.caterators] : " + log);
        }

    }
    /// <summary>
    /// 用来输出一些我暂时用不到，但测试时可能有用的日志，后面没有那个冒号，这样我不想搜索的时候就搜不到
    /// </summary>
    /// <param name="text"></param>
    public static void LogStat(params object[] text)
    {
        if (ShowLogs)
        {
            string log = "";
            foreach (object s in text)
            {
                log += s.ToString();
                log += " ";
            }
            Debug.Log("[syhnne.caterators] " + log);
        }

    }






    #region Player
    private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (Enums.IsCaterator(self.SlugCatClass))
        {
            playerModules.Add(self, new PlayerModule(self));
        }

        if (self.SlugCatClass == Enums.SRSname)
        {
            srs.PlayerHooks.Player_ctor(self, abstractCreature, world);
        }


        // (fp)
        // 这个不能是静态的，只好写这儿了
        if (world.game.IsStorySession && self.slugcatStats.name == Enums.FPname && world.game.StoryCharacter == Enums.FPname)
        {
            int cycle = (world.game.session as StoryGameSession).saveState.cycleNumber;
            bool altEnding = (world.game.session as StoryGameSession).saveState.deathPersistentSaveData.altEnding;
            bool ascended = (world.game.session as StoryGameSession).saveState.deathPersistentSaveData.ascended;

            Plugin.LogStat("Player_ctor - cycle: ", cycle, " altEnding: ", altEnding, "ascended:", ascended);

            MinFoodNow = MinFood;
            self.slugcatStats.maxFood = MaxFood;
            if (self.Malnourished)
            {
                MinFoodNow = MaxFood;
            }
            else if (!altEnding && !ascended)
            {
                MinFoodNow = Math.Min(fp.PlayerHooks.CycleGetFood(cycle), MaxFood);
            }

            if (!altEnding && !ascended && cycle > 5)
            {
                self.redsIllness = new RedsIllness(self, cycle - 5);
            }
            else if (altEnding && !ascended && CustomLore.DPSaveData != null && CustomLore.DPSaveData.CyclesFromLastEnterSSAI > 5)
            {
                self.redsIllness = new RedsIllness(self, CustomLore.DPSaveData.CyclesFromLastEnterSSAI - 5);
            }

            if (!altEnding)
            {
                self.slugcatStats.foodToHibernate = MinFoodNow;
                Plugin.LogStat("Player_ctor - minfoodnow: ", MinFoodNow, "food to hibernate(after): ", self.slugcatStats.foodToHibernate, " maxfood: ", MaxFood);
            }
        }
    }


    #endregion


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




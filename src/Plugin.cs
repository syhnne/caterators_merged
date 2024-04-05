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
using Caterators_by_syhnne.effects;

// 淦 我找不到怎么让vs自动生成我这个新的命名空间 拿这个来检查有没有忘改命名空间的罢
// using Caterators_merged;

namespace Caterators_by_syhnne;







// 天呐，好大的工作量啊（倒地不起）

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "syhnne.caterators";
    public const string MOD_NAME = "Caterators";
    public const string MOD_VERSION = "0.1.1";

    public static new ManualLogSource Logger { get; internal set; }
    public static ConditionalWeakTable<Player, PlayerModule> playerModules = new ConditionalWeakTable<Player, PlayerModule>();
    public static Plugin instance;
    public ConfigOptions option;

    internal const bool ShowLogs = true;
    internal const bool DevMode = true;


    #region 暂存区
    // 正常人基本不会在这种地方存数据，但我不是正常人，我是菜狗，让让我吧
    public static readonly int Cycles = 21;
    public static readonly int MaxFood = 8;
    public static readonly int MinFood = 5;
    public int MinFoodNow = MinFood;

    public List<AbstractPhysicalObject>[] nshInventoryList = new List<AbstractPhysicalObject>[4];
    #endregion

    public void OnEnable()
    {
        try
        {
            Logger = base.Logger;
            instance = this;
            option = new ConfigOptions();
            ConfigOptions.RegisterOI();
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // 我晕……没加下面这句话导致我被物品不生成的bug困扰半个月
            Content.Register(new OxygenMaskModules.OxygenMaskFisob());
            Content.Register(new ReviveSwarmerModules.ReviveSwarmerFisob());


            On.Player.ctor += Player_ctor;
            On.World.GetNode += World_GetNode;
            On.OverWorld.GateRequestsSwitchInitiation += OverWorld_GateRequestsSwitchInitiation;
            On.RainWorldGame.ContinuePaused += RainWorldGame_ContinuePaused;

            PlayerHooks.Apply();
            PlayerGraphicsModule.Apply();
            CustomLore.Apply();
            SLOracleHooks.Apply();
            SSRoomEffects.Apply();
            CustomSaveData.Apply();
            fp.ShelterSS_AI.Apply();
            new EffectDefinitionBuilder("MyRoofTopView_syhnne")
                .AddFloatField("position", 0f, 100f, 1f, 26f, "floorLevel")
                .SetUADFactory((room, data, firstTimeRealized) => new MyRoofTopView(room, data))
                .SetCategory("POMEffectsExamples")
                .Register();

            // 鉴于雨世界更新之后报错一声不吭，连日志都不输出了，现把这一项放在最后面充当报错警告。如果点进游戏发现fp复活了，说明前面的部分有问题。
            SSOracleHooks.Apply();
        }
        catch (Exception ex) 
        { 
            Logger.LogError(ex);
            throw;
        }
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






    #region 需要暂存的函数
    private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (Enums.IsCaterator(self.SlugCatClass))
        {
            playerModules.Add(self, new PlayerModule(self));
        }




        // (fp)
        // 这个不能是静态的，只好写这儿了
        if (world.game.IsStorySession && self.slugcatStats.name == Enums.FPname && world.game.StoryCharacter == Enums.FPname)
        {
            int cycle = (world.game.session as StoryGameSession).saveState.cycleNumber;
            bool altEnding = (world.game.session as StoryGameSession).saveState.deathPersistentSaveData.altEnding;
            bool ascended = (world.game.session as StoryGameSession).saveState.deathPersistentSaveData.ascended;

            Plugin.Log("Player_ctor - cycle: ", cycle, " altEnding: ", altEnding, "ascended:", ascended);

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
            else if (altEnding && !ascended && world.game.GetDeathPersistent().CyclesFromLastEnterSSAI > 5)
            {
                self.redsIllness = new RedsIllness(self, world.game.GetDeathPersistent().CyclesFromLastEnterSSAI - 5);
            }

            if (!altEnding)
            {
                self.slugcatStats.foodToHibernate = MinFoodNow;
                Plugin.Log("Player_ctor - minfoodnow: ", MinFoodNow, "food to hibernate(after): ", self.slugcatStats.foodToHibernate, " maxfood: ", MaxFood);
            }
        }
    }



    private void OverWorld_GateRequestsSwitchInitiation(On.OverWorld.orig_GateRequestsSwitchInitiation orig, OverWorld self, RegionGate reportBackToGate)
    {
        foreach (AbstractCreature player in self.game.Players)
        {
            if (player.realizedCreature != null && Plugin.playerModules.TryGetValue(player.realizedCreature as Player, out var module) && module.nshInventory != null && module.nshInventory.Items != null)
            {
                nshInventoryList[player.ID.number] = module.nshInventory.Items;
                Plugin.Log("nshInventory saved for player", player.ID.number);
            }
        }
        orig(self, reportBackToGate);
    }


    private void RainWorldGame_ContinuePaused(On.RainWorldGame.orig_ContinuePaused orig, RainWorldGame self)
    {
        foreach (AbstractCreature player in self.Players)
        {
            if (player.realizedCreature != null && Plugin.playerModules.TryGetValue(player.realizedCreature as Player, out var module) && module.nshInventory != null && module.nshInventory.Items != null)
            {
                nshInventoryList[player.ID.number] = module.nshInventory.Items;
                Plugin.Log("nshInventory saved for player", player.ID.number);
            }
        }
        orig(self);
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




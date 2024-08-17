using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Mono.Cecil.Cil;
using Random = UnityEngine.Random;
using RWCustom;
using Noise;
using MoreSlugcats;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Drawing.Text;
using BepInEx.Logging;
using Smoke;
using Menu.Remix.MixedUI;
using System.ComponentModel;
using SlugBase.DataTypes;
using SlugBase.SaveData;
using SlugBase;
using System.Runtime.InteropServices;
using static MonoMod.Cil.RuntimeILReferenceBag;
using System.Security.Cryptography;
using Caterators_by_syhnne.srs;
using Caterators_by_syhnne.nsh;
using JollyCoop;
using HUD;

namespace Caterators_by_syhnne._public;

public class PlayerHooks
{

    // 淦我还是觉得这个代码很答辩 又想重写了
    public static void Apply()
    {
        // playerReviver
        /*new Hook(
            typeof(Player).GetProperty(nameof(Player.CanEatMeat), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            get_Player_CanEatMeat
            );
        new Hook(
            typeof(Player).GetProperty(nameof(Player.CanIPutDeadSlugOnBack), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            get_Player_CanIPutDeadSlugOnBack
            );*/
        // On.Player.CanEatMeat += Player_CanEatMeat;
        On.Player.CanIPutDeadSlugOnBack += Player_CanIPutDeadSlugOnBack;
        On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.Update += JollyMeter_PlayerIcon_Update;

        // On.Player.AddFood += Player_AddFood;
        // On.Player.EatMeatUpdate += Player_EatMeatUpdate;
        // On.Player.AddQuarterFood += Player_AddQuarterFood;


        // nshInventory
        On.OverWorld.GateRequestsSwitchInitiation += OverWorld_GateRequestsSwitchInitiation;
        On.RainWorldGame.ContinuePaused += RainWorldGame_ContinuePaused;
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed;
        // On.Player.CanIPickThisUp += Player_CanIPickThisUp;

        // general
        On.Player.Jump += Player_Jump;
        On.Player.ctor += Player_ctor;
        On.Player.Update += Player_Update;
        On.Player.LungUpdate += Player_LungUpdate;
        On.Player.NewRoom += Player_NewRoom;
        // On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
        On.UpdatableAndDeletable.RemoveFromRoom += UpdatableAndDeletable_RemoveFromRoom;


        // PlayerModule
        On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut;
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        On.Player.Die += Player_Die;
        On.Player.Destroy += Player_Destroy;
        On.Player.MovementUpdate += Player_MovementUpdate;
        /*new Hook(
            typeof(PhysicalObject).GetProperty(nameof(PhysicalObject.TotalMass), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            get_PhysicalObject_TotalMass
            );*/
        IL.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdated;


        // 不能吃神经元
        IL.Player.GrabUpdate += IL_Player_GrabUpdate;
        On.Player.BiteEdibleObject += Player_BiteEdibleObject;
        On.Player.CanBeSwallowed += Player_CanBeSwallowed;
        On.Player.ObjectCountsAsFood += Player_ObjectCountsAsFood;
        new Hook(
            typeof(SSOracleSwarmer).GetProperty(nameof(SSOracleSwarmer.Edible), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            SSOracleSwarmer_Edible
            );
        new Hook(
            typeof(SLOracleSwarmer).GetProperty(nameof(SLOracleSwarmer.Edible), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            SLOracleSwarmer_Edible
            );

        fp.PlayerHooks.Apply();
        srs.PlayerHooks.Apply();
        nsh.PlayerHooks.Apply();
        moon.PlayerHooks.Apply();

        
    }




    // 检测多人模式下传给这个函数的参数是不是有问题
    /*private static void Player_AddFood(On.Player.orig_AddFood orig, Player self, int add)
    {
        orig(self, add);
        Plugin.Log("AddFood:", self.SlugCatClass, add);
    }*/



    /*private static void Player_EatMeatUpdate(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex)
    {
        orig(self, graspIndex);

        *//*if (self.playerState.quarterFoodPoints >= 4)
        {
            self.playerState.quarterFoodPoints -= 4;
        }*//*

        Plugin.Log("player_eatmeatUpdate -", self.abstractCreature.ID.number, "qFoodPips:", self.playerState.quarterFoodPoints, "foods:", self.playerState.foodInStomach);
        Plugin.Log("-- player0 - qFoodPips:", (self.abstractCreature.world.game.Players[0].state as PlayerState).quarterFoodPoints, "foods:", (self.abstractCreature.world.game.Players[0].state as PlayerState).foodInStomach);
    }*/


    /*private static void Player_AddQuarterFood(On.Player.orig_AddQuarterFood orig, Player self)
    {
        Plugin.Log("player add quarter food:", self.abstractCreature.ID.number);
        if (ModManager.CoopAvailable && self.abstractCreature.world.game.IsStorySession && self.abstractCreature.world.game.Players[0] != self.abstractCreature && !self.isNPC && self.abstractCreature.world.game.Players[0].realizedCreature != null)
        {
            (self.abstractCreature.world.game.Players[0].realizedCreature as Player).AddQuarterFood();
            return;
        }
        else
        {
            orig(self);
        }

    }*/

















    #region playerReviver

    private static void JollyMeter_PlayerIcon_Update(On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.orig_Update orig, JollyCoop.JollyHUD.JollyMeter.PlayerIcon self)
    {
        orig(self);
        if (!self.playerState.dead && self.dead)
        {
            self.color = PlayerGraphics.SlugcatColor(self.playerState.slugcatCharacter);
            self.iconSprite.RemoveFromContainer();
            self.gradient.RemoveFromContainer();
            self.iconSprite = new FSprite("Kill_Slugcat", true);
            self.meter.fContainer.AddChild(self.iconSprite);
            self.AddGradient(JollyCustom.ColorClamp(self.color, -1f, 360f, 60f, 360f, -1f, 360f));
            self.dead = false;
            self.meter.customFade = 5f;
            self.blink = 3f;
        }
    }









    private static bool Player_CanIPutDeadSlugOnBack(On.Player.orig_CanIPutDeadSlugOnBack orig, Player self, Player pickUpCandidate)
    {
        bool result = orig(self, pickUpCandidate);
        if (Plugin.playerModules.TryGetValue(self, out var mod) && mod.playerReviver != null && mod.playerReviver.Activated)
        {
            result = false;
        }
        return result;

    }


    #endregion







    #region general

    // 用来在香菇模式下（？）修改玩家体重（？）
    // 这已经是我能想到的第二简单的写法了，最简单的那个会卡bug
    private static void Player_GraphicsModuleUpdated(ILContext il)
    {
        // 300 修改读取到的mainBodyChunk.mass
        ILCursor c1 = new(il);
        if (c1.TryGotoNext(MoveType.After,
            i => i.MatchLdfld<BodyChunk>("mass"),
            i => i.Match(OpCodes.Ldarg_0),
            i => i.Match(OpCodes.Call),
            i => i.MatchLdfld<BodyChunk>("mass")
            ))
        {
            c1.Emit(OpCodes.Ldarg_0);
            c1.EmitDelegate<Func<float, Player, float>>((orig, player) =>
            {
                if (Plugin.playerModules.TryGetValue(player, out var mod) && mod.daddy != null)
                {
                    orig += mod.daddy.PlayerExtraMass;
                }
                return orig;
            });
        }
    }





    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        Plugin.playerModules.Add(self, new _public.PlayerModule(self, abstractCreature, world));


        if (self.SlugCatClass == Enums.FPname)
        {
            fp.PlayerHooks.Player_ctor(self, abstractCreature, world);
        }
        else if (self.SlugCatClass == Enums.NSHname)
        {
            nsh.PlayerHooks.Player_ctor(self, abstractCreature, world);
        }





    }



    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        // try catch防止nsh的背包卡bug
        // 好像按太快了就会出问题
        try
        {
            // 谨记，挂在playermodule里面的东西，一律在playermodule那里进行update
            // 由于我刚搬运代码的时候乱写，我两周了才发现gravityController每帧update两次
            bool getModule = Plugin.playerModules.TryGetValue(self, out var module);
            if (getModule)
            {
                module.Update(self, eu);
            }
            orig(self, eu);

            if (self.room == null || self.dead || !getModule || !Enums.IsCaterator(self.SlugCatClass)) return;

            // 防止食肉猫吃队友，只对本模组的4只猫生效
            int grasp = 0;
            if (ModManager.MMF && (self.grasps[0] == null || self.grasps[0].grabbed is not Creature) && self.grasps[1] != null && self.grasps[1].grabbed is Creature)
            {
                grasp = 1;
            }
            if (self.input[0].pckp && self.grasps[grasp] != null && self.grasps[grasp].grabbed is Player)
            {
                self.eatMeat = 0;
                (self.grasps[grasp].grabbed as Player).Template.meatPoints = 0;
            }

            if (self.SlugCatClass == Enums.FPname) 
            { 
                fp.PlayerHooks.Player_Update(self, eu, module.IsMyStory); 
            }
            else if (self.SlugCatClass == Enums.SRSname) 
            { 
                srs.PlayerHooks.Player_Update(self, eu); 
            }

            
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
        }
    }







    private static void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
    {
        orig(self, newRoom);


        if (Plugin.playerModules.TryGetValue(self, out var module))
        {
            module.NewRoom(newRoom);
        }

        if (!newRoom.game.IsStorySession) { return; }

        if (newRoom.abstractRoom.name == "SS_AI" && newRoom.game.StoryCharacter == Enums.FPname)
        {
            newRoom.game.GetDeathPersistent().CyclesFromLastEnterSSAI = 0;
            Plugin.Log("CyclesFromLastEnterSSAI CLEARED");
        }
        else if (newRoom.abstractRoom.name == "SS_AI" && newRoom.game.IsCaterator())
        {
            // TODO: 这里要不要这么写，有待商榷
            // 我操，这是我给什么东西准备的代码啊，竟然连判定都没加就这么在这躺了两个月，，我查了半天ssoraclehooks那边都没找出来为啥我新开的黄猫档刚到fp那里他就认识我还要把我轰出去
            // 这注释也是不说人话，我原来是准备写什么啊（恼）一点都不记得了已经
            if (newRoom.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad <= 0)
            {
                newRoom.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad += 1;
            }
            if (!newRoom.game.GetStorySession.lastEverMetPebbles)
            {
                newRoom.game.GetStorySession.lastEverMetPebbles = true;
            }
        }




        // 我感觉用不着这么写了 写的时候再说吧
        // TODO:
        /*if (module.console != null)
        {
            if (self.room.abstractRoom.name != "SS_AI")
            {
                module.console.isActive = false;
            }
            else if (module.IsMyStory && newRoom.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
            {
                module.console.Enter();
            }
        }*/


        // logs
        if (!Options.DevMode.Value || !newRoom.game.IsStorySession || self.abstractCreature.ID.number != 0) { return; }

        CustomSaveData.SaveDeathPersistent dp = newRoom.game.GetDeathPersistent();
        CustomSaveData.SaveMiscProgression mp = newRoom.game.GetMiscProgression();


        Plugin.Log("  ROOM: ", newRoom.abstractRoom.name, "STORY:", newRoom.game.StoryCharacter);

        Plugin.Log("totalmass:", self.TotalMass);

        // Plugin.Log("--CustomSaveData: CyclesFromLastEnterSSAI", dp.CyclesFromLastEnterSSAI);

        // Plugin.Log("--CustomSaveData:", mp.beaten_fp, mp.beaten_srs, mp.beaten_nsh, mp.beaten_moon);

        /*string warmth = "--IProvideWarmth: ";
        foreach (IProvideWarmth obj in newRoom.blizzardHeatSources)
        {
            warmth += obj.GetType().Name + " ";
        }
        Plugin.Log(warmth);*/

    }


    /*private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
    {
        Room oldRoom = self.room;
        orig(self, entrancePos, carriedByOther);
        if (self is Player && Plugin.playerModules.TryGetValue(self as Player, out var mod) && mod.gravityController != null)
        {
            mod.gravityController.LeaveRoom(oldRoom);
        }
    }*/

    // 经检验，玩家离开房间时终究会调用到这个代码，估计warp之类模组也是一样，所以写在这
    private static void UpdatableAndDeletable_RemoveFromRoom(On.UpdatableAndDeletable.orig_RemoveFromRoom orig, UpdatableAndDeletable self)
    {
        if (self is Player && Plugin.playerModules.TryGetValue(self as Player, out var mod))
        {
            mod.LeaveRoom(self.room);
        }
        orig(self);
    }











    // 不要动这个数，尤其是srs的，动了会导致新地图的难度发生明显变化（
    private static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);
        if (self.SlugCatClass == Enums.FPname || self.SlugCatClass == Enums.test)
        {
            self.jumpBoost *= 1.2f;
        }
        else if (self.SlugCatClass == Enums.NSHname)
        {
            self.jumpBoost *= 1.1f;
        }
        else if (self.SlugCatClass == Enums.Moonname && Plugin.playerModules.TryGetValue(self, out var mod) && mod.swarmerManager != null)
        {
            self.jumpBoost *= mod.swarmerManager.weakMode ? 0.9f : (mod.swarmerManager.hasSwarmers + 7) / 10f;
        }


    }
    #endregion







    #region nshInventory

    // 过业力门时保存背包内物品，防止他消失
    private static void OverWorld_GateRequestsSwitchInitiation(On.OverWorld.orig_GateRequestsSwitchInitiation orig, OverWorld self, RegionGate reportBackToGate)
    {
        foreach (AbstractCreature player in self.game.Players)
        {
            if (player.realizedCreature != null && Plugin.playerModules.TryGetValue(player.realizedCreature as Player, out var module) && module.nshInventory != null && module.nshInventory.Items != null)
            {
                Plugin.instance.nshInventoryList[player.ID.number] = module.nshInventory.SaveToString();
                Plugin.Log("nshInventory saved for player", player.ID.number, "items:", Plugin.instance.nshInventoryList[player.ID.number]);
            }
        }
        orig(self, reportBackToGate);
    }


    // 同上，这是为了防止用warp menu传送的时候物品消失，我怀疑这个可能影响性能，回头加个装没装warp menu的检查（？
    private static void RainWorldGame_ContinuePaused(On.RainWorldGame.orig_ContinuePaused orig, RainWorldGame self)
    {
        foreach (AbstractCreature player in self.Players)
        {
            if (player.realizedCreature != null && Plugin.playerModules.TryGetValue(player.realizedCreature as Player, out var module) && module.nshInventory != null && module.nshInventory.Items != null)
            {
                Plugin.instance.nshInventoryList[player.ID.number] = module.nshInventory.SaveToString();
                Plugin.Log("nshInventory saved for player", player.ID.number, "items:", Plugin.instance.nshInventoryList[player.ID.number]);
            }
        }
        orig(self);
    }





    // 防止退出游戏后继承上一把的背包内容
    // 我想改成存在slugbasedata里。。。
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        Plugin.instance.nshInventoryList = new string[4];
    }




    // 吐出背包里的所有物品
    // 这主要是因为我暂时懒得写存档，但玩家一觉醒来捡东西会比较麻烦
    // 实在懒得写的话，加个背包格数限制就能解决这个问题（你
    private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
    {
        List<PhysicalObject> players = (from x in self.room.physicalObjects.SelectMany((x) => x)
                                        where x is Player
                                        select x).ToList();
        foreach (Player player in players.Cast<Player>())
        {
            if (Plugin.playerModules.TryGetValue(player, out var module) && module.nshInventory != null)
            {
                module.nshInventory.RemoveAndRealizeAllObjects();
            }
        }
        orig(self);

    }





    #endregion










    // 只是为了避免写一些对话而已。实际上好像并不能避免，我防不住雨鹿请联机队友替自己吃神经元（
    /*private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        if (self.slugcatStats.name == Enums.SRSname && obj is SLOracleSwarmer)
        {
            return false;
        }
        return orig(self, obj);
    }*/








    #region PlayerModule

    /*private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
    {
        if (self is Player && Plugin.playerModules.TryGetValue((self as Player), out var mod) && mod.swarmerManager != null) 
        {
            mod.swarmerManager.ForceAllSwarmersIntoShortcut(entrancePos);
        }
        orig(self, entrancePos, carriedByOther);
    }*/



    private static void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        bool stillInShelter = self.stillInStartShelter;

        orig(self, pos, newRoom, spitOutAllSticks);

        bool getModule = Plugin.playerModules.TryGetValue(self, out var module);
        if (getModule && module.srsLightSource != null)
        {
            module.srsLightSource.AddModules();
        }
        if (getModule && module.swarmerManager != null)
        {
            module.swarmerManager.Player_SpitOutOfShortCut(stillInShelter);
        }
    }





    // 一帮模组在这轮番劫持玩家输入……要不还是给搬到playermodule里头去吧
    private static void Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        if (Plugin.playerModules.TryGetValue(self, out var module))
        {
            var newinput = module.PlayerInput(self.input[0].x, self.input[0].y);
            self.input[0].x = newinput.x;
            self.input[0].y = newinput.y;
        }
        orig(self, eu);
    }



    // 好家伙 我都写了这么多东西了
    // 这应该能释放实例罢（思考）
    private static void Player_Destroy(On.Player.orig_Destroy orig, Player self)
    {
        if (Plugin.playerModules.TryGetValue(self, out var module))
        {
            Plugin.playerModules.Remove(self);
        }
        orig(self);
    }




    // 防止你那倒霉的联机队友在你死了之后顶着3倍重力艰难行走。我知道队友有可能也会控制重力，但是我懒得加判断
    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        bool skipDeath = false;
        bool getModule = Plugin.playerModules.TryGetValue(self, out var module);
        if (getModule && module.deathPreventer != null)
        {
            if (module.deathPreventer.justPreventedCounter > 0) { skipDeath = true; }
            else if (module.deathPreventer.TryPreventDeath(PlayerDeathReason.Unknown)) { skipDeath = true; }
        }
        if (skipDeath)
        {
            self.dead = false;
            return;
        }

        orig(self);
        
        if (getModule)
        {
            if (self.room != null) module.gravityController?.LeaveRoom(self.room);
            if (module.deathPreventer != null && !module.deathPreventer.dontRevive)
            {
                module.nshInventory?.RemoveAndRealizeAllObjects();
                module.deathPreventer.dontRevive = false;
            }
            if (module.swarmerManager != null && module.swarmerManager.LastAliveSwarmer != null)
            {
                module.swarmerManager.KillSwarmer(module.swarmerManager.LastAliveSwarmer, false);
            }
        }
    }




    private static void InitPlayerHud(HUD.HUD self, PlayerModule module)
    {
        if (module.gravityController != null)
        {
            self.AddPart(new fp.GravityMeter_v2(self, self.fContainers[1], module.gravityController));
        }
        if (module.nshInventory != null)
        {
            InventoryHUD inventoryHUD = new InventoryHUD(self, self.fContainers[1], module.nshInventory);
            self.AddPart(inventoryHUD);
            module.nshInventory.hud = inventoryHUD;
        }
        if (module.deathPreventer != null)
        {
            self.AddPart(new DeathPreventHUD(self, self.fContainers[1], module.deathPreventer));
        }
        if (module.pearlReader != null)
        {
            self.AddPart(new fp.PearlReaderHUD(self, self.fContainers[1], module.pearlReader));
        }
        if (module.swarmerManager != null)
        {
            moon.MoonSwarmer.SwarmerHUD swarmerHUD = new moon.MoonSwarmer.SwarmerHUD(self, self.fContainers[1], module.swarmerManager);
            self.AddPart(swarmerHUD);
            module.swarmerManager.hud = swarmerHUD;
            swarmerHUD.UpdateIcons();
            if (Options.DevMode.Value)
            {
                self.AddPart(new moon.MoonSwarmer.DebugHUD(self, self.fContainers[1], module.swarmerManager));
            }
        }
    }




    // 已经变成工业生产流程了
    private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);
        

        if (ModManager.CoopAvailable && cam.room.game.session != null && (self.owner as Player).abstractCreature.ID.number == 0)
        {
            for (int i = 0; i < cam.room.game.session.Players.Count; i++)
            {
                if (cam.room.game.session.Players[i].realizedCreature == null || cam.room.game.session.Players[i].realizedCreature is not Player) return;
                Player player = cam.room.game.session.Players[i].realizedCreature as Player;
                if (Plugin.playerModules.TryGetValue(player, out var module2))
                {
                    Plugin.Log("coop - HUD added for player", i);
                    InitPlayerHud(self, module2);
                }
            }
        }
        else if (Plugin.playerModules.TryGetValue(self.owner as Player, out var module1))
        {
            Plugin.Log("single player - HUD added for player", (self.owner as Player).abstractCreature.ID.number);
            /*Plugin.Log("HUD - fContainers:", self.fContainers.Count());
            for (int i = 0; i < self.fContainers.Count(); i++) 
            {
                Plugin.Log("-- container:", i, self.fContainers[i].GetChildCount());
            }*/
            InitPlayerHud(self, module1);
        }

        string str = "- HUDparts:";
        foreach (HudPart hudPart in self.parts)
        {
            str += hudPart.ToString() + " ";
        }
        Plugin.Log(str);



            
        

    }



    // 行吧，不敢这么写了，玩家手上拿东西的时候游戏会直接闪退，没有任何报错信息
    private delegate float orig_get_PhysicalObject_TotalMass(PhysicalObject self);
    private static float get_PhysicalObject_TotalMass(orig_get_PhysicalObject_TotalMass orig, PhysicalObject self)
    {
        var result = orig(self);
        if (self is Player && Plugin.playerModules.TryGetValue(self as Player, out var mod) && mod.daddy != null)
        {
            result += mod.daddy.PlayerExtraMass;
        }
        return result;
    }


    #endregion






    #region OxygenMask

    private static void Player_LungUpdate(On.Player.orig_LungUpdate orig, Player self)
    {
        bool haveOxygenMask = false;
        OxygenMaskModules.OxygenMask mask = null;
        for (int i = 0; i < self.grasps.Length; i++)
        {
            if (self.grasps[i] != null && self.grasps[i].grabbed is OxygenMaskModules.OxygenMask)
            {
                haveOxygenMask = true;
                mask = self.grasps[i].grabbed as OxygenMaskModules.OxygenMask;
                break;
            }
        }
        if (haveOxygenMask && mask != null && mask.count != 1)
        {
            // 没错，按照整数倍提高肺活量的最好办法就是——抽帧！
            // 现在fp也可以拥有比肩水猫的肺活量了，我还是把这个数改小一点罢
        }
        else { orig(self); }

    }



    // 防止氧气面罩被扔出去（虽然fisobs貌似附带了类似的功能，但他不好使啊（汗
    private static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
    {
        if (obj is OxygenMaskModules.OxygenMask)
        {
            return false;
        }
        return orig(self, obj);
    }

    #endregion




    #region 不能吃神经元



    // 好了，现在只有fp不能吃神经元了（。
    private static void IL_Player_GrabUpdate(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // 337末尾，修改神经元的可食用性
        if (c.TryGotoNext(MoveType.After,
            (i) => i.MatchCall<Creature>("get_grasps"),
            (i) => i.Match(OpCodes.Ldloc_S),
            (i) => i.Match(OpCodes.Ldelem_Ref),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Isinst),
            (i) => i.Match(OpCodes.Callvirt)
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, 13);
            c.EmitDelegate<Func<bool, Player, int, bool>>((edible, self, grasp) =>
            {
                if (self.SlugCatClass == Enums.FPname)
                {
                    bool isNotOracleSwarmer = !(self.grasps[grasp].grabbed is OracleSwarmer);
                    return edible && isNotOracleSwarmer;
                }
                else
                {
                    return edible;
                }

            });
        }

        // (fp)

        // TODO: 哈？这代码在我程序里躺了三个月了，合着他有bug？
        // NullReferenceException: Object reference not set to an instance of an object
        // Caterators_by_syhnne.PlayerHooks +<> c.< IL_Player_GrabUpdate > b__2_1(System.Boolean isArtificer, Player self)(at<aab3b65dddfb4301bfff24fdbbdb21cb>:0)
        // MonoMod.Cil.RuntimeILReferenceBag + FastDelegateInvokers.Invoke[T1, T2, TResult](T1 arg1, T2 arg2, MonoMod.Cil.RuntimeILReferenceBag + FastDelegateInvokers + Func`3[T1, T2, TResult] del)(at < 03f8e64dbb9c4841b4665e15d94870d1 >:0)

        // 533末尾，骗代码说我是工匠，让我合成
        ILCursor c2 = new ILCursor(il);
        if (c2.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldc_I4_M1),
            (i) => i.Match(OpCodes.Beq_S),
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldsfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c2.Emit(OpCodes.Ldarg_0);
            c2.EmitDelegate<Func<bool, Player, bool>>((isArtificer, self) =>
            {
                if (self.slugcatStats.name == Enums.FPname)
                {
                    // 这么做是为了防止误触，因为我自己特么的误触好几次了，我想吃东西来着结果吃到刚才用来鲨人的矛，反倒吐了两格
                    if (Plugin.instance.configOptions.CraftKey.Value == KeyCode.None) return true;
                    else if (Input.GetKey(Plugin.instance.configOptions.CraftKey.Value)) return true;
                    else return false;
                }
                else { return isArtificer; }

            });
        }

    }








    private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player self, bool eu)
    {
        if (self.SlugCatClass == Enums.FPname && self.grasps[0] != null && self.grasps[1] != null
            && (self.grasps[0].grabbed is SSOracleSwarmer || self.grasps[0].grabbed is SLOracleSwarmer) && self.grasps[1].grabbed is IPlayerEdible)
        {
            if ((self.grasps[1].grabbed as IPlayerEdible).BitesLeft == 1 && self.SessionRecord != null)
            {
                self.SessionRecord.AddEat(self.grasps[1].grabbed);
            }
            if (self.grasps[1].grabbed is Creature)
            {
                (self.grasps[1].grabbed as Creature).SetKillTag(self.abstractCreature);
            }
            if (self.graphicsModule != null)
            {
                (self.graphicsModule as PlayerGraphics).BiteFly(1);
            }
            (self.grasps[1].grabbed as IPlayerEdible).BitByPlayer(self.grasps[1], eu);
            return;
        }
        else { orig(self, eu); }

    }





    private static bool Player_ObjectCountsAsFood(On.Player.orig_ObjectCountsAsFood orig, Player self, PhysicalObject obj)
    {
        bool result = orig(self, obj);
        if (self.SlugCatClass == Enums.FPname)
        {
            result = result && !(obj is OracleSwarmer);
        }
        return result;
    }



    private delegate bool orig_SLOracleSwarmerEdible(SLOracleSwarmer self);
    private static bool SLOracleSwarmer_Edible(orig_SLOracleSwarmerEdible orig, SLOracleSwarmer self)
    {
        var result = orig(self);
        if (self.grabbedBy.Count > 0 && self.grabbedBy[0] != null && self.grabbedBy[0].grabber is Player && (self.grabbedBy[0].grabber as Player).SlugCatClass == Enums.FPname)
        {
            result = false;
        }
        return result;
    }



    private delegate bool orig_SSOracleSwarmerEdible(SSOracleSwarmer self);
    private static bool SSOracleSwarmer_Edible(orig_SSOracleSwarmerEdible orig, SSOracleSwarmer self)
    {
        var result = orig(self);
        if (self.grabbedBy.Count > 0 && self.grabbedBy[0] != null && self.grabbedBy[0].grabber is Player && (self.grabbedBy[0].grabber as Player).SlugCatClass == Enums.FPname)
        {
            result = false;
        }
        return result;
    }









    private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
    {
        if (self.SlugCatClass == Enums.FPname)
        {
            return (testObj is Rock || testObj is DataPearl || testObj is FlareBomb || testObj is Lantern || testObj is FirecrackerPlant || testObj is VultureGrub && !(testObj as VultureGrub).dead || testObj is Hazer && !(testObj as Hazer).dead && !(testObj as Hazer).hasSprayed || testObj is FlyLure || testObj is ScavengerBomb || testObj is PuffBall || testObj is SporePlant || testObj is BubbleGrass || testObj is OracleSwarmer || testObj is NSHSwarmer || testObj is OverseerCarcass || ModManager.MSC && testObj is FireEgg && self.FoodInStomach >= self.MaxFoodInStomach || ModManager.MSC && testObj is SingularityBomb && !(testObj as SingularityBomb).activateSingularity && !(testObj as SingularityBomb).activateSucktion || testObj is nsh.ReviveSwarmerModules.ReviveSwarmer);
        }
        else { return (orig(self, testObj) || testObj is nsh.ReviveSwarmerModules.ReviveSwarmer); }
    }








    #endregion


}

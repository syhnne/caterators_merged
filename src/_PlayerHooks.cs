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
using Caterators_merged.srs;



namespace Caterators_merged;

public class PlayerHooks
{

    public static void Apply()
    {
        On.Player.Jump += Player_Jump;
        On.Player.Update += Player_Update;
        On.Player.LungUpdate += Player_LungUpdate;
        On.Player.IsObjectThrowable += Player_IsObjectThrowable;
        On.Player.NewRoom += Player_NewRoom;



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
        

    }












    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        bool getModule = Plugin.playerModules.TryGetValue(self, out var module) && Enums.IsCaterator(module.playerName);
        if (getModule)
        {
            module.Update(self, eu);
        }
        orig(self, eu);

        if (self.room == null || self.dead || !getModule || !Enums.IsCaterator(self.SlugCatClass)) return;
        module.gravityController?.Update(eu, module.IsMyStory);

        if (self.SlugCatClass == Enums.FPname) { fp.PlayerHooks.Player_Update(self, eu, module.IsMyStory); }
        else if (self.SlugCatClass == Enums.SRSname) { srs.PlayerHooks.Player_Update(self, eu); }
    }







    private static void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
    {
        orig(self, newRoom);
        bool getModule = Plugin.playerModules.TryGetValue(self, out var module) && Enums.IsCaterator(module.playerName);
        if (self.dead || !getModule || Enums.IsCaterator(self.SlugCatClass)) return;
        module.gravityController?.NewRoom(module.IsMyStory);


        if (newRoom.abstractRoom.name == "SS_AI" && CustomLore.DPSaveData != null && CustomLore.DPSaveData.saveStateNumber == Enums.FPname)
        {
            CustomLore.DPSaveData.CyclesFromLastEnterSSAI = 0;
            Plugin.LogStat("CustomLore.DPSaveData.CyclesFromLastEnterSSAI CLEEARED");
        }

        if (self.room == null) { return; }

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
        if (!Plugin.DevMode) return;

        Plugin.LogStat("ROOM: ", self.room.abstractRoom.name, " SHELTER INDEX: ", self.room.abstractRoom.shelterIndex);
        if (self.room.abstractRoom.isAncientShelter) { Plugin.LogStat("IS ANCIENT SHELTER"); }

        Plugin.Log("CustomLore.DPSaveData.CyclesFromLastEnterSSAI:", CustomLore.DPSaveData.CyclesFromLastEnterSSAI, CustomLore.DPSaveData.saveStateNumber.value);

        Plugin.Log("self.slugcatStats.foodToHibernate:", self.slugcatStats.foodToHibernate);
    }












    private static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);
        if (self.SlugCatClass == Enums.FPname || self.SlugCatClass == Enums.NSHname)
        {
            self.jumpBoost *= 1.2f;
        }
        else if (self.SlugCatClass == Enums.SRSname)
        {
            self.jumpBoost *= 1.1f;
        }
        else if (self.SlugCatClass == Enums.Moonname)
        {
            self.jumpBoost *= 1.3f;
        }

    }





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



    // 修改神经元的可食用性和合成判定
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
                if (Enums.IsCaterator(self.slugcatStats.name))
                {
                    bool isNotOracleSwarmer = !(self.grasps[grasp].grabbed is OracleSwarmer);
                    return (edible && isNotOracleSwarmer);
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
        // Caterators_merged.PlayerHooks +<> c.< IL_Player_GrabUpdate > b__2_1(System.Boolean isArtificer, Player self)(at<aab3b65dddfb4301bfff24fdbbdb21cb>:0)
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
                    if (Plugin.instance.option.CraftKey.Value == KeyCode.None) return true;
                    else if (Input.GetKey(Plugin.instance.option.CraftKey.Value)) return true;
                    else return false;
                }
                else { return isArtificer; }

            });
        }

    }








    private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player self, bool eu)
    {
        if (Enums.IsCaterator(self.slugcatStats.name) && self.grasps[0] != null && self.grasps[1] != null
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
        if (Enums.IsCaterator(self.slugcatStats.name))
        {
            result = result && !(obj is OracleSwarmer);
        }
        return result;
    }



    private delegate bool orig_SLOracleSwarmerEdible(SLOracleSwarmer self);
    private static bool SLOracleSwarmer_Edible(orig_SLOracleSwarmerEdible orig, SLOracleSwarmer self)
    {
        var result = orig(self);
        if (self.grabbedBy.Count > 0 && self.grabbedBy[0] != null && self.grabbedBy[0].grabber is Player && Enums.IsCaterator((self.grabbedBy[0].grabber as Player).slugcatStats.name))
        {
            result = false;
        }
        return result;
    }



    private delegate bool orig_SSOracleSwarmerEdible(SSOracleSwarmer self);
    private static bool SSOracleSwarmer_Edible(orig_SSOracleSwarmerEdible orig, SSOracleSwarmer self)
    {
        var result = orig(self);
        if (self.grabbedBy.Count > 0 && self.grabbedBy[0] != null && self.grabbedBy[0].grabber is Player && Enums.IsCaterator((self.grabbedBy[0].grabber as Player).slugcatStats.name))
        {
            result = false;
        }
        return result;
    }









    private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
    {
        if (Enums.IsCaterator(self.slugcatStats.name))
        {
            return (!ModManager.MSC || !(self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)) && (testObj is Rock || testObj is DataPearl || testObj is FlareBomb || testObj is Lantern || testObj is FirecrackerPlant || (testObj is VultureGrub && !(testObj as VultureGrub).dead) || (testObj is Hazer && !(testObj as Hazer).dead && !(testObj as Hazer).hasSprayed) || testObj is FlyLure || testObj is ScavengerBomb || testObj is PuffBall || testObj is SporePlant || testObj is BubbleGrass || testObj is OracleSwarmer || testObj is NSHSwarmer || testObj is OverseerCarcass || (ModManager.MSC && testObj is FireEgg && self.FoodInStomach >= self.MaxFoodInStomach) || (ModManager.MSC && testObj is SingularityBomb && !(testObj as SingularityBomb).activateSingularity && !(testObj as SingularityBomb).activateSucktion));
        }
        else { return orig(self, testObj); }
    }








    #endregion


}

using MonoMod.Cil;
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





namespace Caterators_merged.fp;

internal class PlayerHooks
{


    public static void Player_Update(Player self, bool eu, bool isMyStory)
    {
        self.redsIllness?.Update();
        if (isMyStory && self.room.abstractRoom.name == "SS_AI" && self.AI == null && !self.dead && !self.Sleeping && self.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
        {
            ShelterSS_AI.Player_Update(self);
        }
    }



    // 纯属复制粘贴游戏代码，只为绕过香菇病效果（
    public static void CustomAddFood(Player player, int add)
    {
        if (player == null) { return; }
        add = Math.Min(add, player.MaxFoodInStomach - player.playerState.foodInStomach);
        if (ModManager.CoopAvailable && player.abstractCreature.world.game.IsStorySession && player.abstractCreature.world.game.Players[0] != player.abstractCreature && !player.isNPC)
        {
            PlayerState playerState = player.abstractCreature.world.game.Players[0].state as PlayerState;
            add = Math.Min(add, Math.Max(player.MaxFoodInStomach - playerState.foodInStomach, 0));
            Plugin.LogStat(string.Format("Player add food {0}. Amount to add {1}", player.playerState.playerNumber, add), false);
            playerState.foodInStomach += add;
        }
        if (player.abstractCreature.world.game.IsStorySession && player.AI == null)
        {
            player.abstractCreature.world.game.GetStorySession.saveState.totFood += add;
        }
        player.playerState.foodInStomach += add;
        if (player.FoodInStomach >= player.MaxFoodInStomach)
        {
            player.playerState.quarterFoodPoints = 0;
        }
        if (player.slugcatStats.malnourished && player.playerState.foodInStomach >= ((player.redsIllness != null) ? player.redsIllness.FoodToBeOkay : player.slugcatStats.maxFood))
        {
            if (player.redsIllness != null)
            {
                Plugin.LogStat("FoodToBeOkay: ", player.redsIllness.FoodToBeOkay);
                player.redsIllness.GetBetter();
                return;
            }
            if (!player.isSlugpup)
            {
                player.SetMalnourished(false);
            }
            if (player.playerState is PlayerNPCState)
            {
                (player.playerState as PlayerNPCState).Malnourished = false;
            }
        }
    }
















    public static void Apply()
    {
        try
        {

            On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;
            On.Creature.Violence += Creature_Violence;
            IL.ZapCoil.Update += IL_ZapCoil_Update;
            IL.Centipede.Shock += IL_Centipede_Shock;

            On.HUD.FoodMeter.SleepUpdate += HUD_FoodMeter_SleepUpdate;
            On.SlugcatStats.SlugcatFoodMeter += SlugcatStats_SlugcatFoodMeter;
            IL.HUD.Map.CycleLabel.UpdateCycleText += IL_HUD_Map_CycleLabel_UpdateCycleText;
            IL.HUD.SubregionTracker.Update += IL_HUD_SubregionTracker_Update;
            IL.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += Menu_SlugcatSelectMenu_SlugcatPageContinue_ctor;
            IL.ProcessManager.CreateValidationLabel += ProcessManager_CreateValidationLabel;

            // 这仨有问题，先不挂了，除了让游戏变难以外没影响
            // TODO: 
            /*new Hook(
                typeof(RedsIllness).GetProperty(nameof(RedsIllness.FoodFac), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                RedsIllness_FoodFac
                );
            new Hook(
                typeof(RedsIllness).GetProperty(nameof(RedsIllness.TimeFactor), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                RedsIllness_TimeFactor
                );
            new Hook(
                typeof(SaveState).GetProperty(nameof(SaveState.SlowFadeIn), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                SaveState_SlowFadeIn
                );*/

        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex);
        }
    }



    // 能进大都会
    private static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
    {
        orig(self);
        if (self.room.abstractRoom.name == "GATE_UW_LC" && self.room.game.IsStorySession && self.room.game.GetStorySession.saveStateNumber == Enums.FPname)
        {
            self.karmaRequirements[0] = RegionGate.GateRequirement.OneKarma;
        }
    }









    // 被电不仅不会死，还会吃饱（？
    // 错误的，线圈全断电了，其实没啥用（。
    private static void IL_ZapCoil_Update(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // 182，还是那个劫持判定
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Stfld),
            (i) => i.MatchLdarg(0),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldloc_1),
            (i) => i.Match(OpCodes.Ldelem_Ref),
            (i) => i.Match(OpCodes.Ldloc_2),
            (i) => i.Match(OpCodes.Callvirt)
            ))
        {
            c.EmitDelegate<Func<PhysicalObject, PhysicalObject>>((physicalObj) =>
            {
                if (physicalObj is Player && (physicalObj as Player).SlugCatClass == Enums.FPname)
                {
                    // 抄的蜈蚣代码
                    (physicalObj as Player).Stun(200);
                    physicalObj.room.AddObject(new CreatureSpasmer(physicalObj as Player, false, (physicalObj as Player).stun));
                    (physicalObj as Player).LoseAllGrasps();
                    if (Plugin.DevMode)
                    {
                        int maxfood = (physicalObj as Player).MaxFoodInStomach;
                        int food = (physicalObj as Player).FoodInStomach;
                        Plugin.Log("Zapcoil - food:" + food + " maxfood: " + maxfood);
                        CustomAddFood(physicalObj as Player, maxfood - food);
                        (physicalObj as Player).AddFood(maxfood - food);
                    }
                    return null;
                }
                else { return physicalObj; }
            });
        }
    }







    private static void IL_Centipede_Shock(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // 226，还是那个劫持判定，修改蜈蚣的体重让他无论如何都会小于玩家体重
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Br),
            (i) => i.Match(OpCodes.Ldarg_1),
            (i) => i.Match(OpCodes.Callvirt),
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<float, PhysicalObject, float>>((centipedeMass, physicalObj) =>
            {
                if (physicalObj is Player && (physicalObj as Player).SlugCatClass == Enums.FPname)
                {
                    CustomAddFood(physicalObj as Player, 1);
                    return 0;
                }
                else { return centipedeMass; }
            });
        }
    }







    private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        if (self is Player && (self as Player).SlugCatClass == Enums.FPname)
        {
            if (type == Creature.DamageType.Electric)
            {

                damage = Mathf.Lerp(1f, 0.1f, self.room.world.rainCycle.RainApproaching) * damage;
                stunBonus = Mathf.Lerp(1f, 0.1f, self.room.world.rainCycle.RainApproaching) * stunBonus;

            }
        }
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }






    #region 香菇病，雨循环倒计时，饱食度

    private delegate float orig_SlowFadeIn(SaveState self);
    private static float SaveState_SlowFadeIn(orig_SlowFadeIn orig, SaveState self)
    {
        var result = orig(self);
        if (self.saveStateNumber == Enums.FPname)
        {
            result = Mathf.Max(self.malnourished ? 4f : 0.8f, (self.cycleNumber >= RedsIllness.RedsCycles(self.redExtraCycles) && !self.deathPersistentSaveData.altEnding && !Custom.rainWorld.ExpeditionMode) ? Custom.LerpMap((float)self.cycleNumber, (float)RedsIllness.RedsCycles(false), (float)(RedsIllness.RedsCycles(false) + 5), 4f, 15f) : 0.8f);
        }
        return result;
    }




    // 从珍珠猫代码里抄的，总之这么写能跑，那就这么写吧（
    private delegate float orig_FoodFac(RedsIllness self);
    private static float RedsIllness_FoodFac(orig_FoodFac orig, RedsIllness self)
    {
        var result = orig(self);
        if (self.player.slugcatStats.name == Enums.FPname)
        {
            result = Mathf.Max(0.2f, 1f / ((float)self.cycle * 0.25f + 2f));
        }
        return result;
    }







    private delegate float orig_TimeFactor(RedsIllness self);
    private static float RedsIllness_TimeFactor(orig_TimeFactor orig, RedsIllness self)
    {
        var result = orig(self);
        if (self.player.slugcatStats.name == Enums.FPname)
        {
            result = 1f - 0.9f * Mathf.Max(Mathf.Max(self.fadeOutSlow ? Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, self.player.abstractCreature.world.game.manager.fadeToBlack), 0.65f) : 0f, Mathf.InverseLerp(40f * Mathf.Lerp(12f, 21f, self.Severity), 40f, (float)self.counter) * Mathf.Lerp(0.2f, 0.5f, self.Severity)), self.CurrentFitIntensity * 0.1f);
        }
        return result;
    }








    private static IntVector2 SlugcatStats_SlugcatFoodMeter(On.SlugcatStats.orig_SlugcatFoodMeter orig, SlugcatStats.Name slugcat)
    {
        return (slugcat == Enums.FPname) ? new IntVector2(Plugin.MaxFood, Plugin.instance.MinFoodNow) : orig(slugcat);
    }






    // 修改游戏界面显示的雨循环倒计时以及饱食度
    private static void Menu_SlugcatSelectMenu_SlugcatPageContinue_ctor(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // 247 修改判定
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Dup),
            (i) => i.Match(OpCodes.Ldc_I4_4),
            (i) => i.Match(OpCodes.Ldarg_S),
            (i) => i.Match(OpCodes.Ldsfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c.Emit(OpCodes.Ldarg, 4);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, SlugcatStats.Name, Menu.SlugcatSelectMenu.SlugcatPageContinue, bool>>((isRed, name, menu) =>
            {
                return isRed || (name == Enums.FPname && !menu.saveGameData.altEnding);
            });
        }

        ILCursor c2 = new ILCursor(il);
        // 189 修改食物条显示
        if (c2.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldarg_S),
            (i) => i.Match(OpCodes.Call),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldarg_S),
            (i) => i.Match(OpCodes.Call),
            (i) => i.Match(OpCodes.Ldfld)
            ))
        {
            c2.Emit(OpCodes.Ldarg, 4);
            c2.Emit(OpCodes.Ldarg_0);
            c2.EmitDelegate<Func<int, SlugcatStats.Name, Menu.SlugcatSelectMenu.SlugcatPageContinue, int>>((foodToHibernate, name, menu) =>
            {
                if (name == Enums.FPname)
                {
                    int cycle = menu.saveGameData.cycle;
                    int result = Plugin.MinFood;
                    if (!menu.saveGameData.altEnding)
                    {
                        result = CycleGetFood(cycle);
                    }
                    return Math.Min(result, Plugin.MaxFood);
                }
                return foodToHibernate;
            });
        }

        ILCursor c3 = new ILCursor(il);
        // 256 修改雨循环显示数字
        if (c3.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Br_S),
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Call),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c3.Emit(OpCodes.Ldarg, 4);
            c3.EmitDelegate<Func<int, SlugcatStats.Name, int>>((redCycles, name) =>
            {
                if (name == Enums.FPname)
                {
                    return Plugin.Cycles;
                }
                return redCycles;
            });
        }
    }







    // 修改游戏内显示的雨循环倒计时
    private static void IL_HUD_SubregionTracker_Update(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // 164 修改是否是红猫的判定
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Isinst),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldsfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c.Emit(OpCodes.Ldloc_0);
            c.EmitDelegate<Func<bool, Player, bool>>((isRed, player) =>
            {
                return isRed || (player.room.game.IsStorySession && player.room.game.GetStorySession.saveState.saveStateNumber == Enums.FPname && !player.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding);
            });
        }

        ILCursor c2 = new ILCursor(il);
        // 175 修改RedsCycles函数返回值 啊 我恨死这个静态函数了
        if (c2.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldloc_0),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Callvirt),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c2.Emit(OpCodes.Ldloc_0);
            c2.EmitDelegate<Func<int, Player, int>>((RedsCycles, player) =>
            {
                if (player.room.game.IsStorySession && player.room.game.GetStorySession.saveStateNumber == Enums.FPname)
                {
                    return Plugin.Cycles;
                }
                return RedsCycles;
            });
        }
    }






    // 不知道这个是干嘛的，但既然搜索搜出来了就改一下罢
    private static void IL_HUD_Map_CycleLabel_UpdateCycleText(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // 23 修改判定
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Isinst),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldsfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c.Emit(OpCodes.Ldloc_0); //Player
            c.EmitDelegate<Func<bool, Player, bool>>((isRed, player) =>
            {
                return isRed || (player.room.game.IsStorySession && player.room.game.GetStorySession.saveStateNumber == Enums.FPname && !player.abstractCreature.world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding);
            });
        }
        ILCursor c2 = new ILCursor(il);
        // 32 改数值
        if (c2.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Callvirt),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Callvirt),
            (i) => i.Match(OpCodes.Callvirt),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c2.Emit(OpCodes.Ldloc_0); //Player
            c2.EmitDelegate<Func<int, Player, int>>((redsCycles, player) =>
            {
                return player.slugcatStats.name == Enums.FPname ? Plugin.Cycles : redsCycles;
            });
        }
    }





    // 修改速通验证的循环数
    private static void ProcessManager_CreateValidationLabel(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // 25 修改判定
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldloc_2),
            (i) => i.Match(OpCodes.Brfalse),
            (i) => i.Match(OpCodes.Ldloc_1),
            (i) => i.Match(OpCodes.Ldsfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c.Emit(OpCodes.Ldloc_1);
            c.Emit(OpCodes.Ldloc_2);
            c.EmitDelegate<Func<bool, SlugcatStats.Name, Menu.SlugcatSelectMenu.SaveGameData, bool>>((isRed, name, saveGameData) =>
            {
                return isRed || (name == Enums.FPname && !saveGameData.altEnding);
            });
        }

        ILCursor c2 = new ILCursor(il);
        // 32 修改Cycles数值
        if (c2.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldloc_2),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Br_S),
            (i) => i.Match(OpCodes.Ldloc_2),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c2.Emit(OpCodes.Ldloc_1);
            c2.EmitDelegate<Func<int, SlugcatStats.Name, int>>((redsCycles, name) =>
            {
                return name == Enums.FPname ? Plugin.Cycles : redsCycles;
            });
        }
    }





    // 在雨眠页面上做个食物条移动动画
    private static void HUD_FoodMeter_SleepUpdate(On.HUD.FoodMeter.orig_SleepUpdate orig, HUD.FoodMeter self)
    {
        if (self.hud.owner is Menu.SleepAndDeathScreen && (self.hud.owner as Menu.KarmaLadderScreen).myGamePackage.saveState.saveStateNumber == Enums.FPname && !(self.hud.owner as Menu.KarmaLadderScreen).myGamePackage.saveState.deathPersistentSaveData.altEnding)
        {
            // 太好了，这个game package里面基本上够用了
            Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package = (self.hud.owner as Menu.KarmaLadderScreen).myGamePackage;
            Menu.SleepAndDeathScreen owner = (self.hud.owner as Menu.SleepAndDeathScreen);
            if (CycleGetFood(package.saveState.cycleNumber - 1) < CycleGetFood(package.saveState.cycleNumber))
            {
                // Plugin.LogStat("HUD_FoodMeter_SleepUpdate - FOOD CHANGING survival limit: ", player.survivalLimit, " start malnourished: ", owner.startMalnourished);
                owner.startMalnourished = true;
                // 强制玩家观看动画。反正占不了他们几秒，但我可是做了一下午，都给我看（
                if (CycleGetFood(package.saveState.cycleNumber) == Plugin.MinFood + 1)
                { owner.forceWatchAnimation = true; }
                self.survivalLimit = CycleGetFood(package.saveState.cycleNumber);

            }
        }
        orig(self);
    }





    public static int CycleGetFood(int cycle)
    {
        int result = Plugin.MinFood + (int)Math.Floor((float)cycle / Plugin.Cycles * (Plugin.MaxFood + 1 - Plugin.MinFood));
        return result;
    }


    #endregion

}

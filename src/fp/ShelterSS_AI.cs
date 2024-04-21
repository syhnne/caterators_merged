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
using Menu.Remix.MixedUI;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlugBase.DataTypes;
using MonoMod.RuntimeDetour;
using System.Reflection;
using SlugBase.SaveData;
using System.Runtime.InteropServices;
using static MonoMod.InlineRT.MonoModRule;
using System.Runtime.CompilerServices;
using IL.Menu;
using HUD;
using JollyCoop;

namespace Caterators_by_syhnne.fp;




// TODO: 这是个很烂的方法，找个良辰吉日重写
// 经过我的思考，重写会引发一个比较麻烦的问题，就是打结局之前怎么让他不能在这睡觉。自动关门这个倒是好解决，不断给noinputcounter赋值为0就行了。
public static class ShelterSS_AI
{


    public static void Apply()
    {
        // On.HUD.FoodMeter.MoveSurvivalLimit += HUD_FoodMeter_MoveSurvivalLimit;
        IL.HUD.FoodMeter.GameUpdate += IL_HUD_Foodmeter_GameUpdate;
        On.HUD.FoodMeter.GameUpdate += HUD_Foodmeter_GameUpdate;
        IL.HUD.KarmaMeter.Draw += IL_HUD_KarmaMeter_Draw;
        // On.HUD.KarmaMeter.Update += HUD_KarmaMeter_Update;

        new Hook(
            typeof(KarmaMeter).GetProperty(nameof(KarmaMeter.Radius), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            KarmaMeter_Radius
            );
    }





    private delegate float orig_Radius(KarmaMeter self);
    private static float KarmaMeter_Radius(orig_Radius orig, KarmaMeter self)
    {
        var result = orig(self);
        if (self.hud.owner is Player && (self.hud.owner as Player).room != null && (self.hud.owner as Player).room.game.IsStorySession && (self.hud.owner as Player).room.game.StoryCharacter == Enums.FPname)
        {
            float forceSleep = (self.hud.owner as Player).FoodInStomach >= self.hud.foodMeter.survivalLimit ? 0f : self.hud.foodMeter.forceSleep;
            result = self.rad + (self.showAsReinforced ? (8f * (1f - Mathf.InverseLerp(0.2f, 0.4f, forceSleep))) : 0f);
        }
        return result;
    }





    private static void HUD_Foodmeter_GameUpdate(On.HUD.FoodMeter.orig_GameUpdate orig, FoodMeter self)
    {
        try
        {
            orig(self);
            // Plugin.Log("HUD_Foodmeter_GameUpdate:", self.survivalLimit);

        }
        catch (Exception e)
        {
            Plugin.LogException(e);
        }

    }





    private static void HUD_KarmaMeter_Update(On.HUD.KarmaMeter.orig_Update orig, KarmaMeter self)
    {
        orig(self);
        Plugin.Log("karmameter showasreinforced:", self.showAsReinforced, "radius:", self.Radius);
    }




    // 防止吃饱了睡觉时有业力花丢失动画
    private static void IL_HUD_KarmaMeter_Draw(ILContext il)
    {
        // 119?
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Callvirt),
            (i) => i.Match(OpCodes.Ldc_R4),
            (i) => i.Match(OpCodes.Ldc_R4),
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Callvirt),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Call)
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, KarmaMeter, float>>((fl, self) =>
            {
                if (self.hud.owner is Player && (self.hud.owner as Player).room != null && (self.hud.owner as Player).room.game.IsStorySession && (self.hud.owner as Player).room.game.StoryCharacter == Enums.FPname)
                {
                    return (self.hud.owner as Player).FoodInStomach >= self.hud.foodMeter.survivalLimit ? 0f : fl;
                }
                return fl;
            });
        }
    }





    // 为了避免强制睡觉的时候显示消耗的食物增多
    private static void IL_HUD_Foodmeter_GameUpdate(ILContext il)
    {
        // 533
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Conv_R4),
            (i) => i.Match(OpCodes.Ldarg_0),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Ldfld),
            (i) => i.Match(OpCodes.Callvirt)
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<int, FoodMeter, int>>((currentFood, self) =>
            {
                if (self.hud.owner is Player && (self.hud.owner as Player).room != null && (self.hud.owner as Player).room.game.IsStorySession && (self.hud.owner as Player).room.game.StoryCharacter == Enums.FPname)
                {
                    // 这个malnourished有问题，他很。。智障，我吃饱了之后，他就变成false了，这个时候就会给我返回5，
                    return Math.Min(currentFood, self.survivalLimit);
                }
                return currentFood;
            });
        }
    }













    // 正常睡觉的第一环节
    public static void Player_Update(Player player)
    {
        // Plugin.Log("sleepCounter: ", player.sleepCounter, "touchedNoInputCounter: ", player.touchedNoInputCounter);


        // 删除了msc判断，反正有依赖
        bool pupFood = true;
        for (int i = 0; i < player.room.game.cameras[0].hud.foodMeter.pupBars.Count; i++)
        {
            FoodMeter foodMeter = player.room.game.cameras[0].hud.foodMeter.pupBars[i];
            if (!foodMeter.PupHasDied && foodMeter.abstractPup.Room == player.room.abstractRoom && (foodMeter.PupInDanger || foodMeter.CurrentPupFood < foodMeter.survivalLimit))
            {
                pupFood = false;
                break;
            }
        }

        bool canSleep = false;
        bool canStarve = false;

        if (pupFood && player.FoodInRoom(player.room, false) >= (player.abstractCreature.world.game.GetStorySession.saveState.malnourished ? player.slugcatStats.maxFood : player.slugcatStats.foodToHibernate))
        { canSleep = true; }
        else if ((!pupFood && player.abstractCreature.world.game.GetStorySession.saveState.malnourished && player.FoodInRoom(player.room, false) >= player.MaxFoodInStomach)
            || (!player.abstractCreature.world.game.GetStorySession.saveState.malnourished && player.FoodInRoom(player.room, false) > 0 && player.FoodInRoom(player.room, false) < player.slugcatStats.foodToHibernate)) // 玩家和猫崽都没饱 || 猫崽没饱
        { canStarve = true; }


        bool starveForceSleep = false;
        if (player.forceSleepCounter > 260)
        {
            Plugin.Log("forceSleep! pupFood: ", pupFood);
            Plugin.Log("food: ", player.FoodInRoom(player.room, false) >= (player.abstractCreature.world.game.GetStorySession.saveState.malnourished ? player.slugcatStats.maxFood : player.slugcatStats.foodToHibernate));
            Plugin.Log("stillInStartShelter:", player.stillInStartShelter);

            // player.forceSleepCounter = 0;

            if (canSleep)
            {
                Plugin.Log("forceSleep! - readyForWin");
                // 第一种情况：吃饱了睡的
                player.readyForWin = true;
            }
            else if (canStarve)
            {
                Plugin.Log("forceSleep! - starve");
                // 第二种情况：没吃饱睡的
                starveForceSleep = true;
            }
        }
        // 同时按住下键和拾取键才能睡觉
        else if ((canSleep || canStarve) && player.input[0].y < 0 && !player.input[0].jmp && !player.input[0].thrw && !player.input[0].pckp
            && player.IsTileSolid(1, 0, -1) && (player.input[0].x == 0 || ((!player.IsTileSolid(1, -1, -1) || !player.IsTileSolid(1, 1, -1)) && player.IsTileSolid(1, player.input[0].x, 0))))
        {
            Plugin.Log("force sleep counter: ", player.forceSleepCounter, " reinforcedKarma: ", player.room.game.GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma);

            player.forceSleepCounter++;
            player.showKarmaFoodRainTime = 40;
        }
        else
        {
            player.forceSleepCounter = 0;
        }





        if (player.Stunned)
        {
            player.readyForWin = false;
        }

        if (player.readyForWin)
        {
            Plugin.Log("readyForWin !!!");
            if (ModManager.CoopAvailable)
            {
                player.ReadyForWinJolly = true;
            }
            ShelterDoorClose(player);
        }
        else if (starveForceSleep)
        {
            Plugin.Log("readyForStarve !!!");
            if (ModManager.CoopAvailable)
            {
                player.ReadyForStarveJolly = true;
            }
            player.sleepCounter = -24;
            ShelterDoorClose(player);
        }
    }







    // 正常睡觉的第二环节
    // 纯属复制游戏代码，我想把他改的更阳间一点，但暂时先别改
    public static void ShelterDoorClose(Player self)
    {
        Plugin.Log("ShelterDoorClose");
        // 只能复制源代码了 改不了一点
        if (ModManager.CoopAvailable)
        {
            List<AbstractCreature> playersToProgressOrWin = self.room.game.PlayersToProgressOrWin;
            List<AbstractCreature> list = (from x in self.room.physicalObjects.SelectMany((List<PhysicalObject> x) => x).OfType<Player>()
                                           select x.abstractCreature).ToList<AbstractCreature>();
            bool flag = true;
            bool flag2 = false;
            bool flag3 = false;
            foreach (AbstractCreature abstractCreature in playersToProgressOrWin)
            {
                if (!list.Contains(abstractCreature))
                {
                    int playerNumber = (abstractCreature.state as PlayerState).playerNumber;
                    flag3 = true;
                    flag = false;
                    flag2 = false;
                    if (self.room.BeingViewed)
                    {
                        try
                        {
                            self.room.game.cameras[0].hud.jollyMeter.playerIcons[playerNumber].blinkRed = 20;
                        }
                        catch
                        {
                        }
                    }
                }
                if (flag3)
                {
                    foreach (Player player in from x in list
                                              select x.realizedCreature as Player)
                    {
                        player.forceSleepCounter = 0;
                        player.sleepCounter = 0;
                        player.touchedNoInputCounter = 0;
                    }
                }
                if (!abstractCreature.state.dead)
                {
                    Player player2 = abstractCreature.realizedCreature as Player;
                    if (!player2.ReadyForWinJolly)
                    {
                        flag = false;
                    }
                    if (player2.ReadyForStarveJolly)
                    {
                        flag2 = true;
                    }
                }
            }
            if (!flag && !flag2)
            {
                return;
            }
        }
        if (!self.room.game.rainWorld.progression.miscProgressionData.regionsVisited.ContainsKey(self.room.world.name))
        {
            self.room.game.rainWorld.progression.miscProgressionData.regionsVisited.Add(self.room.world.name, new List<string>());
        }
        if (!self.room.game.rainWorld.progression.miscProgressionData.regionsVisited[self.room.world.name].Contains(self.room.game.StoryCharacter.value))
        {
            self.room.game.rainWorld.progression.miscProgressionData.regionsVisited[self.room.world.name].Add(self.room.game.StoryCharacter.value);
        }




        bool winGame = true;
        if (ModManager.CoopAvailable)
        {
            List<PhysicalObject> list = (from x in self.room.physicalObjects.SelectMany((List<PhysicalObject> x) => x)
                                         where x is Player
                                         select x).ToList();
            int playerCount = list.Count();
            int foodInRoom = 0;
            int y = SlugcatStats.SlugcatFoodMeter(self.room.game.StoryCharacter).y;
            winGame = (playerCount >= self.room.game.PlayersToProgressOrWin.Count);
            JollyCustom.Log("Player(s) in shelter: " + playerCount.ToString() + " Survived: " + winGame.ToString(), false);
            if (winGame)
            {
                Plugin.Log("jolly wingame");
                foreach (PhysicalObject physicalObject in list)
                {
                    foodInRoom = Math.Max((physicalObject as Player).FoodInRoom(self.room, false), foodInRoom);
                }
                JollyCustom.Log("Survived!, food in room " + foodInRoom.ToString(), false);
                foreach (AbstractCreature abstractCreature in self.room.game.Players)
                {
                    if (abstractCreature.Room != self.room.abstractRoom)
                    {
                        try
                        {
                            JollyCustom.WarpAndRevivePlayer(abstractCreature, self.room.abstractRoom, self.room.LocalCoordinateOfNode(0));
                        }
                        catch (Exception arg)
                        {
                            JollyCustom.Log(string.Format("Could not warp and revive player {0} [{1}]", abstractCreature, arg), false);
                        }
                    }
                }
                self.room.game.Win(foodInRoom < y);
            }
            else
            {
                self.room.game.GoToDeathScreen();
            }
        }
        else
        {
            for (int i = 0; i < self.room.game.Players.Count; i++)
            {
                if (!self.room.game.Players[i].state.alive)
                {
                    winGame = false;
                }
            }
            if (winGame)
            {
                Plugin.Log("single player win");
                self.room.game.Win((self.room.game.Players[0].realizedCreature as Player).FoodInRoom(self.room, false) < (self.room.game.Players[0].realizedCreature as Player).slugcatStats.foodToHibernate);
            }
            else
            {
                self.room.game.GoToDeathScreen();
            }

        }


    }













}


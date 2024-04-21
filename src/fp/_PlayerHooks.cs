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



// 显然，这个文件夹里有些东西是我刚学C#的时候写的

namespace Caterators_by_syhnne.fp;

internal class PlayerHooks
{


    // 我是sb
    public static void Player_ctor(Player self, AbstractCreature abstractCreature, World world)
    {
        if (world.game.IsStorySession && world.game.StoryCharacter == Enums.FPname)
        {
            int cycle = (world.game.session as StoryGameSession).saveState.cycleNumber;
            bool altEnding = (world.game.session as StoryGameSession).saveState.deathPersistentSaveData.altEnding;
            bool ascended = (world.game.session as StoryGameSession).saveState.deathPersistentSaveData.ascended;

            Plugin.Log("Player_ctor - cycle: ", cycle, " altEnding: ", altEnding, "ascended:", ascended);

            Plugin.instance.MinFoodNow = Plugin.MinFood;
            self.slugcatStats.maxFood = Plugin.MaxFood;
            if (self.Malnourished)
            {
                Plugin.instance.MinFoodNow = Plugin.MaxFood;
            }
            else if (!altEnding && !ascended)
            {
                Plugin.instance.MinFoodNow = Math.Min(fp.PlayerHooks.CycleGetFood(cycle), Plugin.MaxFood);
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
                self.slugcatStats.foodToHibernate = Plugin.instance.MinFoodNow;
                Plugin.Log("Player_ctor - minfoodnow: ", Plugin.instance.MinFoodNow, "food to hibernate(after): ", self.slugcatStats.foodToHibernate, " maxfood: ", Plugin.MaxFood);
            }
        }
    }




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
            Plugin.Log(string.Format("Player add food {0}. Amount to add {1}", player.playerState.playerNumber, add), false);
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
                Plugin.Log("FoodToBeOkay: ", player.redsIllness.FoodToBeOkay);
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
            // 合成，二段跳
            On.Player.ClassMechanicsArtificer += Player_ClassMechanicsArtificer;
            On.Player.CraftingResults += Player_CraftingResults;
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            // On.Player.SwallowObject += Player_SwallowObject;
            On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            On.Player.ThrownSpear += Player_ThrownSpear;

            // 其他
            On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;
            
            

            // 香菇病，雨循环倒计时，饱食度
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
            Plugin.LogException(ex);
        }
    }




    #region 合成，二段跳






    // 一矛超人，只要不使用二段跳，就是常驻2倍矛伤。使用二段跳会导致这个伤害发生衰减，最低不低于0.5。修改slugbase的基础矛伤可以使所有的值发生变化
    private static void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
    {
        orig(self, spear);
        if (self.SlugCatClass == Enums.FPname)
        {
            float spearDmgBonus = 1f;
            if (self.pyroJumpCounter > 0)
            {
                spearDmgBonus /= self.pyroJumpCounter;
            }
            spear.spearDamageBonus *= (0.5f + spearDmgBonus);
            Plugin.Log("spearDmgBonus: " + (0.5f + spearDmgBonus) + "  result: " + spear.spearDamageBonus);
        }

    }







    // 学c#的第三天，那时我只会复制粘贴……
    private static void Player_ClassMechanicsArtificer(On.Player.orig_ClassMechanicsArtificer orig, Player self)
    {
        // 卧槽 这代码啥时候写的
        // 这要是没写origself 以前玩炸猫的时候不卡bug吗
        orig(self);
        if (self.SlugCatClass == Enums.FPname)
        {
            Room room = self.room;
            bool flag = self.wantToJump > 0 && self.input[0].pckp;
            bool flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;
            int explosionCapacity = ConfigOptions.ExplosionCapacity.Value;
            int num = Mathf.Max(1, explosionCapacity - 5);
            if (self.pyroJumpCounter > 0 && (self.Consious || self.dead))
            {
                self.pyroJumpCooldown -= 1f;
                if (self.pyroJumpCooldown <= 0f)
                {
                    if (self.pyroJumpCounter >= num)
                    {
                        self.pyroJumpCooldown = 40f;
                    }
                    else
                    {
                        self.pyroJumpCooldown = 60f;
                    }
                    self.pyroJumpCounter--;
                }
            }
            self.pyroParryCooldown -= 1f;
            if (self.pyroJumpCounter >= num)
            {
                if (Random.value < 0.25f)
                {
                    // 这应该是炸多了的冒烟效果
                    self.room.AddObject(new Explosion.ExplosionSmoke(self.mainBodyChunk.pos, Custom.RNV() * 2f * Random.value, 1f));
                }
                if (Random.value < 0.5f)
                {
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
                }
            }

            if (flag
                && !self.pyroJumpped
                && self.canJump <= 0 && !flag2
                && (self.input[0].y >= 0 || (self.input[0].y < 0 && (self.bodyMode == Player.BodyModeIndex.ZeroG || self.gravity <= 0.1f)))
                && self.Consious && self.bodyMode != Player.BodyModeIndex.Crawl
                && self.bodyMode != Player.BodyModeIndex.CorridorClimb
                && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut
                && self.animation != Player.AnimationIndex.HangFromBeam
                && self.animation != Player.AnimationIndex.ClimbOnBeam
                && self.bodyMode != Player.BodyModeIndex.WallClimb
                && self.bodyMode != Player.BodyModeIndex.Swimming
                && self.animation != Player.AnimationIndex.AntlerClimb
                && self.animation != Player.AnimationIndex.VineGrab
                && self.animation != Player.AnimationIndex.ZeroGPoleGrab
                && self.onBack == null)
            {
                self.pyroJumpped = true;
                self.pyroJumpDropLock = 40;
                self.noGrabCounter = 5;
                Vector2 pos = self.firstChunk.pos;
                // 这是正经爆炸效果罢
                // 哦不，这是有烟无伤的二段跳
                // 现在有伤害了，只要你在水里起跳，就会让附近生物触电，只不过没什么伤害


                for (int i = 0; i < 8; i++)
                {
                    Vector2 vector = Custom.DegToVec(360f * Random.value);
                    self.room.AddObject(new MouseSpark(pos + vector * 9f, self.firstChunk.vel + vector * 36f * Random.value, 20f, new Color(0.7f, 1f, 1f)));
                }
                self.room.AddObject(new Explosion.ExplosionLight(pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
                for (int j = 0; j < 10; j++)
                {
                    Vector2 vector = Custom.RNV();
                    self.room.AddObject(new Spark(pos + vector * Random.value * 40f, vector * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
                }
                self.room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, self.firstChunk.pos);
                self.room.InGameNoise(new InGameNoise(pos, 8000f, self, 1f));
                int num2 = Mathf.Max(1, explosionCapacity - 3);
                if (self.Submersion <= 0.5f)
                {
                    self.room.AddObject(new UnderwaterShock(self.room, self, pos, 10, 500f, 0.5f, self, new Color(0.8f, 0.8f, 1f)));
                }
                if (self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity == 0f || self.gravity == 0f)
                {
                    float num3 = (float)self.input[0].x;
                    float num4 = (float)self.input[0].y;
                    while (num3 == 0f && num4 == 0f)
                    {
                        num3 = (float)(((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1));
                        num4 = (float)(((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1));
                    }
                    self.bodyChunks[0].vel.x = 9f * num3;
                    self.bodyChunks[0].vel.y = 9f * num4;
                    self.bodyChunks[1].vel.x = 8f * num3;
                    self.bodyChunks[1].vel.y = 8f * num4;
                    self.pyroJumpCooldown = 150f;
                    self.pyroJumpCounter++;
                }
                else
                {
                    if (self.input[0].x != 0)
                    {
                        self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y, 0f) + 8f;
                        self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y, 0f) + 7f;
                        self.jumpBoost = 6f;
                    }
                    if (self.input[0].x == 0 || self.input[0].y == 1)
                    {
                        if (self.pyroJumpCounter >= num2)
                        {
                            self.bodyChunks[0].vel.y = 16f;
                            self.bodyChunks[1].vel.y = 15f;
                            self.jumpBoost = 10f;
                        }
                        else
                        {
                            self.bodyChunks[0].vel.y = 11f;
                            self.bodyChunks[1].vel.y = 10f;
                            self.jumpBoost = 8f;
                        }
                    }
                    if (self.input[0].y == 1)
                    {
                        self.bodyChunks[0].vel.x = 8f * (float)self.input[0].x;
                        self.bodyChunks[1].vel.x = 6f * (float)self.input[0].x;
                    }
                    else
                    {
                        self.bodyChunks[0].vel.x = 14f * (float)self.input[0].x;
                        self.bodyChunks[1].vel.x = 12f * (float)self.input[0].x;
                    }
                    self.animation = Player.AnimationIndex.Flip;
                    self.pyroJumpCounter++;
                    self.pyroJumpCooldown = 150f;
                    self.bodyMode = Player.BodyModeIndex.Default;
                }
                if (self.pyroJumpCounter >= num2)
                {
                    self.Stun(60 * (self.pyroJumpCounter - (num2 - 1)));
                }
                if (self.pyroJumpCounter >= explosionCapacity)
                {
                    self.room.AddObject(new ShockWave(pos, 200f, 0.2f, 6, false));
                    self.room.AddObject(new Explosion(self.room, self, pos, 7, 350f, 26.2f, 2f, 280f, 0.35f, self, 0.7f, 160f, 1f));
                    self.room.ScreenMovement(new Vector2?(pos), default(Vector2), 1.3f);
                    self.room.InGameNoise(new InGameNoise(pos, 9000f, self, 1f));
                    self.Die();
                }

            }


            else if (flag

                && !flag2
                && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl)
                && (self.canJump > 0 || self.input[0].y < 0) && self.Consious && !self.pyroJumpped && self.pyroParryCooldown <= 0f)
            {
                if (self.canJump <= 0)
                {
                    self.pyroJumpped = true;
                    self.bodyChunks[0].vel.y = 8f;
                    self.bodyChunks[1].vel.y = 6f;
                    self.jumpBoost = 6f;
                    self.forceSleepCounter = 0;
                }
                if (self.pyroJumpCounter <= num)
                {
                    self.pyroJumpCounter += 2;
                }
                else
                {
                    self.pyroJumpCounter++;
                }
                self.pyroParryCooldown = 40f;
                self.pyroJumpCooldown = 150f;

                Vector2 pos2 = self.firstChunk.pos;

                for (int i = 0; i < 8; i++)
                {
                    Vector2 vector3 = Custom.DegToVec(360f * Random.value);
                    self.room.AddObject(new MouseSpark(pos2 + vector3 * 9f, self.firstChunk.vel + vector3 * 36f * Random.value, 20f, new Color(0.7f, 1f, 1f)));
                }


                self.room.AddObject(new Explosion.ExplosionLight(pos2, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));

                for (int l = 0; l < 8; l++)
                {
                    Vector2 vector2 = Custom.RNV();
                    self.room.AddObject(new Spark(pos2 + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
                }
                self.room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6, false));
                self.room.AddObject(new ZapCoil.ZapFlash(pos2, 10f));
                // player.room.PlaySound(SoundID.Flare_Bomb_Burn, pos2);
                // player.room.PlaySound(SoundID.Zapper_Zap, pos2, 1f, 0.2f + 0.25f * Random.value);
                self.room.PlaySound(SoundID.Zapper_Zap, pos2, 1f, 1f + 0.25f * Random.value);
                // player.room.PlaySound(SoundID.Fire_Spear_Explode, pos2, 0.3f + Random.value * 0.3f, 0.5f + Random.value * 2f);
                self.room.InGameNoise(new InGameNoise(pos2, 8000f, self, 1f));

                if (self.room.Darkness(pos2) > 0f)
                {
                    self.room.AddObject(new LightSource(pos2, false, new Color(0.7f, 1f, 1f), self));
                }

                List<Weapon> list = new List<Weapon>();
                for (int m = 0; m < self.room.physicalObjects.Length; m++)
                {
                    for (int n = 0; n < self.room.physicalObjects[m].Count; n++)
                    {
                        if (self.room.physicalObjects[m][n] is Weapon)
                        {
                            Weapon weapon = self.room.physicalObjects[m][n] as Weapon;
                            if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
                            {
                                list.Add(weapon);
                            }
                        }
                        bool flag3;
                        if (ModManager.CoopAvailable && !Custom.rainWorld.options.friendlyFire)
                        {
                            Player player = self.room.physicalObjects[m][n] as Player;
                            flag3 = (player == null || player.isNPC);
                        }
                        else
                        {
                            flag3 = true;
                        }
                        bool flag4 = flag3;
                        if (self.room.physicalObjects[m][n] is Creature && self.room.physicalObjects[m][n] != self && flag4)
                        {
                            Creature creature = self.room.physicalObjects[m][n] as Creature;
                            if (Custom.Dist(pos2, creature.firstChunk.pos) < 200f && (Custom.Dist(pos2, creature.firstChunk.pos) < 60f || self.room.VisualContact(self.abstractCreature.pos, creature.abstractCreature.pos)))
                            {
                                self.room.socialEventRecognizer.WeaponAttack(null, self, creature, true);
                                creature.SetKillTag(self.abstractCreature);
                                if (creature is Scavenger)
                                {
                                    (creature as Scavenger).HeavyStun(80);
                                    creature.Blind(400);
                                }
                                else
                                {
                                    creature.Stun(80);
                                    creature.Blind(400);
                                }
                                creature.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * 30f;
                                if (creature is TentaclePlant)
                                {
                                    for (int num5 = 0; num5 < creature.grasps.Length; num5++)
                                    {
                                        creature.ReleaseGrasp(num5);
                                    }
                                }
                            }
                        }
                    }
                }
                if (self.Submersion <= 0.5f)
                {
                    self.room.AddObject(new UnderwaterShock(self.room, self, pos2, 10, 800f, 2f, self, new Color(0.8f, 0.8f, 1f)));
                }
                if (list.Count > 0 && self.room.game.IsArenaSession)
                {
                    self.room.game.GetArenaGameSession.arenaSitting.players[0].parries++;
                }
                for (int num6 = 0; num6 < list.Count; num6++)
                {
                    list[num6].ChangeMode(Weapon.Mode.Free);
                    list[num6].firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, list[num6].firstChunk.pos)) * 20f;
                    list[num6].SetRandomSpin();
                }
                int num7 = Mathf.Max(1, explosionCapacity - 3);
                if (self.pyroJumpCounter >= num7)
                {
                    self.Stun(60 * (self.pyroJumpCounter - (num7 - 1)));
                }
                if (self.pyroJumpCounter >= explosionCapacity)
                {
                    self.room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6, false));
                    self.room.AddObject(new Explosion(self.room, self, pos2, 7, 350f, 26.2f, 2f, 280f, 0.35f, self, 0.7f, 160f, 1f));
                    self.room.ScreenMovement(new Vector2?(pos2), default(Vector2), 1.3f);
                    self.room.InGameNoise(new InGameNoise(pos2, 9000f, self, 1f));
                    self.Die();
                }
            }


            if (self.canJump > 0
                || !self.Consious
                || self.Stunned
                || self.animation == Player.AnimationIndex.HangFromBeam
                || self.animation == Player.AnimationIndex.ClimbOnBeam
                || self.bodyMode == Player.BodyModeIndex.WallClimb
                || self.animation == Player.AnimationIndex.AntlerClimb
                || self.animation == Player.AnimationIndex.VineGrab
                || self.animation == Player.AnimationIndex.ZeroGPoleGrab
                || self.bodyMode == Player.BodyModeIndex.Swimming
                || ((self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity <= 0.5f || self.gravity <= 0.5f) && (self.wantToJump == 0 || !self.input[0].pckp)))
            {
                self.pyroJumpped = false;
            }
        }
        else { orig(self); }

    }






    private static AbstractPhysicalObject.AbstractObjectType Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
    {
        if (self.SlugCatClass == Enums.FPname)
        {
            if (self.FoodInStomach > 1)
            {
                Creature.Grasp[] grasps = self.grasps;
                for (int i = 0; i < grasps.Length; i++)
                {
                    if (grasps[i] != null && grasps[i].grabbed is IPlayerEdible && (grasps[i].grabbed as IPlayerEdible).Edible)
                    {
                        return null;
                    }
                }
                //要实现的效果：只拦截有电的电矛。没电的电矛、炸矛、普通矛都不拦截
                // 没事了，现在电矛能吃（大雾
                if (grasps[0] != null && grasps[0].grabbed is Spear)
                {
                    return AbstractPhysicalObject.AbstractObjectType.Spear;

                }
                if (grasps[0] == null && grasps[1] != null && grasps[1].grabbed is Spear && self.objectInStomach == null)
                {
                    return AbstractPhysicalObject.AbstractObjectType.Spear;
                }

            }
            else if (self.grasps[0] != null && self.grasps[0].grabbed is ElectricSpear && (self.grasps[0].grabbed as Spear).abstractSpear.electricCharge > 0)
            {
                return AbstractPhysicalObject.AbstractObjectType.Spear;
            }
            else if (self.grasps[0] == null && self.grasps[1] != null && self.grasps[1].grabbed is ElectricSpear && self.objectInStomach == null && (self.grasps[0].grabbed as Spear).abstractSpear.electricCharge > 0)
            {
                return AbstractPhysicalObject.AbstractObjectType.Spear;
            }
            return null;
        }
        else { return orig(self); }
    }






    private static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
    {
        if (self.SlugCatClass == Enums.FPname && (self.CraftingResults() != null))
        {
            return true;
        }
        return orig(self);
    }






    // 下面这个暂时没用，但先留着以防我日后突然想给他加点别的能力
    private static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        if (self.SlugCatClass == Enums.FPname)
        {
            if (grasp < 0 || self.grasps[grasp] == null)
            {
                return;
            }
            AbstractPhysicalObject abstractPhysicalObject = self.grasps[grasp].grabbed.abstractPhysicalObject;
            if (abstractPhysicalObject is AbstractSpear)
            {
                (abstractPhysicalObject as AbstractSpear).stuckInWallCycles = 0;
            }
            self.objectInStomach = abstractPhysicalObject;
            if (ModManager.MMF && self.room.game.IsStorySession)
            {
                (self.room.game.session as StoryGameSession).RemovePersistentTracker(self.objectInStomach);
            }
            self.ReleaseGrasp(grasp);
            self.objectInStomach.realizedObject.RemoveFromRoom();
            self.objectInStomach.Abstractize(self.abstractCreature.pos);
            self.objectInStomach.Room.RemoveEntity(self.objectInStomach);
            if (self.FoodInStomach > 0)
            {
                if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear && !(abstractPhysicalObject as AbstractSpear).explosive && !(abstractPhysicalObject as AbstractSpear).electric)
                {
                    // 这应该是生成矛的代码。那么它为什么不起作用呢（恼
                    abstractPhysicalObject = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), true);
                    // 经测试，生成炸矛和下面的代码无关（1）那么我加这hook有用吗？一会把它删了逝逝
                    self.SubtractFood(1);
                }
            }
            self.objectInStomach = abstractPhysicalObject;
            self.objectInStomach.Abstractize(self.abstractCreature.pos);
            BodyChunk mainBodyChunk = self.mainBodyChunk;
            mainBodyChunk.vel.y = mainBodyChunk.vel.y + 2f;
            self.room.PlaySound(SoundID.Slugcat_Swallow_Item, self.mainBodyChunk);
        }
        else { orig(self, grasp); }
    }











    // 修改合成结果
    private static void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
    {
        if (self.SlugCatClass == Enums.FPname)
        {
            var vector = self.mainBodyChunk.pos;

            // TODO: 我要写一个craftingtutorial，并且单独绑一个变量，因为它内容跟原来的不一样
            self.room.PlaySound(SoundID.Slugcat_Swallow_Item, self.mainBodyChunk);

            for (int i = 0; i < self.grasps.Length; i++)
            {
                if (self.grasps[i] != null)
                {
                    AbstractPhysicalObject grabbed = self.grasps[i].grabbed.abstractPhysicalObject;
                    if (grabbed is AbstractSpear)
                    {
                        AbstractSpear spear = grabbed as AbstractSpear;
                        if (spear.explosive)
                        {
                            ExplosiveSpear explosiveSpear = self.grasps[i].grabbed as ExplosiveSpear;
                            self.room.AddObject(new SootMark(self.room, vector, 50f, false));
                            self.room.AddObject(new Explosion(self.room, explosiveSpear, vector, 5, 50f, 4f, 0.1f, 60f, 0.3f, explosiveSpear.thrownBy, 0.8f, 0f, 0.7f));
                            for (int g = 0; g < 14; g++)
                            {
                                self.room.AddObject(new Explosion.ExplosionSmoke(vector, Custom.RNV() * 5f * Random.value, 1f));
                            }
                            self.room.AddObject(new Explosion.ExplosionLight(vector, 160f, 1f, 3, explosiveSpear.explodeColor));
                            self.room.AddObject(new ExplosionSpikes(self.room, vector, 9, 4f, 5f, 5f, 90f, explosiveSpear.explodeColor));
                            self.room.AddObject(new ShockWave(vector, 60f, 0.045f, 4, false));
                            for (int j = 0; j < 20; j++)
                            {
                                Vector2 vector2 = Custom.RNV();
                                self.room.AddObject(new Spark(vector + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), explosiveSpear.explodeColor, null, 4, 18));
                            }
                            self.room.ScreenMovement(new Vector2?(vector), default(Vector2), 0.7f);
                            for (int k = 0; k < 2; k++)
                            {
                                Smolder smolder = null;
                                if (explosiveSpear.stuckInObject != null)
                                {
                                    smolder = new Smolder(self.room, explosiveSpear.stuckInChunk.pos, explosiveSpear.stuckInChunk, explosiveSpear.stuckInAppendage);
                                }
                                else
                                {
                                    Vector2? vector3 = SharedPhysics.ExactTerrainRayTracePos(self.room, explosiveSpear.firstChunk.pos, explosiveSpear.firstChunk.pos + ((k == 0) ? (explosiveSpear.rotation * 20f) : (Custom.RNV() * 20f)));
                                    if (vector3 != null)
                                    {
                                        smolder = new Smolder(self.room, vector3.Value + Custom.DirVec(vector3.Value, explosiveSpear.firstChunk.pos) * 3f, null, null);
                                    }
                                }
                                if (smolder != null)
                                {
                                    self.room.AddObject(smolder);
                                }
                            }
                            self.Stun(200);
                            explosiveSpear.abstractPhysicalObject.LoseAllStuckObjects();
                            explosiveSpear.room.PlaySound(SoundID.Fire_Spear_Explode, vector);
                            explosiveSpear.room.InGameNoise(new InGameNoise(vector, 8000f, explosiveSpear, 1f));
                            explosiveSpear.Destroy();
                        }
                        // 好了，谁能告诉我拿着有电的电矛的时候吐了两格是什么情况
                        else if (spear.electric && spear.electricCharge > 0)
                        {
                            // 其实电矛是一种用来储存食物的工具（大雾
                            // 绕过香菇病的一种加食物方法
                            Plugin.Log("HOLDING ELECTRIC SPEAR");
                            CustomAddFood(self, spear.electricCharge);
                            spear.electricCharge = 0;
                        }
                        else
                        {
                            self.ReleaseGrasp(i);
                            grabbed.realizedObject.RemoveFromRoom();
                            self.room.abstractRoom.RemoveEntity(grabbed);
                            // 对了，生成矛是这个。。我可能是那个眼瞎。。
                            self.SubtractFood(2);
                            AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false, true);
                            self.room.abstractRoom.AddEntity(abstractSpear);
                            abstractSpear.RealizeInRoom();
                            abstractSpear.electricCharge = 2;
                            if (self.FreeHand() != -1)
                            {
                                self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                            }
                        }
                        return;
                    }
                }
            }
        }
        else { orig(self); }
    }


    #endregion






    #region 其他
    // 能进大都会
    private static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
    {
        orig(self);
        if (self.room.abstractRoom.name == "GATE_UW_LC" && self.room.game.IsStorySession && self.room.game.StoryCharacter == Enums.FPname)
        {
            self.karmaRequirements[0] = RegionGate.GateRequirement.OneKarma;
        }
    }


    






    







    







    
    #endregion





    #region 香菇病，雨循环倒计时，饱食度

    /*
     * 有时间的话我希望能重写一下这堆东西，用miscWorld或者miscprogression来存这个饱食度（前提是menu界面读得到这个
     * 很难想象我刚学会c#的第一个月里是咋想出用循环数当场计算饱食度这种阴间小技巧的
     */

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
            i => i.Match(OpCodes.Dup),
            i => i.Match(OpCodes.Ldc_I4_4),
            i => i.Match(OpCodes.Ldarg_S),
            i => i.Match(OpCodes.Ldsfld),
            i => i.Match(OpCodes.Call)
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
                return isRed || (player.room.game.IsStorySession && player.room.game.StoryCharacter == Enums.FPname && !player.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding);
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
                if (player.room.game.IsStorySession && player.room.game.StoryCharacter == Enums.FPname)
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
                return isRed || (player.room.game.IsStorySession && player.room.game.StoryCharacter == Enums.FPname && !player.abstractCreature.world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding);
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
                // Plugin.Log("HUD_FoodMeter_SleepUpdate - FOOD CHANGING survival limit: ", player.survivalLimit, " start malnourished: ", player.startMalnourished);
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

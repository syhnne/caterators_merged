using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Caterators_by_syhnne.srs;

public static class PlayerHooks
{







    public static void Player_Update(Player self, bool eu)
    {
        // 为了防止玩家发现虚空流体就是水这个事实（。
        WaterUpdate(self, self.room);
        // 体温高的话，热传递效率高，更容易被冻死，很合理吧（你
        // 这个跟肚子里有多少吃的有关，众所周知饿着肚子更容易冷
        if (self.room != null && self.room.blizzard)
        {
            self.Hypothermia += self.HypothermiaExposure * RainWorldGame.DefaultHeatSourceWarmth * 2f * self.room.world.rainCycle.CycleProgression;
        }
        /*if (self.room != null && self.room.blizzard)
        {
            self.Hypothermia += Mathf.Lerp(Mathf.Lerp(self.Malnourished ? 1f : 2.5f, 0.2f, (self.FoodInStomach / self.MaxFoodInStomach)) * RainWorldGame.DefaultHeatSourceWarmth, 0.1f, self.HypothermiaExposure);
        }*/
    }

    




    // 不知道咋写，先凑合一下（
    // 啊啊啊啊啊啊啊啊啊啊啊啊啊我的耳朵！！
    // 嗷 原来是他被调用好几次（
    public static void WaterUpdate(Player player, Room room)
    {
        if (player.dead) { return; }

        if (player.Hypothermia > 5f)
        {
            player.Blink(3);
        }

        // Plugin.Log(player.Hypothermia, player.HypothermiaGain, player.HypothermiaExposure, Mathf.Lerp(1f, 0f, player.Hypothermia));

        for (int i = 0; i < player.grasps.Length; i++)
        {
            if (player.grasps[i] != null && (player.grasps[i].grabbed is OxygenMaskModules.OxygenMask))
            {
                return;
            }
            else if (player.grasps[i] != null && player.grasps[i].grabbed is BubbleGrass)
            {
                BubbleGrass bubbleGrass = player.grasps[i].grabbed as BubbleGrass;
                // Plugin.Log("bubbleGrass oxygen left:", bubbleGrass.AbstrBubbleGrass.oxygenLeft);
                if (player.animation == Player.AnimationIndex.SurfaceSwim)
                {
                    bubbleGrass.AbstrBubbleGrass.oxygenLeft = Mathf.Max(0f, bubbleGrass.AbstrBubbleGrass.oxygenLeft - 0.0009090909f);
                }
                if (bubbleGrass.AbstrBubbleGrass.oxygenLeft > 0f) return;
            }
        }

        // 这玩意太升血压了
        if (!player.Malnourished && player.Submersion > 0.2f && player.room.abstractRoom.name != "SB_L01" && player.room.abstractRoom.name != "FR_FINAL")
        {
            player.Hypothermia += 0.05f * player.Submersion;
            if (player.Hypothermia > 2f)
            {
                player.Hypothermia += 0.05f;
            }
            
        }

        


    }





    public static void PercentageViolence(this Creature crit, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        if (source != null && source.owner is Creature)
        {
            crit.SetKillTag((source.owner as Creature).abstractCreature);
        }
        if (directionAndMomentum != null)
        {
            if (hitChunk != null)
            {
                hitChunk.vel += Vector2.ClampMagnitude(directionAndMomentum.Value / hitChunk.mass, 10f);
            }
            else if (hitAppendage != null && crit is PhysicalObject.IHaveAppendages)
            {
                (crit as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(hitAppendage, directionAndMomentum.Value);
            }
        }
        float num = damage / crit.Template.baseDamageResistance;
        float num2 = (damage * 30f + stunBonus) / crit.Template.baseStunResistance;
        if (crit.State is HealthState)
        {
            num2 *= 1.5f + Mathf.InverseLerp(0.5f, 0f, (crit.State as HealthState).health) * Random.value;
        }
        if (type.Index != -1)
        {
            if (crit.Template.damageRestistances[type.Index, 0] > 0f)
            {
                num /= crit.Template.damageRestistances[type.Index, 0];
            }
            if (crit.Template.damageRestistances[type.Index, 1] > 0f)
            {
                num2 /= crit.Template.damageRestistances[type.Index, 1];
            }
        }
        if (ModManager.MSC)
        {
            if (crit.room != null && crit.room.world.game.IsArenaSession && crit.room.world.game.GetArenaGameSession.chMeta != null && crit.room.world.game.GetArenaGameSession.chMeta.resistMultiplier > 0f && !(crit is Player))
            {
                num /= crit.room.world.game.GetArenaGameSession.chMeta.resistMultiplier;
            }
            if (crit.room != null && crit.room.world.game.IsArenaSession && crit.room.world.game.GetArenaGameSession.chMeta != null && crit.room.world.game.GetArenaGameSession.chMeta.invincibleCreatures && !(crit is Player))
            {
                num = 0f;
            }
        }
        crit.stunDamageType = type;
        crit.Stun((int)num2);
        crit.stunDamageType = Creature.DamageType.None;
        if (crit.State is HealthState)
        {
            (crit.State as HealthState).health -= damage;
            if (crit.Template.quickDeath && (Random.value < -(crit.State as HealthState).health || (crit.State as HealthState).health < -1f || ((crit.State as HealthState).health < 0f && Random.value < 0.33f)))
            {
                crit.Die();
            }
        }
        if (num >= crit.Template.instantDeathDamageLimit)
        {
            crit.Die();
        }
    }




    















    public static void Apply()
    {
        

        On.Player.ClassMechanicsSpearmaster += Player_ClassMechanicsSpearmaster;
        On.Player.Grabability += Player_Grabability;
        On.Spear.Spear_NeedleCanFeed += Spear_NeedleCanFeed;
        On.Spear.HitSomething += Spear_HitSomething;
        On.Spear.HitSomethingWithoutStopping += Spear_HitSomethingWithoutStopping;
        On.Spear.DrawSprites += Spear_DrawSprites;
        On.PlayerGraphics.TailSpeckles.setSpearProgress += TailSpeckles_setSpearProgress;
        On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.Creature.HypothermiaUpdate += IL_Creature_HypothermiaUpdate;
        On.Creature.HypothermiaUpdate += Creature_HypothermiaUpdate;
        IL.Spear.HitSomethingWithoutStopping += IL_Spear_HitSomethingWithoutStopping;


    }








    private static void Creature_HypothermiaUpdate(On.Creature.orig_HypothermiaUpdate orig, Creature self)
    {
        orig(self);
        if (self.room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard && self is Player && (self as Player).SlugCatClass == Enums.SRSname)
        {
            // Plugin.Log("warm room hypothermia update");
            if (self.Hypothermia >= 10f && self.Consious && self.room != null && !self.room.abstractRoom.shelter)
            {
                if (self.HypothermiaStunDelayCounter < 0)
                {
                    int st = (int)Mathf.Lerp(5f, 60f, Mathf.Pow(self.Hypothermia / 2f, 8f));
                    self.HypothermiaStunDelayCounter = (int)Random.Range(300f - self.Hypothermia * 120f, 500f - self.Hypothermia * 100f);
                    self.Stun(st);
                }
            }
            if (self.Hypothermia >= 10f && (float)self.stun > 50f && !self.dead)
            {
                self.Die();
                return;
            }
        }
    }




    // 真纳闷，他为啥闲的没事在这里卡个2f，除了给我添堵以外还有什么用吗
    private static void IL_Creature_HypothermiaUpdate(ILContext il)
    {
        // 444 返回true强制跳过赋值
        ILCursor c1 = new ILCursor(il);
        if (c1.TryGotoNext(MoveType.After,
            i => i.MatchLdarg(0),
            i => i.Match(OpCodes.Callvirt),
            i => i.MatchRet(),
            i => i.MatchLdarg(0),
            i => i.Match(OpCodes.Call)
            ))
        {
            c1.Emit(OpCodes.Ldarg_0);
            c1.EmitDelegate<Func<float, Creature, float>>((orig, creature) =>
            {
                if (creature is Player && (creature as Player).SlugCatClass == Enums.SRSname)
                {
                    orig = 0f;
                }
                return orig;
            });
        }
    }






    // 使蜘蛛远离玩家
    // 我很怀疑，小蜘蛛用的是这个ai吗，他咋不起作用
    private static CreatureTemplate.Relationship IUseARelationshipTracker_UpdateDynamicRelationship(On.BigSpiderAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigSpiderAI self,RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation); 
        if (dRelation.trackerRep.representedCreature.realizedCreature is Player)
        {
            Player player = dRelation.trackerRep.representedCreature.realizedCreature as Player;
            if (player.glowing && player.SlugCatClass == Enums.SRSname)
            {
                result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, player.Malnourished? 0.8f : 0.4f);
            }

            
        }
        return result;
    }



    



    // 借用speartype标记拔出的矛的类型。源代码这个数字不会超过3，而且使用的时候是取的他的余数，所以我可以随便往上加
    private static void TailSpeckles_setSpearProgress(On.PlayerGraphics.TailSpeckles.orig_setSpearProgress orig, PlayerGraphics.TailSpeckles self, float p)
    {
        if (self.pGraphics.player.slugcatStats.name == Enums.SRSname)
        {
            self.spearType = self.pGraphics.player.Malnourished? Random.Range(4, 7) : Random.Range(7, 10);
            self.spearProg = Mathf.Clamp(p, 0f, 1f);
        }
        else { orig(self, p); }
    }




    private static void Spear_DrawSprites(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.IsNeedle && self.spearmasterNeedleType > 3)
        {
            float num = (float)self.spearmasterNeedle_fadecounter / (float)self.spearmasterNeedle_fadecounter_max;
            if (self.spearmasterNeedle_hasConnection)
            {
                num = 1f;
            }
            if (num < 0.01f)
            {
                num = 0.01f;
            }
            if (ModManager.CoopAvailable && self.jollyCustomColor != null)
            {
                sLeaser.sprites[0].color = self.jollyCustomColor.Value;
            }
            else if (PlayerGraphics.CustomColorsEnabled())
            {
                sLeaser.sprites[0].color = Color.Lerp(PlayerGraphics.CustomColorSafety(2), self.color, 1f - num);
            }
            else
            {
                sLeaser.sprites[0].color = Color.Lerp(self.spearmasterNeedleType > 6? PlayerGraphicsModule.spearColor : PlayerGraphicsModule.spearColorDark, self.color, 1f - num);
            }
        }
    }



    private static void Spear_HitSomethingWithoutStopping(On.Spear.orig_HitSomethingWithoutStopping orig, Spear self, PhysicalObject obj, BodyChunk chunk, PhysicalObject.Appendage appendage)
    {
        if (obj != null && self.thrownBy is Player && (self.thrownBy as Player).slugcatStats.name == Enums.SRSname
            && obj is OracleSwarmer)
        {
            return;
        }
        orig(self, obj, chunk, appendage);
    }




    // 嗯。。
    private static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {

        bool die = false;
        float dmg = 0f;
        if (result.obj != null && result.obj is Creature && !(result.obj as Creature).dead
            && self.thrownBy is Player && (self.thrownBy as Player).slugcatStats.name == Enums.SRSname
            && self.Spear_NeedleCanFeed() && self.spearmasterNeedleType > 6
            && (result.obj as Creature).State is HealthState
            && (result.obj as Creature).SpearStick(self, 0.1f, result.chunk, result.onAppendagePos, self.firstChunk.vel))
        {
            die = true;
            dmg = ((result.obj as Creature).State as HealthState).h;
        }

        // 写完这些代码大概两周以后，我才想起来他有嘴，吃爆米花不用只吃五个。。
        if (self.Spear_NeedleCanFeed() && result.obj is SeedCob
            && self.thrownBy is Player && (self.thrownBy as Player).slugcatStats.name == Enums.SRSname)
        {
            (self.thrownBy as Player).AddFood(9);
        }

        bool res = orig(self, result, eu);



        if (die)
        {
            
            Plugin.Log("orig h:", dmg, "crit:", result.obj);
            // 物理伤害+2f火伤+10%火伤（大雾
            // 没错 就是针对香菇设计的
            (result.obj as Creature).PercentageViolence(self.firstChunk, new Vector2?(self.firstChunk.vel * self.firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.None, 0.1f, 20f);

            (result.obj as Creature).Violence(self.firstChunk, new Vector2?(self.firstChunk.vel * self.firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.None, 2f, 0f);

            /*(result.obj as Creature).Die();
            (result.obj as Creature).SetKillTag(self.thrownBy.abstractCreature);*/
        }
        return res;


    }




    private static void IL_Spear_HitSomethingWithoutStopping(ILContext il)
    {
        // 160 如果是srs的矛就返回false
        ILCursor c1 = new ILCursor(il);
        if (c1.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Blt),
            i => i.MatchLdarg(1),
            i => i.MatchCallvirt<UpdatableAndDeletable>("Destroy"),
            i => i.MatchLdarg(1),
            i => i.MatchIsinst<OracleSwarmer>()
            ))
        {
            // TODO: 
            /*c1.Emit(OpCodes.Ldarg_0);
            c1.EmitDelegate<Func<bool, Spear, bool>>((orig, spear) =>
            {
                if (spear.spearmasterNeedleType > 3) return false;
                return orig;
            });*/
        }
    }




    private static bool Spear_NeedleCanFeed(On.Spear.orig_Spear_NeedleCanFeed orig, Spear self)
    {
        if (self.thrownBy != null && self.thrownBy is Player && (self.thrownBy as Player).slugcatStats.name == Enums.SRSname && self.spearmasterNeedle && self.spearmasterNeedle_hasConnection)
        {
            return true;
        }
        return orig(self);
    }





    // 双持
    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (self.slugcatStats.name == Enums.SRSname && obj is Weapon)
        {
            return Player.ObjectGrabability.OneHand;
        }
        else { return orig(self, obj); }
    }





    // 挂这里不是因为需要挂这里，而是因为这个比较好认（？
    private static void Player_ClassMechanicsSpearmaster(On.Player.orig_ClassMechanicsSpearmaster orig, Player self)
    {
        orig(self);
        if (self.slugcatStats.name != Enums.SRSname) return;


        if ((self.graphicsModule as PlayerGraphics).tailSpecks == null)
        {
            Plugin.Log("tailSpecks not found !!!");
            return;
        }

        if (!Plugin.playerModules.TryGetValue(self, out var module)) return;

        // Plugin.Log("srs aerobiclevel", self.aerobicLevel);
        if (self.aerobicLevel > 0.95f && module.spearExhaustCounter > 0)
        {
            self.exhausted = true;
        }
        else if (self.aerobicLevel < 0.4f)
        {
            self.exhausted = false;
        }

        // 我受不了这个体力限制了，所以现在它在开发者模式下会解除（
        if (self.exhausted && !(Options.DevMode.Value && self.room != null && self.room.game.devToolsActive))
        {
            self.slowMovementStun = Math.Max(self.slowMovementStun, (int)Custom.LerpMap(self.aerobicLevel, 0.7f, 0.4f, 6f, 0f));
            if (self.aerobicLevel > 0.9f && Random.value < 0.05f)
            {
                self.Stun(7);
            }
            if (self.aerobicLevel > 0.9f && Random.value < 0.1f)
            {
                self.standing = false;
            }
            if (!self.lungsExhausted || !(self.animation != Player.AnimationIndex.SurfaceSwim))
            {
                self.swimCycle += 0.05f;
            }
        }
        else
        {
            self.slowMovementStun = Math.Max(self.slowMovementStun, (int)Custom.LerpMap(self.aerobicLevel, 1f, 0.4f, 2f, 0f, 2f));
        }

        // 20
        if (!self.input[0].pckp || self.input[0].y != 1)
        {
            

            PlayerGraphics.TailSpeckles tailSpecks = (self.graphicsModule as PlayerGraphics).tailSpecks;
            if (tailSpecks.spearProg > 0f)
            {
                tailSpecks.setSpearProgress(Mathf.Lerp(tailSpecks.spearProg, 0f, 0.05f));
                if (tailSpecks.spearProg < 0.025f)
                {
                    tailSpecks.setSpearProgress(0f);
                }
            }
            else
            {
                self.smSpearSoundReady = false;
            }
        }

        // 100
        int num5 = -1;
        for (int i = 0; i < 2; i++)
        {
            if (self.grasps[i] != null && self.grasps[i].grabbed is IPlayerEdible && (self.grasps[i].grabbed as IPlayerEdible).Edible)
            {
                num5 = i;
            }
        }

        // 174 需要按住拾取和上键
        if ((self.grasps[0] == null || self.grasps[1] == null) && num5 == -1 && self.input[0].y == 1)
        {
            // 防止拔矛的时候把背上的玩家扔下来
            // 总之我自己感觉体验极差，遂改之。尤其是在杆子上拔矛的时候，任何一个联机p2都能给我拽掉地上去
            // 希望这个不会引发别的问题
            // TODO: 看样子不会 太好了 我认为这是史上最伟大的修复 应该给矛大师也整一个
            if (self.slugOnBack.HasASlug)
            {
                self.slugOnBack.counter = 0;
            }

            PlayerGraphics.TailSpeckles tailSpecks = (self.graphicsModule as PlayerGraphics).tailSpecks;
            if (tailSpecks.spearProg == 0f)
            {
                tailSpecks.newSpearSlot();
            }
            if (tailSpecks.spearProg < 0.1f)
            {
                tailSpecks.setSpearProgress(Mathf.Lerp(tailSpecks.spearProg, 0.11f, 0.1f));
            }
            else
            {
                self.Blink(5);
                if (!self.smSpearSoundReady)
                {
                    self.smSpearSoundReady = true;
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Pull, 0f, 1f, 1f + Random.value * 0.5f);
                }
                tailSpecks.setSpearProgress(Mathf.Lerp(tailSpecks.spearProg, 1f, 0.05f));
            }
            if (tailSpecks.spearProg > 0.6f)
            {
                (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * ((tailSpecks.spearProg - 0.6f) / 0.4f) * 2f;
            }
            if (tailSpecks.spearProg > 0.95f)
            {
                tailSpecks.setSpearProgress(1f);
            }

            // 加一些体力限制（
            if (tailSpecks.spearProg > 0.1f && tailSpecks.spearProg < 0.95f)
            {
                self.AerobicIncrease(0.07f);
            }

            if (tailSpecks.spearProg == 1f)
            {
                // 防止其他原因导致力竭
                module.spearExhaustCounter = 400;

                self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Grab, 0f, 1f, 0.5f + Random.value * 1.5f);
                self.smSpearSoundReady = false;
                Vector2 pos = (self.graphicsModule as PlayerGraphics).tail[(int)((float)(self.graphicsModule as PlayerGraphics).tail.Length / 2f)].pos;
                for (int j = 0; j < 4; j++)
                {
                    Vector2 vector = Custom.DirVec(pos, self.bodyChunks[1].pos);
                    self.room.AddObject(new WaterDrip(pos + Custom.RNV() * Random.value * 1.5f, Custom.RNV() * 3f * Random.value + vector * Mathf.Lerp(2f, 6f, Random.value), false));
                }
                for (int k = 0; k < 5; k++)
                {
                    Vector2 vector2 = Custom.RNV();
                    self.room.AddObject(new Spark(pos + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
                }
                int spearType = tailSpecks.spearType;
                tailSpecks.setSpearProgress(0f);
                AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), false);
                self.room.abstractRoom.AddEntity(abstractSpear);
                abstractSpear.pos = self.abstractCreature.pos;
                abstractSpear.RealizeInRoom();
                Vector2 vector3 = self.bodyChunks[0].pos;
                Vector2 vector4 = Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos);
                if (Mathf.Abs(self.bodyChunks[0].pos.y - self.bodyChunks[1].pos.y) > Mathf.Abs(self.bodyChunks[0].pos.x - self.bodyChunks[1].pos.x) && self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y)
                {
                    vector3 += Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos) * 5f;
                    vector4 *= -1f;
                    vector4.x += 0.4f * (float)self.flipDirection;
                    vector4.Normalize();
                }
                abstractSpear.realizedObject.firstChunk.HardSetPosition(vector3);
                abstractSpear.realizedObject.firstChunk.vel = Vector2.ClampMagnitude((vector4 * 2f + Custom.RNV() * Random.value) / abstractSpear.realizedObject.firstChunk.mass, 6f);
                if (self.FreeHand() > -1)
                {
                    self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                }
                if (abstractSpear.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                {
                    (abstractSpear.realizedObject as Spear).Spear_makeNeedle(spearType, true);
                    if ((self.graphicsModule as PlayerGraphics).useJollyColor)
                    {
                        (abstractSpear.realizedObject as Spear).jollyCustomColor = new Color?(PlayerGraphics.JollyColor(self.playerState.playerNumber, 2));
                    }
                }
                self.wantToThrow = 0;
            }
        }



    }











}

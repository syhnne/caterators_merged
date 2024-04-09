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
using static Caterators_by_syhnne.nsh.ReviveSwarmerModules;
using UnityEngine.LowLevel;
using System.Security.Policy;
using System.Security.Cryptography.X509Certificates;
using HUD;

namespace Caterators_by_syhnne._public;




public class DeathPreventHooks
{
    public static void Apply()
    {
        // deathPreventer
        On.DaddyLongLegs.Eat += DaddyLongLegs_Eat;
        IL.Creature.Update += IL_Creature_Update;
        // IL.Player.ClassMechanicsSaint += IL_Player_ClassMechanicsSaint;
        IL.ZapCoil.Update += IL_ZapCoil_Update;
        IL.Centipede.Shock += IL_Centipede_Shock;
        On.Creature.Violence += Creature_Violence;
    }

    private static void IL_Creature_Update(ILContext il)
    {
        // 158
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Brtrue),
            i => i.Match(OpCodes.Ldarg_0),
            i => i.MatchCall<PhysicalObject>("get_Submersion"),
            i => i.Match(OpCodes.Ldc_R4)
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, Creature, float>>((origSubmersion, creature) =>
            {
                if (creature is Player && Plugin.playerModules.TryGetValue(creature as Player, out var module) && module.deathPreventer != null)
                {
                    module.deathPreventer.TryPreventDeath(PlayerDeathReason.LethalWater);
                    return 0f;
                }
                return origSubmersion;
            });
        }
    }

    // 奶奶的 不管了
    /*private static void IL_Player_ClassMechanicsSaint(ILContext il)
    {
        // 1223
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Call),
            i => i.Match(OpCodes.Stfld),
            i => i.Match(OpCodes.Ldloca_S),
            i => i.MatchIsinst<Creature>()
            ))
        {
            c.Emit(OpCodes.Ldloc, 18);
            c.EmitDelegate<Func<bool, PhysicalObject, bool>>((isCreature, obj) => 
            { 
                if (obj is Player && Plugin.playerModules.TryGetValue(obj as Player, out var module) && module.deathPreventer != null)
                {
                    module.deathPreventer.TryPreventDeath(PlayerDeathReason.SaintAscention);
                    return false;
                }
                return isCreature;
            });
        }
    }*/


    private static void IL_ZapCoil_Update(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // 182
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
                // (fp)
                if (physicalObj is Player && (physicalObj as Player).SlugCatClass == Enums.FPname)
                {
                    (physicalObj as Player).Stun(200);
                    physicalObj.room.AddObject(new CreatureSpasmer(physicalObj as Player, false, (physicalObj as Player).stun));
                    (physicalObj as Player).LoseAllGrasps();
                    if (Plugin.DevMode)
                    {
                        int maxfood = (physicalObj as Player).MaxFoodInStomach;
                        int food = (physicalObj as Player).FoodInStomach;
                        Plugin.Log("Zapcoil - food:" + food + " maxfood: " + maxfood);
                        fp.PlayerHooks.CustomAddFood(physicalObj as Player, maxfood - food);
                        (physicalObj as Player).AddFood(maxfood - food);
                    }
                    return null;
                }
                // (deathPreventer)
                else if (physicalObj is Player && Plugin.playerModules.TryGetValue(physicalObj as Player, out var module) && module.deathPreventer != null)
                {
                    bool prevented = module.deathPreventer.TryPreventDeath(PlayerDeathReason.ZapCoil);
                    return prevented ? null : physicalObj;
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
                // (fp)
                if (physicalObj is Player && (physicalObj as Player).SlugCatClass == Enums.FPname)
                {
                    fp.PlayerHooks.CustomAddFood(physicalObj as Player, 1);
                    return 0;
                }
                // (deathPreventer)
                else if (physicalObj is Player && Plugin.playerModules.TryGetValue(physicalObj as Player, out var module) && module.deathPreventer != null)
                {
                    bool prevented = module.deathPreventer.TryPreventDeath(PlayerDeathReason.DangerGrasp);
                    return prevented ? 0 : centipedeMass;
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
        else if (self is Player && Plugin.playerModules.TryGetValue(self as Player, out var module) && module.deathPreventer != null)
        {
            if (self.room.world.rainCycle.RainApproaching > 0f && module.deathPreventer.TryPreventDeath(PlayerDeathReason.Violence))
            {
                { return; }
            }
            
        }
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }



    private static void DaddyLongLegs_Eat(On.DaddyLongLegs.orig_Eat orig, DaddyLongLegs self, bool eu)
    {
        for (int i = self.eatObjects.Count - 1; i >= 0; i--)
        {
            if (self.eatObjects[i].chunk.owner is Player && Plugin.playerModules.TryGetValue((self.eatObjects[i].chunk.owner as Player), out var module) && module.deathPreventer != null)
            {
                if (module.deathPreventer.TryPreventDeath(PlayerDeathReason.InstanceDestroy) || module.deathPreventer.justPreventedCounter > 0)
                {
                    self.eatObjects[i].progression = 0;
                }
                
            }
        }
        orig(self, eu);
    }



}









// 呃 怎么写比较好。。
public class DeathPreventer
{
    public Player player;
    public int justPreventedCounter;

    public DeathPreventer(Player player) 
    { 
        this.player = player;
        justPreventedCounter = 0;
        Plugin.Log("new deathPreventer for player", player.abstractCreature.ID.number);
    }




    public void SetInvuln()
    {
        player.airInLungs = 1f;
        player.drown = 0f;
        player.lungsExhausted = false;
        player.stun = 0;
        player.abstractCreature.tentacleImmune = true;
        player.abstractCreature.lavaImmune = true;
        player.abstractCreature.HypothermiaImmune = true;
        for (int i = player.grabbedBy.Count - 1; i >= 0; i--)
        {
            if (player.grabbedBy[i].grabber is not Player)
                player.grabbedBy[i].grabber.ReleaseGrasp(player.grabbedBy[i].graspUsed);
        }
        
    }


    public void DisableInvuln()
    {
        player.abstractCreature.tentacleImmune = false;
        player.abstractCreature.lavaImmune = false;
        player.abstractCreature.HypothermiaImmune = false;
    }



    // 检测一些常见的死法，比如被蜥蜴咬
    public void Update()
    {
        
        if (player.slatedForDeletetion || player.room == null) return;

        // Plugin.Log(justPreventedCounter);
        // 防止反复去世
        if (justPreventedCounter > 0)
        {
            justPreventedCounter--;
            SetInvuln();
        }
        else { DisableInvuln(); }

        // 
        /*if (player.grabbedBy != null)
        {
            foreach (var item in player.grabbedBy)
            {
                if (item.grabber is not Player)
                {
                    TryPreventDeath(PlayerDeathReason.DangerGrasp);
                }
            }
        }*/
        if (player.dangerGraspTime > 30)
        {
            TryPreventDeath(PlayerDeathReason.DangerGrasp);
        }

    }



    // private是为了防止我在别的地方调用这些代码，导致出现player.room是null之类的问题
    // 这个东西可能是moon或者nsh的神经元，所以返回一个apo
    private AbstractPhysicalObject? DeathPreventObject
    {
        get
        {
            if (player == null || player.slatedForDeletetion) return null;
            if (player.grasps != null)
            {
                foreach (var grasp in player.grasps)
                {
                    if (grasp != null && grasp.grabbed != null && grasp.grabbed is ReviveSwarmer)
                    {
                        return grasp.grabbed.abstractPhysicalObject;
                    }
                }
            }
            if (player.objectInStomach != null && player.objectInStomach is ReviveSwarmerAbstract)
            {
                return player.objectInStomach;
            }
            if (Plugin.playerModules.TryGetValue(player, out var module))
            {
                // 反倒是这个，一点问题也没有……上面的东西全是bug
                if (module.nshInventory != null)
                {
                    foreach (AbstractPhysicalObject obj in module.nshInventory.Items)
                    {
                        if (obj is ReviveSwarmerAbstract)
                        {
                            return obj;
                        }
                    }
                }
                // else if (moon...)
            }

            return null;
        }
    }




    public bool TryPreventDeath(PlayerDeathReason deathReason)
    {
        Plugin.Log("Try prevent death for player", player.abstractCreature.ID.number, deathReason.ToString());
        AbstractPhysicalObject reviveSwarmer = DeathPreventObject;
        if (player.room == null || reviveSwarmer == null || (reviveSwarmer is not nsh.ReviveSwarmerModules.ReviveSwarmerAbstract ))
        {
            Plugin.Log("player", player.abstractCreature.ID.number, "death NOT prevented, reason:", deathReason.ToString(), DeathPreventObject == null);
            return false;
        }

        bool deathExplode = true;
        if (deathReason == PlayerDeathReason.DangerGrasp || deathReason == PlayerDeathReason.Violence)
        {
            
            player.AllGraspsLetGoOfThisObject(true);
        }


        
        if (deathExplode && justPreventedCounter <= 0)
        {
            // 有空再细改，现在抄的fp那边的代码
            Vector2 pos2 = player.firstChunk.pos;
            Color effectColor = new Color(0.6f, 1f, 0.6f);
            player.room.AddObject(new Explosion.ExplosionLight(pos2, 200f, 1f, 4, effectColor));
            for (int l = 0; l < 8; l++)
            {
                Vector2 vector2 = Custom.RNV();
                player.room.AddObject(new Spark(pos2 + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
            }
            player.room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6, false));
            player.room.AddObject(new ElectricDeath.SparkFlash(pos2, 10f));
            player.room.PlaySound(SoundID.Centipede_Shock, pos2, 1f, 1f + 0.25f * Random.value);
            player.room.InGameNoise(new InGameNoise(pos2, 8000f, player, 1f));
            if (player.room.Darkness(pos2) > 0f)
            {
                player.room.AddObject(new LightSource(pos2, false, effectColor, player));
            }

            List<Weapon> list = new List<Weapon>();
            for (int m = 0; m < player.room.physicalObjects.Length; m++)
            {
                for (int n = 0; n < player.room.physicalObjects[m].Count; n++)
                {
                    if (player.room.physicalObjects[m][n] is Weapon)
                    {
                        Weapon weapon = player.room.physicalObjects[m][n] as Weapon;
                        if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
                        {
                            list.Add(weapon);
                        }
                    }
                    bool flag3;
                    if (ModManager.CoopAvailable && !Custom.rainWorld.options.friendlyFire)
                    {
                        Player player = this.player.room.physicalObjects[m][n] as Player;
                        flag3 = (player == null || player.isNPC);
                    }
                    else
                    {
                        flag3 = true;
                    }
                    if (player.room.physicalObjects[m][n] is Creature && player.room.physicalObjects[m][n] != player && flag3)
                    {
                        Creature creature = player.room.physicalObjects[m][n] as Creature;
                        if (Custom.Dist(pos2, creature.firstChunk.pos) < 200f && (Custom.Dist(pos2, creature.firstChunk.pos) < 60f || player.room.VisualContact(player.abstractCreature.pos, creature.abstractCreature.pos)))
                        {
                            player.room.socialEventRecognizer.WeaponAttack(null, player, creature, true);
                            creature.SetKillTag(player.abstractCreature);
                            if (creature is Scavenger)
                            {
                                (creature as Scavenger).HeavyStun(100);
                                creature.Blind(400);
                            }
                            else
                            {
                                creature.Stun(100);
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
                    for (int num6 = 0; num6 < list.Count; num6++)
                    {
                        list[num6].ChangeMode(Weapon.Mode.Free);
                        list[num6].firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, list[num6].firstChunk.pos)) * 20f;
                        list[num6].SetRandomSpin();
                    }
                }
            }
        }


        // 复活玩家
        (player.State as PlayerState).alive = true;
        (player.State as PlayerState).permaDead = false;
        (player.State as PlayerState).permanentDamageTracking = 0;
        player.dead = false;
        player.killTag = null;
        player.killTagCounter = 0;
        AbstractCreatureAI abstractAI = player.abstractCreature.abstractAI;
        abstractAI?.SetDestination(player.abstractCreature.pos);
        

        


        if (justPreventedCounter == 0)
        {
            // 移除复活用的神经元（月姐那个估计要单独的函数，回头再写）
            if (reviveSwarmer.realizedObject != null)
            {
                reviveSwarmer.realizedObject.AllGraspsLetGoOfThisObject(true);
                player.room.RemoveObject(reviveSwarmer.realizedObject);
                reviveSwarmer.realizedObject = null;
            }
            if (Plugin.playerModules.TryGetValue(player, out var module) && module.nshInventory != null && module.nshInventory.RemoveSpecificObj(reviveSwarmer))
            {
            }
            else if (player.objectInStomach == reviveSwarmer)
            {
                player.objectInStomach = null;
            }
            else
            {
                player.room.abstractRoom.RemoveEntity(reviveSwarmer);
            }
        }

        // 复活后有短暂的无敌（但如果是队友把你复活的，那就不会有）
        justPreventedCounter = 120;
        Plugin.Log("player", player.abstractCreature.ID.number, "Death prevented! reason:", deathReason.ToString());

        return true;
    }



    public void Destroy()
    {

    }


}


// 只能防以下内容，其他的比如舌草，下雨，无底洞什么的防不住（才不是因为我懒得写呢（而且下雨这东西防了也没用的罢
// 唉 写这个的时候才发现 那些个死法我都没记全 白死那么多回了
// 写这个是因为死了之后需要加一小段无敌时间
// 啊 其实应该直接抄DevConsole的代码
public enum PlayerDeathReason
{
    Unknown,
    Drown,
    LethalWater,
    DangerGrasp,
    Violence,
    FP, // fp：你似乎拿着一种不属于这个世界线的东西 SSOracleBehavior.ThrowOutBehavior.Update()
    InstanceDestroy, // 人话：删模杀（比如香菇和利维坦 BigEel.JawsSnap() StowawayBug.Eat()（话说这是什么生物啊 藤壶吗）DaddyLongLegs.Eat()
    TerrainImpact, // 你感受到了动能 Player.TerrainImpact()
    Hypothermia, // Creature.HypothermiaUpdate()
    // 我自己能想起来的就到此为止了，剩下的事在assembly_csharp里面查找Player.Die()的全部引用找到的
    Explosion,
    ArtiPyroDeath, // 啊。。这个只包括炸多了去世的，不包括泡水里，那个算在Drown里面
    Leech, // 不翻代码不知道，原来水蛭能直接吸死猫啊 Leech.Attached()
    BigJellyFish, // MoreSlugcats.BigJellyFish.Collide()
    Singularity, // MoreSlugcats.EnergyCell.Explode() MoreSlugcats.SingularityBomb.Explode()
    SaintAscention, // 妈的不翻代码真的想不到这个 Player.ClassMechanicsSaint()
    GourmandSmash, // ...痛击我的队友 Player.Collide()
    // room.game.setupValues.invincibility 这是什么 他是我需要的那个吗
    SpiderAttach, // Spider.Attached()
    ZapCoil, // 啊 这个我想不起来也无所谓 因为我已经写了（）卧槽，舌头舔到这玩意也会寄 Player.Tongue.Update()
    // LilyPuck是能当矛用吗？？？
    // 挑战70被茅草裂片杀死这个我不管了……
    Spear, // 我以为这个是在Violence里面的，看来不是

}




public class DeathPreventHUD : HudPart
{
    public DeathPreventer owner;
    public FSprite circle;
    public Vector2 pos;
    public Vector2 lastPos;
    public float fade;
    public float lastFade;

    public DeathPreventHUD(HUD.HUD hud, FContainer fContainer, DeathPreventer owner) : base(hud)
    { 
        this.owner = owner;
        circle = new FSprite("Futile_White", true);
        circle.shader = hud.rainWorld.Shaders["HoldButtonCircle"];
        circle.color = nsh.ReviveSwarmerModules.NSHswarmerColor;
        pos = owner.player.mainBodyChunk.pos;
        lastPos = pos;
        fade = 0f;
        lastFade = 0f;
        circle.scale = 8f;
        fContainer.AddChild(circle);
    }

    private bool Show
    {
        get
        {
            return owner != null && owner.player.room != null && owner.justPreventedCounter > 0;
        }
    }


    public override void Update()
    {

        base.Update();
        lastPos = pos;
        lastFade = fade;
        Vector2 camPos = Vector2.zero;
        if (owner.player.room != null)
        {
            camPos = owner.player.room.game.cameras[0].pos;
        }
        pos = owner.player.mainBodyChunk.pos - camPos;

        circle.isVisible = true;
        fade = (owner.justPreventedCounter / 120);

        


    }


    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(lastPos, pos, timeStacker);
    }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame) return;

        circle.alpha = fade;
    }


    public override void ClearSprites()
    {
        base.ClearSprites();
        circle.RemoveFromContainer();
        circle = null;
    }

}
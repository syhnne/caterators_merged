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
using Caterators_by_syhnne.moon.MoonSwarmer;

namespace Caterators_by_syhnne._public;



// : 这些东西生效吗？
// 破案了，香菇那个没什么用，被香菇舌草抓了基本上死路一条，救也没用。其他的倒是挺好使的。
public class DeathPreventHooks
{
    public static void Apply()
    {
        // deathPreventer
        On.DaddyLongLegs.Eat += DaddyLongLegs_Eat;
        IL.Creature.Update += IL_Creature_Update; // hypothermia
        // IL.Player.ClassMechanicsSaint += IL_Player_ClassMechanicsSaint;
        IL.ZapCoil.Update += IL_ZapCoil_Update;
        IL.Centipede.Shock += IL_Centipede_Shock;
        On.Creature.Violence += Creature_Violence;
        // IL.Creature.Violence += IL_Creature_Violence;
        IL.DaddyCorruption.EatenCreature.Update += IL_DaddyCorruption_EatenCreature_Update;
        IL.WormGrass.WormGrassPatch.InteractWithCreature += IL_WormGrass_WormGrassPatch_InteractWithCreature;
    }

    private static void IL_Creature_Update(ILContext il)
    {
        ILCursor c2 = new(il);
        if (c2.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Isinst),
            i => i.Match(OpCodes.Brfalse_S),
            i => i.MatchLdarg(0),
            i => i.MatchIsinst<Player>()))
        {
            c2.EmitDelegate<Func<Player, Player>>((player) =>
            {
                if (Plugin.playerModules.TryGetValue(player, out var mod) && mod.deathPreventer != null)
                {
                    mod.deathPreventer.dontRevive = true;
                }
                return player;
            });
        }
    }




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
                    return null;
                }
                // (deathPreventer)
                /*else if (physicalObj is Player && Plugin.playerModules.TryGetValue(physicalObj as Player, out var module) && module.deathPreventer != null)
                {
                    bool prevented = module.deathPreventer.TryPreventDeath(PlayerDeathReason.ZapCoil);
                    return prevented ? null : physicalObj;
                }*/
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
                /*else if (physicalObj is Player && Plugin.playerModules.TryGetValue(physicalObj as Player, out var module) && module.deathPreventer != null)
                {
                    bool prevented = module.deathPreventer.TryPreventDeath(PlayerDeathReason.DangerGrasp);
                    return prevented ? 0 : centipedeMass;
                }*/
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
        /*else if (self is Player && Plugin.playerModules.TryGetValue(self as Player, out var module) && module.deathPreventer != null && )
        {
            if (self.room.world.rainCycle.RainApproaching > 0f && module.deathPreventer.TryPreventDeath(PlayerDeathReason.Violence))
            {
                { return; }
            }
        }*/
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

        if (self.State is HealthState)
        {
            Plugin.Log("creature:", self, "health:", (self.State as HealthState).health);
        }
    }



    private static void IL_Creature_Violence(ILContext il)
    {
        /*ILCursor c1 = new(il);
        if (c1.TryGotoNext(
            i => i.MatchIsinst<HealthState>(),
            i => i.MatchDup(),
            i => i.MatchCallvirt<HealthState>("get_health"),
            i => i.MatchLdloc(0)))
        {
            Plugin.Log("ilcursor found");
            c1.Emit(OpCodes.Ldarg_0);
            c1.EmitDelegate<Func<float, Creature, float>>((minus, self) =>
            {
                if (self is Player && Plugin.playerModules.TryGetValue(self as Player, out var module) && module.deathPreventer != null && module.deathPreventer.TryPreventDeath(PlayerDeathReason.Violence))
                {
                    minus = 0f;
                }
                return minus;
            });
        }*/

        /*ILCursor c2 = new(il);
        if (c2.TryGotoNext(
            i => i.Match(OpCodes.Ldsfld),
            i => i.MatchStfld<Creature>("stunDamageType"),
            i => i.MatchLdarg(0),
            i => i.MatchCall<Creature>("get_State"),
            i => i.MatchIsinst<HealthState>()))
        {
            c2.Emit(OpCodes.Ldarg_0);
            c2.Emit(OpCodes.Ldloc_0);
            c2.EmitDelegate<Func<bool, Creature, float, bool>>((orig, creature, minus) =>
            {
                return orig;
            });
        }*/

        ILCursor c3 = new(il);
        if (c3.TryGotoNext(
            i => i.Match(OpCodes.Call),
            i => i.MatchLdcR4(0.33f),
            i => i.Match(OpCodes.Bge_Un_S),
            i => i.MatchLdarg(0)))
        {

        }
    }



    private static void DaddyLongLegs_Eat(On.DaddyLongLegs.orig_Eat orig, DaddyLongLegs self, bool eu)
    {
        for (int i = self.eatObjects.Count - 1; i >= 0; i--)
        {
            if (self.eatObjects[i].chunk.owner is Player && Plugin.playerModules.TryGetValue((self.eatObjects[i].chunk.owner as Player), out var module) && module.deathPreventer != null)
            {
                /*if (module.deathPreventer.TryPreventDeath(PlayerDeathReason.Destroy) || module.deathPreventer.justPreventedCounter > 0)
                {
                    self.eatObjects[i].progression = 0;
                }*/
                module.deathPreventer.dontRevive = true;
            }
        }
        orig(self, eu);
    }


    private static void IL_DaddyCorruption_EatenCreature_Update(ILContext il)
    {
        // 142之后，不影响他原来的值，只是给deathpreventer传一个值
        ILCursor c1 = new(il);
        if (c1.TryGotoNext(
            i => i.MatchLdfld<DaddyCorruption.EatenCreature>("progression"),
            i => i.MatchLdcR4(1),
            i => i.Match(OpCodes.Blt_Un_S)))
        {
            c1.Emit(OpCodes.Ldarg_0);
            c1.EmitDelegate<Action<DaddyCorruption.EatenCreature>>((daddyCorruption) =>
            {
                if (daddyCorruption.creature is Player && Plugin.playerModules.TryGetValue(daddyCorruption.creature as Player, out var mod) && mod.deathPreventer != null)
                {
                    mod.deathPreventer.dontRevive = true;
                }
            });
        }

    }


    // 妈的有没有人能告诉我这玩意儿为什么不管用
    // byd竟然是打错字了 ldloca到底是什么东西为啥会多个a 为啥他自动tab会给我tab出这个 为啥我在这里输出的日志一律不管用。。。。
    private static void IL_WormGrass_WormGrassPatch_InteractWithCreature(ILContext il)
    {
        // 176 他存了一个player实例变量，此处已经判定完毕player不为null。所以直接调用
        ILCursor c1 = new(il);
        if (c1.TryGotoNext(
            i => i.Match(OpCodes.Isinst),
            i => i.Match(OpCodes.Stloc_S),
            i => i.Match(OpCodes.Ldloc_S),
            i => i.Match(OpCodes.Brfalse_S),
            i => i.Match(OpCodes.Ldloc_S)))
        {
            Plugin.Log("IL_WormGrass_WormGrassPatch_InteractWithCreature");
            c1.EmitDelegate<Func<Player, Player>>((player) =>
            {
                if (Plugin.playerModules.TryGetValue(player, out var mod) && mod.deathPreventer != null)
                {
                    Plugin.Log("wormgrass dont revive");
                    mod.deathPreventer.dontRevive = true;
                }
                return player;
            });
        }
    }



}









// 呃 怎么写比较好。。
public class DeathPreventer
{
    public Player player;
    public int justPreventedCounter;
    public moon.MoonSwarmer.SwarmerManager swarmerManager;
    public Color effectColor;

    // 给swarmermanager提供的防止连锁反应的手段
    public bool dontRevive = false;
    public bool forceRevive = false;

    public int dangerGraspCounter = 0;

    public DeathPreventer(Player player) 
    { 
        this.player = player;
        justPreventedCounter = 0;
        Plugin.Log("new deathPreventer for player", player.abstractCreature.ID.number);
        effectColor = nsh.ReviveSwarmerModules.NSHswarmerColor;
    }




    public void SetInvuln()
    {
        player.airInLungs = 1f;
        player.drown = 0f;
        player.lungsExhausted = false;
        player.stun = 0;
        player.Hypothermia -= 0.1f;
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
        bool g = false;
        for (int i = player.grabbedBy.Count - 1; i >= 0; i--)
        {
            // 这我不好说嗷 不知道除了蛞蝓猫以外还有什么属于是危险生物
            if (player.grabbedBy[i].grabber is not Player)
            {
                g = true;
            }
        }
        if (g) dangerGraspCounter++; else dangerGraspCounter = 0;

        if (dangerGraspCounter > 58 && player.dangerGrasp.grabber is not Centipede)
        {
            TryPreventDeath(PlayerDeathReason.DangerGrasp);
        }

    }



    // private是为了防止我在别的地方调用这些代码，导致出现player.room是null之类的问题
    // 这个东西可能是moon或者nsh的神经元，所以返回一个apo
    private object? DeathPreventObject
    {
        get
        {
            if (player == null || player.slatedForDeletetion) return null;
            bool getModule = Plugin.playerModules.TryGetValue(player, out var module);

            // 如果启动了一个神经元，在准备复活队友的时候自己去世了，就会使用启动的这个神经元
            if (getModule && module.playerReviver != null && module.playerReviver.activatedSwarmer != null)
            {
                return module.playerReviver.activatedSwarmer.abstractPhysicalObject;
            }

            // moon会优先消耗自己的神经元，因为这个只能复活自己不能复活队友
            if (getModule && module.swarmerManager != null)
            {
                if (module.swarmerManager.callBackSwarmers != null)
                {
                    return 1;
                }
                else if (module.swarmerManager.CanConsumeSwarmer)
                {
                    return module.swarmerManager.LastAliveSwarmer.abstractCreature;
                }
                
            }


            // TODO: 这个目前有bug，抓在手上的时候不好使。
            // 触发逻辑倒是修好了，原来蛞蝓猫被咬的时候会先调用violence造成咬伤啊（恍然大悟
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
            
            if (getModule && module.nshInventory != null)
            {
                foreach (AbstractPhysicalObject obj in module.nshInventory.Items)
                {
                    if (obj is ReviveSwarmerAbstract)
                    {
                        return obj;
                    }
                }
            }

            if (player.objectInStomach != null && player.objectInStomach is ReviveSwarmerAbstract)
            {
                return player.objectInStomach;
            }

            return null;
        }
    }



    // TODO: 修复玩家被蜥蜴叼走然后复活的时候，蜥蜴钻管道导致玩家图像消失的问题
    public bool TryPreventDeath(PlayerDeathReason deathReason)
    {
        Plugin.Log("TryPreventDeath:", !dontRevive, justPreventedCounter <= 0, deathReason < PlayerDeathReason.DeathPit);
        if (dontRevive)
        {
            // 好吧 不能在这改 因为还有别的函数要读这个 这个在player.Die()里改了
            // dontRevive = false;
            return false;
        }
        if (justPreventedCounter > 0) { return false; }
        if (deathReason >= PlayerDeathReason.DeathPit) return false;
        object reviveSwarmer = DeathPreventObject;
        // Plugin.Log("Try prevent death for player", player.abstractCreature.ID.number, deathReason.ToString(), "forceRevive:", forceRevive, "nullObject:", reviveSwarmer == null, "nullRoom:", player.room == null);
        
        if (player.room == null || (reviveSwarmer == null && !forceRevive))
        {
            // Plugin.Log("player", player.abstractCreature.ID.number, "death NOT prevented, reason:", deathReason.ToString(), reviveSwarmer == null);
            return false;
        }
        if (forceRevive && swarmerManager != null && swarmerManager.callBackSwarmers != null)
        {
            swarmerManager.callBackSwarmers--;
        }

        effectColor = reviveSwarmer != null && reviveSwarmer is not AbstractCreature ? nsh.ReviveSwarmerModules.NSHswarmerColor : Color.white;

        bool deathExplode = true;
        if (deathReason == PlayerDeathReason.DangerGrasp || deathReason == PlayerDeathReason.Violence)
        {
            for (int i = 0; i < player.grabbedBy.Count; i++)
            {
                player.grabbedBy[i]?.Release();

            }
            player.AllGraspsLetGoOfThisObject(true);
        }


        
        if (deathExplode && justPreventedCounter <= 0)
        {
            // 有空再细改，现在抄的fp那边的代码
            Vector2 pos2 = player.firstChunk.pos;

            player.room.AddObject(new Explosion.ExplosionLight(pos2, 200f, 1f, 4, effectColor));
            for (int l = 0; l < 8; l++)
            {
                Vector2 vector2 = Custom.RNV();
                player.room.AddObject(new Spark(pos2 + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
            }
            player.room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6, false));
            if (reviveSwarmer is not AbstractCreature)
            {
                player.room.AddObject(new ElectricDeath.SparkFlash(pos2, 10f));
            }
            // player.room.PlaySound(SoundID.Centipede_Shock, pos2, 1f, 1f + 0.25f * Random.value);
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
                    if (player.room.physicalObjects[m][n] is Creature && player.room.physicalObjects[m][n] is not Player && flag3)
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
        player.slatedForDeletetion = false;
        AbstractCreatureAI abstractAI = player.abstractCreature.abstractAI;
        abstractAI?.SetDestination(player.abstractCreature.pos);

        // 不知道为啥，神经元用完之后还会粘在手上，我怀疑allgrasps那个函数就是坏的，调用它好几次没有一次成功的（擦汗
        // player.LoseAllGrasps();
        if (reviveSwarmer is AbstractPhysicalObject)
        {
            (reviveSwarmer as AbstractPhysicalObject).realizedObject?.AllGraspsLetGoOfThisObject(true);
        }

        // 移除复活用的神经元
        if (reviveSwarmer  != null) RemoveSwarmer(reviveSwarmer);



        // 复活后有短暂的无敌（但如果是队友把你复活的，那就不会有）
        justPreventedCounter = 120;
        Plugin.Log("player", player.abstractCreature.ID.number, "Death prevented! reason:", deathReason.ToString());

        return true;
    }



    public void RemoveSwarmer(object reviveSwarmer)
    {
        
        if (justPreventedCounter > 0) { return; }

        

        if (reviveSwarmer is int) 
        {
            if (swarmerManager.callBackSwarmers != null) swarmerManager.callBackSwarmers--;
            else Plugin.Log("!! used int as reviveSwarmer, but callBackSwarmers is null");
            return;
        }
        AbstractPhysicalObject s = reviveSwarmer as AbstractPhysicalObject;

        if (swarmerManager != null && s is AbstractCreature && s.realizedObject != null)
        {
            swarmerManager.KillSwarmer(s.realizedObject as MoonSwarmer, true);
            return;
        }
        
        
        if (s.realizedObject != null && s is ReviveSwarmerAbstract)
        {
            (s.realizedObject as ReviveSwarmer).Use(false);
        }
        else
        {
            s.Destroy();
        }

        if (Plugin.playerModules.TryGetValue(player, out var module) && module.nshInventory != null && module.nshInventory.RemoveSpecificObj(s))
        {
        }
        else if (player.objectInStomach == s)
        {
            player.objectInStomach = null;
        }
        else
        {
            player.room.abstractRoom.RemoveEntity(s);
        }

    }


}


// 只能防以下内容，其他的比如舌草，下雨，无底洞什么的防不住（才不是因为我懒得写呢（而且下雨这东西防了也没用的罢
// 唉 写这个的时候才发现 那些个死法我都没记全 白死那么多回了
// 写这个是因为死了之后需要加一小段无敌时间
// 啊 其实应该直接抄DevConsole的代码
/*public enum PlayerDeathReason
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
    Singularity, // MoreSlugcats.EnergyCell.Kill() MoreSlugcats.SingularityBomb.Kill()
    SaintAscention, // 妈的不翻代码真的想不到这个 Player.ClassMechanicsSaint()
    GourmandSmash, // ...痛击我的队友 Player.Collide()
    // room.game.setupValues.invincibility 这是什么 他是我需要的那个吗
    SpiderAttach, // Spider.Attached()
    ZapCoil, // 啊 这个我想不起来也无所谓 因为我已经写了（）卧槽，舌头舔到这玩意也会寄 Player.Tongue.Update()
    // LilyPuck是能当矛用吗？？？
    // 挑战70被茅草裂片杀死这个我不管了……
    Spear, // 我以为这个是在Violence里面的，看来不是

}*/

public enum PlayerDeathReason
{
    Unknown,
    LethalWater,
    DangerGrasp,
    Violence,
    ZapCoil,
    // 分界线
    DeathPit,
    Destroy,
    // singularity是删模型吗 不是的话我不管了
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
        circle.scale = 4f;
        fContainer.AddChild(circle);
    }

    private bool Show
    {
        get
        {
            return owner != null && owner.player.room != null && owner.justPreventedCounter > 0 && !owner.dontRevive;
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
        circle.color = owner.effectColor;
        fade =((float)owner.justPreventedCounter / 120f);

        


    }


    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(lastPos, pos, timeStacker);
    }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame) return;
        circle.alpha = fade;
        circle.SetPosition(DrawPos(timeStacker));
    }


    public override void ClearSprites()
    {
        base.ClearSprites();
        circle.RemoveFromContainer();
        circle = null;
    }

}
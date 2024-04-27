using Caterators_by_syhnne.nsh;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne._public;


// 呃啊 还是得写个这种东西

// 现在的想法：手上有神经元时（在肚子或者背包里不行）在空中按下拾取+跳跃激活神经元（持续280帧），然后抓住玩家按住拾取键复活
// 这个略有一些复杂，但我都写了那么多复杂的东西了，应该问题不大。至于怎么无缝转换成moon的神经元……就用很亮的光效掩盖过去罢（心虚
// 另外我还想要一个效果，把这玩意扔给月姐看，会触发跟拿到fp神经元完全一样的对话
public class PlayerReviver
{
    public Player player;
    public int reviveCounter = 0;
    public Vector2 activatedPos;
    
    public PlayerReviver(Player player)
    {
        this.player = player;
    }


    

    public ReviveSwarmerModules.ReviveSwarmer? activatedSwarmer;
    public ReviveSwarmerModules.ReviveSwarmer? holdingSwarmer
    {
        get
        {
            if (holdingSwarmerGrasp == null)  return null;
            if (player.grasps[(int)holdingSwarmerGrasp].grabbed is not ReviveSwarmerModules.ReviveSwarmer) return null;
            return player.grasps[(int)holdingSwarmerGrasp].grabbed as ReviveSwarmerModules.ReviveSwarmer;
        }
    }
    public int? holdingSwarmerGrasp
    {
        get 
        { 
            if (player == null || player.grasps.Count() == 0) { return null; }
            for (int i = 0; i < player.grasps.Count(); i++)
            {
                if (player.grasps[i] != null && player.grasps[i].grabbed is ReviveSwarmerModules.ReviveSwarmer)
                    return i;
            }
            return null;
        }
    }
    public bool Activated
    {
        get
        {
            return activatedSwarmer != null && activatedSwarmer.activatedCounter > 0;
        }
    }
    public Player? slugcat
    {
        get
        {
            if (player == null || player.grasps.Count() == 0 || !Activated) { return null; }
            if (player.grasps[0] != null && player.grasps[0].grabbed is Player)
            {
                return player.grasps[0].grabbed as Player;
            }
            else if (player.grasps[1] != null && player.grasps[1].grabbed is Player)
            {
                return player.grasps[1].grabbed as Player;
            }
            return null;
        }
    }


    public void Update(bool eu)
    {
        try
        {
            if (activatedSwarmer != null && (activatedSwarmer.activatedCounter == 0 || activatedSwarmer.room == null || player.room == null || activatedSwarmer.room != player.room))
            {
                Plugin.Log("-- Update");
                activatedSwarmer.Deactivate();
                activatedSwarmer = null;
            }

            ReviveSwarmerModules.ReviveSwarmer? s = holdingSwarmer;
            if (activatedSwarmer == null && s != null && holdingSwarmerGrasp != null && player.room != null
                && player.wantToJump > 0 && player.input[0].pckp && player.canJump <= 0 && !(player.eatMeat >= 20 || player.maulTimer >= 15)
                && player.bodyMode != Player.BodyModeIndex.Crawl && player.bodyMode != Player.BodyModeIndex.CorridorClimb && player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && player.animation != Player.AnimationIndex.HangFromBeam && player.animation != Player.AnimationIndex.ClimbOnBeam && player.bodyMode != Player.BodyModeIndex.WallClimb && player.bodyMode != Player.BodyModeIndex.Swimming && player.Consious && !player.Stunned && player.animation != Player.AnimationIndex.AntlerClimb && player.animation != Player.AnimationIndex.VineGrab && player.animation != Player.AnimationIndex.ZeroGPoleGrab)
            {
                player.TossObject((int)holdingSwarmerGrasp, eu);
                ActivateSwarmer(s);
            }

            if (Activated)
            {
                

                Player slug = slugcat;

                if (player.dangerGraspTime > 0)
                {
                    activatedSwarmer.activatedPos = Vector2.Lerp((Vector2)activatedSwarmer.activatedPos, player.mainBodyChunk.pos, 0.05f);
                }
                else if (slug != null && CanBeRevived(slug))
                {
                    reviveCounter++;
                    // 防止玩家在此期间抓住队友开吃
                    player.eatMeat = 0;
                    if (player.slugOnBack != null)
                    {
                        player.slugOnBack.counter = 0;
                    }
                    activatedSwarmer.activatedPos = Vector2.Lerp((Vector2)activatedSwarmer.activatedPos, slug.mainBodyChunk.pos, 0.05f);
                    // Plugin.Log("reviveCounter:", reviveCounter);
                }
                else
                {
                    activatedSwarmer.activatedPos = activatedPos;
                    reviveCounter = 0;
                }

                if (reviveCounter > 50)
                {
                    Plugin.Log("try revive player", slug.abstractCreature.ID.number);
                    RevivePlayer(slug, activatedSwarmer);
                    reviveCounter = 0;
                    
                }

            }
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
        }
    }




    public void ActivateSwarmer(ReviveSwarmerModules.ReviveSwarmer swarmer)
    {
        if (player.room == null) return;

        WorldCoordinate pos1 = player.abstractCreature.pos;
        // 如果可以的话，在玩家头顶上2格的位置激活
        pos1.y += 2;
        WorldCoordinate pos2 = player.room.GetTile(pos1).IsSolid() ? pos1 : player.abstractCreature.pos;
        activatedPos = player.room.MiddleOfTile(pos2);
        swarmer.Activate(activatedPos);
        activatedSwarmer = swarmer;

        for (int i = 0; i < 3; i++)
        {
            GreenSparks.GreenSpark spark = new GreenSparks.GreenSpark(activatedPos);
            spark.lifeTime = 50;
            player.room.AddObject(spark);
        }
        player.room.AddObject(new LightSource(activatedPos, false, nsh.ReviveSwarmerModules.NSHswarmerColor, activatedSwarmer));
    }



    public static bool CanBeRevived(Player player)
    {
        if (Plugin.playerModules.TryGetValue(player, out var mod) && mod.swarmerManager != null && mod.deathPreventer != null && mod.deathPreventer.justPreventedCounter <= 0) return true;
        if (player.room == null || !(player.dead || !player.playerState.alive || player.playerState.permaDead)) return false;
        return true;
    }



    public static bool RevivePlayer(Player slugcat, ReviveSwarmerModules.ReviveSwarmer activatedSwarmer)
    {
        
        try
        {
            if (slugcat == null) { Plugin.Log("RevivePlayer(): null slugcat"); return false; }
            else if (Plugin.playerModules.TryGetValue(slugcat, out var mod) && mod.swarmerManager != null)
            {
                if (mod.deathPreventer != null) { mod.deathPreventer.justPreventedCounter = 10; }
                mod.swarmerManager.ConvertNSHSwarmer(activatedSwarmer);

                if (!slugcat.dead) { return true; }
            }
            else
            {
                activatedSwarmer.Use(true);
            }
            slugcat.playerState.permanentDamageTracking = 0f;
            slugcat.playerState.alive = true;
            slugcat.playerState.permaDead = false;
            slugcat.dead = false;
            slugcat.killTag = null;
            slugcat.killTagCounter = 0;
            slugcat.stun = 50;
            slugcat.aerobicLevel = 1f;
            slugcat.exhausted = true;
            AbstractCreatureAI abstractAI = slugcat.abstractCreature.abstractAI;
            abstractAI?.SetDestination(slugcat.abstractCreature.pos);

            slugcat.room.AddObject(new ElectricDeath.SparkFlash(slugcat.mainBodyChunk.pos, 5f));
            for (int i = 0; i < 5; i++)
            {
                GreenSparks.GreenSpark spark = new GreenSparks.GreenSpark(slugcat.mainBodyChunk.pos);
                spark.lifeTime = 30;
                slugcat.room.AddObject(spark);
            }
            return true;
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
            return false;
        }
        
    }


}

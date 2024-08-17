using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.fp.Daddy;



// byd这是真超模，我只要在地图上随便走走就能把烟囱天棚堵我门那三只青蜥蜴吓得抱头鼠窜
// TODO: 是时候砍玩家移速和抓握能力了
// 拟态草或成唯一真神
public class CreatureRelationship
{

    public static void Apply()
    {
        // 地毯式排查
        // BigEelAI 算了，利维坦还是算了（
        On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += BigSpiderAI_UpdateDynamicRelationship;
        On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += CentipedeAI_UpdateDynamicRelationship;
        On.CicadaAI.IUseARelationshipTracker_UpdateDynamicRelationship += CicadaAI_UpdateDynamicRelationship;
        // DaddyAI 这是你爹，放尊重点
        // DeerAI 进不去，怎么想都进不去吧！！
        On.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += DropBugAI_UpdateDynamicRelationship;
        // EggBugAI 不用写了，本来就怂
        // GarbageWormAI 这个我用矛都打不到的还是算了
        // MoreSlugcats.InspectorAI 我在一个有香菇的房间放了个监察者，然后他在红色和绿色之间反复横跳，究竟是他红温了还是我生了个圣诞限定款监察者
        On.JetFishAI.IUseARelationshipTracker_UpdateDynamicRelationship += JetFishAI_UpdateDynamicRelationship;
        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += LizardAI_UpdateDynamicRelationship;
        On.MirosBirdAI.IUseARelationshipTracker_UpdateDynamicRelationship += MirosBirdAI_UpdateDynamicRelationship;
        // MouseAI 不用写了，本来就怂
        // NeedleWormAI 没找到那个函数，拉倒吧
        // OverseerAI 这个肯定不用写的
        On.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += ScavengerAI_UpdateDynamicRelationship;
        On.MoreSlugcats.SlugNPCAI.IUseARelationshipTracker_UpdateDynamicRelationship += MoreSlugcats_SlugNPCAI_UpdateDynamicRelationship;
        // SnailAI 没找到那个函数，拉倒吧
        On.SmallNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += SmallNeedleWormAI_UpdateDynamicRelationship;
        // MoreSlugcats.StowawayBugAI
        // TempleGuardAI 呃……
        On.TentaclePlantAI.UpdateDynamicRelationship += TentaclePlantAI_UpdateDynamicRelationship;
        // TubeWormAI 据我苟在下悬挂观察的经验，香菇不吃管虫（是因为肉太少吗
        On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += VultureAI_UpdateDynamicRelationship;
        On.MoreSlugcats.YeekAI.IUseARelationshipTracker_UpdateDynamicRelationship += MoreSlugcats_YeekAI_UpdateDynamicRelationship;

    }


    // Lizards
    // TODO: 他们是否害怕你，在一定程度上和玩家好感度有关，如果玩家还不能直接生吞蜥蜴并且和他们好感度比较高的话，是不会害怕的
    private static CreatureTemplate.Relationship LizardAI_UpdateDynamicRelationship(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && dRelation.trackerRep.representedCreature.realizedCreature != null)
        {
            Player player = dRelation.trackerRep.representedCreature.realizedCreature as Player;
            if (Plugin.playerModules.TryGetValue(player, out var mod) && mod.daddy != null && mod.daddy.controlOfPlayer >= DaddyModule.Control.Throw)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, (int)mod.daddy.controlOfPlayer * 0.2f);
            }
        }
        return orig(self, dRelation);
    }

    // 想了想，还是把这个提到单独写的一栏里吧
    private static CreatureTemplate.Relationship MoreSlugcats_SlugNPCAI_UpdateDynamicRelationship(On.MoreSlugcats.SlugNPCAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, MoreSlugcats.SlugNPCAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, false);
        return result;
    }

    private static CreatureTemplate.Relationship ScavengerAI_UpdateDynamicRelationship(On.ScavengerAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, ScavengerAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, false);
        return result;
    }







    #region other creatures

    // 这玩意复现率太高了，写个函数
    // 蜥蜴和拾荒者之类比较复杂的单独写↑
    /// <summary>
    /// eatOrAfraid 针对玩家的各种捕食者，如果玩家不能生吞了他们的话还是不会害怕，只是捕食欲望会减小
    /// </summary>
    /// <param name="dRelation"></param>
    /// <param name="eatOrAfraid"></param>
    /// <returns></returns>
    public static CreatureTemplate.Relationship AfraidOfPlayer(CreatureTemplate.Relationship result, RelationshipTracker.DynamicRelationship dRelation, bool eatOrAfraid)
    {
        if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && dRelation.trackerRep.representedCreature.realizedCreature != null && Plugin.playerModules.TryGetValue(dRelation.trackerRep.representedCreature.realizedCreature as Player, out var mod) && mod.daddy != null && mod.daddy.controlOfPlayer >= DaddyModule.Control.Throw)
        {
            if (eatOrAfraid)
            {
                if (mod.daddy.controlOfPlayer >= DaddyModule.Control.Stun)
                {
                    return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, (int)mod.daddy.controlOfPlayer * 0.2f);
                }
                if (result.type != CreatureTemplate.Relationship.Type.Ignores) { result.intensity *= 0.5f; }
            }
            result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, (int)mod.daddy.controlOfPlayer * 0.2f);
        }
        return result;
    }





    // 我第一次知道，红树竟然还会害怕
    // 但我得测试一下，如果玩家的触手根本不会去抓红树还是算了
    private static CreatureTemplate.Relationship TentaclePlantAI_UpdateDynamicRelationship(On.TentaclePlantAI.orig_UpdateDynamicRelationship orig, TentaclePlantAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, true);
        return result;
    }

    private static CreatureTemplate.Relationship MirosBirdAI_UpdateDynamicRelationship(On.MirosBirdAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, MirosBirdAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, true);
        return result;
    }

    private static CreatureTemplate.Relationship MoreSlugcats_YeekAI_UpdateDynamicRelationship(On.MoreSlugcats.YeekAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, MoreSlugcats.YeekAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, false);
        return result;
    }

    private static CreatureTemplate.Relationship VultureAI_UpdateDynamicRelationship(On.VultureAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, VultureAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, true);
        return result;
    }

    private static CreatureTemplate.Relationship SmallNeedleWormAI_UpdateDynamicRelationship(On.SmallNeedleWormAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, SmallNeedleWormAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, false);
        return result;
    }

    private static CreatureTemplate.Relationship JetFishAI_UpdateDynamicRelationship(On.JetFishAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, JetFishAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, false);
        return result;
    }

    private static CreatureTemplate.Relationship DropBugAI_UpdateDynamicRelationship(On.DropBugAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DropBugAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, true);
        return result;
    }
    
    // 啊原来你可以给蝉乌贼送东西吃的吗
    // 但他们都吃啥啊（
    private static CreatureTemplate.Relationship CicadaAI_UpdateDynamicRelationship(On.CicadaAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CicadaAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, false);
        return result;
    }

    private static CreatureTemplate.Relationship CentipedeAI_UpdateDynamicRelationship(On.CentipedeAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CentipedeAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, true);
        return result;
    }
    private static CreatureTemplate.Relationship BigSpiderAI_UpdateDynamicRelationship(On.BigSpiderAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigSpiderAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        result = AfraidOfPlayer(result, dRelation, true);
        return result;
    }



    #endregion

}

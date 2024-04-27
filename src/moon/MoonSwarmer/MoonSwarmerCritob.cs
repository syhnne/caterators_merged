using DevInterface;
using Fisobs.Creatures;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;
using CreatureType = CreatureTemplate.Type;

namespace Caterators_by_syhnne.moon.MoonSwarmer;

public class MoonSwarmerCritob : Critob
{
    public static readonly CreatureTemplate.Type MoonSwarmer = new("MoonslugcatSwarmer", true);


    public MoonSwarmerCritob() : base(MoonSwarmer)
    {
        LoadedPerformanceCost = 20f;
        SandboxPerformanceCost = new(linear: 0.6f, exponential: 0.1f);
        ShelterDanger = ShelterDanger.Safe;
        // 是叫这个吗（思考
        CreatureName = "Neuron Fly";
        Icon = new MoonSwarmerIcon();
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate t = new CreatureFormula(this)
        {
            // 回头再写。。
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
            HasAI = true,
            InstantDeathDamage = 3,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Fly),
            TileResistances = new()
            {
                Air = new(0.8f, Allowed),
                Floor = new(1.2f, Allowed),
            },
            ConnectionResistances = new()
            {
                Standard = new(0.9f, Allowed),
                OpenDiagonal = new(0.5f, Allowed),
                ShortCut = new(1.1f, Allowed),
                NPCTransportation = new(1, Allowed),
                OffScreenMovement = new(1, Allowed),
                BetweenRooms = new(1, Allowed),
            },
            DamageResistances = new()
            {
                Base = 10f,
            },
            StunResistances = new()
            {
                Base = 10f,
            }
        }.IntoTemplate();

        t.offScreenSpeed = 10f;
        t.abstractedLaziness = 30;
        t.roamBetweenRoomsChance = 1f;
        t.bodySize = 0.1f;
        t.stowFoodInDen = false;
        t.shortcutSegments = 1;
        t.grasps = 1;
        t.visualRadius = 1000f;
        t.movementBasedVision = 0.5f;
        t.communityInfluence = 0f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.waterPathingResistance = 1f;
        t.canFly = true;
        t.meatPoints = 0;
        t.dangerousToPlayer = 0f;

        t.saveCreature = false;
        return t;
    }


    public override void EstablishRelationships()
    {
        Relationships self = new(MoonSwarmer);

        foreach (var template in StaticWorld.creatureTemplates)
        {
            if (template.quantified)
            {
                self.Ignores(template.type);
                self.IgnoredBy(template.type);
            }
        }
        self.IsInPack(MoonSwarmer, 1f);
        // 为了让它跟着玩家走。。这有用吗（
        self.PlaysWith(CreatureType.Slugcat, 0.5f);
        self.Intimidates(CreatureTemplate.Type.Spider, 0.1f);
        self.Intimidates(CreatureTemplate.Type.SpitterSpider, 0.1f);
        self.Intimidates(CreatureTemplate.Type.BigSpider, 0.1f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
    {
        return new MoonSwarmerAI(acrit);
    }

    public override Creature CreateRealizedCreature(AbstractCreature acrit)
    {
        return new MoonSwarmer(acrit);
    }


    public override string DevtoolsMapName(AbstractCreature acrit)
    {
        return "neu";
    }

    public override Color DevtoolsMapColor(AbstractCreature acrit)
    {
        return Color.white;
    }


    public override ItemProperties? Properties(Creature crit)
    {
        // If you don't need the `forObject` parameter, store one ItemProperties instance as a static object and return that.
        // The CentiShields example demonstrates this.
        if (crit is MoonSwarmer swarmer)
        {
            return new MoonSwarmerProperties(swarmer);
        }

        return null;
    }
}

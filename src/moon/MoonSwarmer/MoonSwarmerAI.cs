using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using UnityEngine;
using RWCustom;

namespace Caterators_by_syhnne.moon.MoonSwarmer;


// 尝试看懂别人写的ai，结果是看不懂一点。先把跟随玩家做出来吧
// 只有管虫的ai我勉强能看懂……

// 啊啊啊啊啊啊虽然很不想，但这下真要大量复制粘贴代码了orz
public class MoonSwarmerAI : ArtificialIntelligence
{

    public SwarmerManager manager;
    public Behavior currentBehavior;
    public int destCounter;
    public WorldCoordinate lastIdlePos;
    public bool notInSameRoom
    {
        get
        {
            return playerPos != null && ((WorldCoordinate)playerPos).room != creature.pos.room;
        }
    }

    // 这是啥 看上去好神奇
    public DebugDestinationVisualizer destVis;
    public moon.MoonSwarmer.MoonSwarmer swarmer
    {
        get
        {
            return this.creature.realizedCreature as MoonSwarmer;
        }
    }
    public WorldCoordinate? playerPos
    {
        get { return manager?.player?.abstractCreature.pos; }
    }

    public enum Behavior
    {
        Idle,
        FollowPlayer,
        // 可能还有一些动画效果
    }




    public MoonSwarmerAI(AbstractCreature abstr) : base(abstr, abstr.world)
    {
        // 这个看字面意思肯定得整一个罢 用来寻路找到玩家在哪
        AddModule(new StandardPather(this, abstr.world, abstr));
        // 这个我猜就是用来追踪其他生物的 加一个逝逝先
        // AddModule(new Tracker(this, 10, 2, 200, 0.5f, 5, 5, 10));
        swarmer.AI = this;
        currentBehavior = Behavior.FollowPlayer;
        destCounter = 0;
        pathFinder.stepsPerFrame = 20;
        manager = swarmer.manager;
    }


    public override void Update()
    {
        base.Update();
        if (destCounter > 0){
            destCounter--;
        }
        // FindPlayer();
        return;
        
        // if (creature.Room != null && creature.Room.shelter) { return; }
        switch (currentBehavior)
        {
            case Behavior.FollowPlayer:
                FindPlayer();
                break;
            case Behavior.Idle:
            default:
                WorldCoordinate? c = RandomDest();
                if (c == null) break;
                if (IdleCoordScore(lastIdlePos) > IdleCoordScore((WorldCoordinate)c) - (float)destCounter)
                {
                    lastIdlePos = (WorldCoordinate)c;
                    // Plugin.Log("swarmer", creature.ID.number, "new idle destination:", c.ToString());
                    this.creature.abstractAI.SetDestination((WorldCoordinate)c);
                }
                break;
        }
    }

    public void FindPlayer()
    {

        WorldCoordinate? coord = playerPos != null? playerPos : null;
        if (coord != null)
        {
            // Plugin.Log("swarmer", creature.ID.number, "findPlayer:", playerPos.ToString());
            WorldCoordinate c = (WorldCoordinate)coord;
            this.creature.abstractAI.SetDestination((WorldCoordinate)c);
        }
    }



    public void SwitchBehavior(MoonSwarmerAI.Behavior behaviour)
    {
        if (behaviour == currentBehavior) return;
        // Plugin.Log("swarmer", creature.ID.number, "behavior switch to:", behaviour.ToString());
        currentBehavior = behaviour;
    }




    public float IdleCoordScore(WorldCoordinate coord)
    {
        if (coord == null || coord.room != creature.pos.room || (swarmer.room != null && swarmer.room.GetTile(coord).Solid)) return float.MaxValue;
        float result = 10000f;

        if (playerPos != null && coord.room != ((WorldCoordinate)playerPos).room) result += 5000f;

        if (swarmer.room != null && swarmer.room.aimap.getAItile(coord).narrowSpace) result += 1000f;
        // 尽量呆在高的地方
        result -= coord.y * 2f;
        // 靠近玩家
        if (swarmer.manager.player.room != null && playerPos != null && coord.room == ((WorldCoordinate)playerPos).room)
        {
            result -=  30f * Custom.Dist(swarmer.manager.player.room.MiddleOfTile(coord), swarmer.manager.player.DangerPos);
        }
        return result;

    }



    public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
    {
        return new PathCost(cost.resistance, cost.legality);
    }


    private WorldCoordinate? RandomDest()
    {
        if (swarmer == null || swarmer.room == null || pathFinder == null || destCounter > 0) return null;
        WorldCoordinate worldCoordinate = new WorldCoordinate(swarmer.room.abstractRoom.index, Random.Range(0, swarmer.room.TileWidth), Random.Range(0, swarmer.room.TileHeight), -1);
        if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
        {
            // this.creature.abstractAI.SetDestination(worldCoordinate);
            destCounter = Random.Range(400, 1200);
        }
        return worldCoordinate;
    }

}

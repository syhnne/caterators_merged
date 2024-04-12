using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using UnityEngine;

namespace Caterators_by_syhnne.moon.MoonSwarmer;


// 尝试看懂别人写的ai，结果是看不懂一点。先把跟随玩家做出来吧
// 只有管虫的ai我勉强能看懂……
public class MoonSwarmerAI : ArtificialIntelligence
{

    public SwarmerManager manager = null;
    public Behavior currentBehavior;
    public int destCounter;

    // 这是啥 看上去好神奇
    public DebugDestinationVisualizer destVis;
    public moon.MoonSwarmer.MoonSwarmer swarmer
    {
        get
        {
            return this.creature.realizedCreature as MoonSwarmer;
        }
    }

    public enum Behavior
    {
        Idle,
        FollowPlayer,
        // 可能还有一些动画效果
    }





    public MoonSwarmerAI(AbstractCreature abstr) : base(abstr, abstr.world)
    {
        base.AddModule(new StandardPather(this, abstr.world, abstr));
        swarmer.AI = this;
        currentBehavior = Behavior.FollowPlayer;
        destCounter = 0;
    }


    public override void Update()
    {
        base.Update();
        destCounter--;
        if (currentBehavior == Behavior.FollowPlayer)
        {
            FindPlayer();
        }
        if (currentBehavior == Behavior.Idle && destCounter < 1)
        {
            RandomDest();
        }
    }

    public void FindPlayer()
    {
        if (manager == null) return;
        WorldCoordinate dest = new WorldCoordinate();
        if (manager.player != null && manager.alive)
        {
            dest = manager.player.abstractCreature.pos;
        }
        if (pathFinder.CoordinateReachableAndGetbackable(dest))
        {
            this.creature.abstractAI.SetDestination(dest);
        }
    }


    private void RandomDest()
    {
        WorldCoordinate worldCoordinate = new WorldCoordinate(swarmer.room.abstractRoom.index, Random.Range(0, swarmer.room.TileWidth), Random.Range(0, swarmer.room.TileHeight), -1);
        if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
        {
            this.creature.abstractAI.SetDestination(worldCoordinate);
            destCounter = Random.Range(400, 1200);
        }
    }

}

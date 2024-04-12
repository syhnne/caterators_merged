using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.moon.MoonSwarmer;




public class SwarmerManager
{
    public Player player;
    public static int maxSwarmer = 7;
    public List<MoonSwarmer> swarmers;

    // 把这项设为false可以一键断开连接
    public bool alive;
    // public List<> swarmers;

    public bool CanConsumeSwarmer
    {
        get
        {
            foreach (MoonSwarmer s in swarmers)
            {
                if (s.State.alive) return true;
            }
            return false;
        }
    }

    public MoonSwarmer? LastAliveSwarmer
    {
        get
        {
            for (int i = swarmers.Count - 1; i >= 0; i--)
            {
                if (swarmers[i].State.alive)
                {
                    return swarmers[i];
                }
            }
            return null;
        }
    }










    public SwarmerManager(Player player)
    {
        swarmers = new();
        this.player = player;
        alive = true;
    }


    public void Update()
    {
        if (player.dead)
        {
            alive = false;  
        }
    }





    public void AddSwarmer()
    {
        AbstractCreature abstr = new AbstractCreature(player.room.world, StaticWorld.GetCreatureTemplate(MoonSwarmerCritob.MoonSwarmer), null, player.abstractCreature.pos, player.room.game.GetNewID());
        player.room.abstractRoom.AddEntity(abstr);
        abstr.RealizeInRoom();
        abstr.realizedObject.firstChunk.pos = player.firstChunk.pos;
        Plugin.Log("spawn MoonSwarmer");

        (abstr.realizedCreature as MoonSwarmer).AI.manager = this;
        swarmers.Add(abstr.realizedCreature as MoonSwarmer);
    }

    public bool KillSwarmer(MoonSwarmer s)
    {
        if (s != null)
        {
            s.isActive = false;
            s.killTag = null;
            s.Die();
            swarmers.Remove(s);
            Plugin.Log("killed, moon has swarmer:", swarmers.Count);
            return true;
        }
        return false;
    }

    public void Destroy()
    {

    }


}

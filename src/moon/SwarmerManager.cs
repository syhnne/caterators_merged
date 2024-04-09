using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.moon;

public class SwarmerManager
{
    public Player player;
    public static int maxSwarmer = 7;
    public int hasSwarmers;
    // public List<> swarmers;

    public SwarmerManager(Player player) 
    { 
        hasSwarmers = 1;
        this.player = player;
    }


    public void Update()
    {

    }


    public bool CanConsumeSwarmer
    {
        get
        {
            return hasSwarmers > 0;
        }
    }

    public bool ConsumeSwarmer()
    {
        if (hasSwarmers > 0)
        {
            hasSwarmers--;
            Plugin.Log("moon has swarmer:", hasSwarmers);
            return true;
        }
        return false;
    }

    public void Destroy()
    {

    }


}

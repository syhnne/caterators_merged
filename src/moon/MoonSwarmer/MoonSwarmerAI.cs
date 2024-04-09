using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.moon.MoonSwarmer;

public class MoonSwarmerAI : ArtificialIntelligence
{

    public moon.SwarmerManager manager;

    public MoonSwarmerAI(AbstractCreature abstr, World world, moon.SwarmerManager manager) : base(abstr, world)
    {
        this.manager = manager;

    }



}

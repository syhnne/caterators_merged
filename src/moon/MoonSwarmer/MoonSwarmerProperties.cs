using Fisobs.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.moon.MoonSwarmer;

public class MoonSwarmerProperties : ItemProperties
{

    private readonly MoonSwarmer swarmer;

    public MoonSwarmerProperties(MoonSwarmer s)
    {
        this.swarmer = s;
    }

    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.OneHand;
    }

}

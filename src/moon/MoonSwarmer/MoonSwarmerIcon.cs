using Fisobs.Core;
using UnityEngine;

namespace Caterators_by_syhnne.moon.MoonSwarmer;

public class MoonSwarmerIcon : Icon
{
    public override int Data(AbstractPhysicalObject apo)
    {
        return 1;
    }

    public override Color SpriteColor(int data)
    {
        return Color.white;
    }

    public override string SpriteName(int data)
    {
        return "Symbol_Neuron";
    }
}


using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Drawing.Text;
using RWCustom;
using Noise;
using System.Collections.Generic;
using MoreSlugcats;
using BepInEx.Logging;
using Smoke;
using Random = UnityEngine.Random;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using System.Runtime.CompilerServices;
using Menu;

namespace Caterators_by_syhnne;








internal static class Enums
{

    public static bool IsCaterator(this SaveState saveState) => IsCaterator(saveState.saveStateNumber);

    public static bool IsCaterator(this RainWorldGame game) => game.IsStorySession && IsCaterator(game.StoryCharacter);
    
    public static bool IsCaterator(SlugcatStats.Name name)
    {
        if (name == FPname || name == SRSname || name == NSHname || name == Moonname || name == test) { return true; }
        else { return false; }
    }


    


    public static readonly SlugcatStats.Name FPname = new("FPslugcat", false);
    public static readonly SlugcatStats.Name SRSname = new("SRSslugcat", false);
    public static readonly SlugcatStats.Name NSHname = new("NSHslugcat", false);
    public static readonly SlugcatStats.Name Moonname = new("Moonslugcat", false);
    public static readonly SlugcatStats.Name test = new("placeholder_syhnne", false);
}

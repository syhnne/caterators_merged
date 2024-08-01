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
using static Caterators_by_syhnne.srs.OxygenMaskModules;
using SlugBase.SaveData;
using SlugBase;
using SlugBase.Assets;

namespace Caterators_by_syhnne.moon;

public class CustomMenu
{

    public static MenuScene.SceneID MoonSlugcatScene(int neurons)
    {
        if (neurons < 1 || neurons > 5)
        {
            Plugin.Log("!! MoonSlugcatScene : invalid neurons");
            return new("");
        }
        return new("Slugcat_moon_" + neurons.ToString());
    }

}

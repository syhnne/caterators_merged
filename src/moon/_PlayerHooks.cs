using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_by_syhnne.moon;

public class PlayerHooks
{




    public static void Apply()
    {
        
        /*new Hook(
                typeof(SlugcatStats).GetProperty(nameof(SlugcatStats.runspeedFac), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                get_SlugcatStats_runspeedFac
                );*/

        new Hook(
                typeof(Player).GetProperty(nameof(Player.isRivulet), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                get_Player_isRivulet
                );
    }


    private delegate bool Player_isRivulet(Player self);
    private static bool get_Player_isRivulet(Player_isRivulet orig, Player self)
    {
        var result = orig(self);
        if (self.SlugCatClass == Enums.Moonname && Plugin.playerModules.TryGetValue(self, out var module) && module.swarmerManager != null && module.swarmerManager.agility)
        {
            result = true;
        }
        return result;
    }


    /*private delegate bool SlugcatStats_runspeedFac(SlugcatStats self);
    private static bool get_SlugcatStats_runspeedFac(SlugcatStats_runspeedFac orig, SlugcatStats self)
    {
        var result = orig(self);
        if (self.name == Enums.Moonname && Plugin.playerModules.TryGetValue(self., out var module) && module.swarmerManager != null && module.swarmerManager.agility)
        {
            result = true;
        }
        return result;
    }*/


}

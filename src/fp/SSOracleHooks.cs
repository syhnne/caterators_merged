using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_by_syhnne.fp;

public class SSOracleHooks
{
    public static void Apply()
    {
        On.SSOracleBehavior.storedPearlOrbitLocation += SSOracleBehavior_storedPearlOrbitLocation;
        On.PebblesPearl.Update += PebblesPearl_Update;
        new Hook(
            typeof(SSOracleBehavior).GetProperty(nameof(SSOracleBehavior.EyesClosed), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            SSOracleBehavior_EyesClosed
            );
    }




    private delegate bool orig_EyesClosed(SSOracleBehavior self);
    private static bool SSOracleBehavior_EyesClosed(orig_EyesClosed orig, SSOracleBehavior self)
    {
        var result = orig(self);
        if (self.player != null && self.player.SlugCatClass == Enums.FPname)
        {
            result = true;
        }
        return result;
    }





    // 储存珍珠。纯复制粘贴。。
    private static Vector2 SSOracleBehavior_storedPearlOrbitLocation(On.SSOracleBehavior.orig_storedPearlOrbitLocation orig, SSOracleBehavior self, int index)
    {
        if (self.oracle.room.game.IsStorySession && self.oracle.room.game.StoryCharacter == Enums.FPname)
        {
            float num = 5f;
            float num2 = (float)index % num;
            float num3 = Mathf.Floor((float)index / num);
            float num4 = num2 * 0.5f;
            return new Vector2(615f, 100f) + new Vector2(num2 * 26f, (num3 + num4) * 18f);
        }
        return orig(self, index);
    }



    // 珍珠会绕着猫转
    // 但是效果没有我想象中那么好（。）所以先关了
    private static void PebblesPearl_Update(On.PebblesPearl.orig_Update orig, PebblesPearl self, bool eu)
    {
        orig(self, eu);
        if (self.hoverPos == null && self.oracle != null && self.oracle.room == self.room)
        {
            if (!self.oracle.Consious) self.orbitObj = null;
            // 写这个&&false 是因为我把console砍了
            else if ((self.oracle.oracleBehavior as SSOracleBehavior).player != null && false)
            {
                self.orbitObj = (self.oracle.oracleBehavior as SSOracleBehavior).player;
            }
            else 
            { 
                self.orbitObj = self.oracle; 
            }

        }

    }


}

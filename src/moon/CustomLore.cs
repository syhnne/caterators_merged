using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;

namespace Caterators_by_syhnne.moon;

public class CustomLore
{




    public static void Apply()
    {
        On.MoreSlugcats.CLOracleBehavior.Update += CLOracleBehavior_Update;
    }


    // 啊 多洗爹
    private static void CLOracleBehavior_Update(On.MoreSlugcats.CLOracleBehavior.orig_Update orig, CLOracleBehavior self, bool eu)
    {
        try
        {
            orig(self, eu);
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
        }
    }
}

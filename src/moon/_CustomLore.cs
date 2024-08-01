using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;

namespace Caterators_by_syhnne.moon;

public class CustomLore
{

    public static void Room_Loaded(Room self)
    {
        // 不是。咋不干活啊。给我动换啊。
        // 反了你了，怎么连日志都没得
        if (self.abstractRoom.name == "SL_AI")
        {
            self.AddObject(new roomScript.MoonStartCutscene(self));
        }
    }





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

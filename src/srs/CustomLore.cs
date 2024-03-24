using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_merged.srs;

public class CustomLore
{













    public static void Apply()
    {
        On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;
    }



    // 打开外层空间的门。这是你唯一出去的路了，毕竟不能走根源设施（但说实话被困外层空间挺难顶的，给他们一个3-4级的初始业力罢
    private static bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
    {
        if (self.room.game.IsStorySession && self.room.game.StoryCharacter == Enums.SRSname)
        {
            return true;
        }
        return orig(self);
    }
}

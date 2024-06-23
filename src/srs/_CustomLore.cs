using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Caterators_by_syhnne.srs.OxygenMaskModules;

namespace Caterators_by_syhnne.srs;

public class CustomLore
{

    public static void Room_Loaded(Room self)
    {
        if (self.game.GetDeathPersistent() == null) { return; }
        if ((self.water || self.blizzard) && !self.game.GetDeathPersistent().SRSwaterMessage)
        {
            self.AddObject(new roomScript.SRSwaterMessage(self));
        }

        /*if (self.abstractRoom.name == "SS_AI")
        {
            Plugin.Log("Add OxygenMask");
            AbstractPhysicalObject abstr = new OxygenMaskAbstract(self.game.world, new WorldCoordinate(self.abstractRoom.index, -1, -1, 0), self.game.GetNewID(), 3);
            abstr.destroyOnAbstraction = true;
            self.abstractRoom.AddEntity(abstr);
            abstr.RealizeInRoom();
            (abstr.realizedObject as OxygenMask).firstChunk.pos = new Vector2(300f, 300f);
        }*/
        if (self.abstractRoom.name == "OE_RAIL03" && !self.game.GetDeathPersistent().SRSstartTutorial)
        {
            self.AddObject(new roomScript.SRSstartTutorial(self));
        }
    }











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

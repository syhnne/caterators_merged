using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Caterators_by_syhnne.srs.OxygenMaskModules;

namespace Caterators_by_syhnne.nsh;

public class CustomLore
{
    public static void Room_Loaded(Room self)
    {
        // TODO: 等我想好他出生点应该在哪，再写那个房间
        if (self.game.GetDeathPersistent() == null) { return; }
        if (self.abstractRoom.name == "LF_S01" && !self.game.GetDeathPersistent().NSHinventoryTutorial)
        {
            self.AddObject(new roomScript.NSHinventoryTutorial(self));
        }
        else if (self.abstractRoom.name == "LF_D06" && !self.game.GetDeathPersistent().NSHneuronTutorial)
        {
            self.AddObject(new roomScript.NSHneuronTutorial(self));
        }
    }




}

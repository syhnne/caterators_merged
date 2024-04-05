using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.effects;

public class EffectHooks
{
    // 不行 pom太烧脑了 我还是得使用笨方法



    public static void ApplyEffects(Room room)
    {
        string name = room.abstractRoom.name;
        
        if (name == "RL_Z01")
        {
            
            for (int i = 0; i < room.roomSettings.effects.Count; i++)
            {
                if (room.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.RoofTopView)
                {
                    room.AddObject(new MyRoofTopView(room, room.roomSettings.effects[i]));
                }
            }
            Plugin.Log("effects applied for", name);
        }
    }

}

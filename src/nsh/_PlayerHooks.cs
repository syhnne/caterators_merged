using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.nsh;


// 迟早要写，先把apply挂上省得我忘（错误的，迟早要写也不是这里要写啊）
// 现在的想法是，每个雨循环能拿到一个复活用神经元，可以复活自己或者队友
// 并且有一个四格背包，背包里所有东西都是从背上拿下来的（大雾
// 感觉我该找那位太太要技能设计的授权了 不能说是毫不相干 只能说是一模一样

// 你别看这里啥也没有……实际上这个技能写得我脑子要爆炸了
public class PlayerHooks
{


    public static void Player_ctor(Player self, AbstractCreature abstractCreature, World world)
    {
        if (abstractCreature.Room.world.game.IsStorySession)
        {
            AbstractPhysicalObject abstractPhysicalObject = new nsh.ReviveSwarmerModules.ReviveSwarmerAbstract(world, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), world.game.GetNewID(), true);
            abstractCreature.Room.AddEntity(abstractPhysicalObject);
            abstractPhysicalObject.RealizeInRoom();
        }
    }






    public static void Apply()
    {
        
    }


    




}

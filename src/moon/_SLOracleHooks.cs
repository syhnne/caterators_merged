using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.moon;


// 想做个开场动画……但这得取决于剧情咋写了 啊啊啊这下不得不摇人了！！
public class SLOracleHooks
{

    public static void Apply()
    {
        IL.Oracle.SetUpSwarmers += IL_Oracle_SetUpSwarmers;
    }


    // ？我有病啊，我直接把ripmoon写成true不就完事了吗
    private static void IL_Oracle_SetUpSwarmers(ILContext il)
    {
        // 29，使ripMoon返回true，生成0个神经元
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            i => i.MatchLdfld<SaveState>("deathPersistentSaveData"),
            i => i.MatchLdfld<DeathPersistentSaveData>("ripMoon")
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, Oracle, bool>>((orig, oracle) =>
            {
                if (oracle.room.game.StoryCharacter == Enums.Moonname)
                {
                    return true;
                }
                return orig;
            });
        }
    }


}

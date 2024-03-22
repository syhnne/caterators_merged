using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Noise;

namespace Caterators_merged.srs;

internal class PlayerHooks
{

    public static void Player_Update(Player self, bool eu)
    {
        // 为了防止玩家发现虚空流体就是水这个事实（。
        if (!self.Malnourished && self.Submersion > 0.2f && self.room.abstractRoom.name != "SB_L01" && self.room.abstractRoom.name != "FR_FINAL")
        {
            WaterDeath(self, self.room);
        }
    }






    // 不知道咋写，先凑合一下（
    // 啊啊啊啊啊啊啊啊啊啊啊啊啊我的耳朵！！
    // 嗷 原来是他被调用好几次（
    public static void WaterDeath(Player player, Room room)
    {
        if (player.dead) { return; }
        for (int i = 0; i < player.grasps.Length; i++)
        {
            if (player.grasps[i] != null && (player.grasps[i].grabbed is OxygenMaskModules.OxygenMask))
            {
                return;
            }
            else if (player.grasps[i] != null && player.grasps[i].grabbed is BubbleGrass)
            {
                BubbleGrass bubbleGrass = player.grasps[i].grabbed as BubbleGrass;
                Plugin.Log("bubbleGrass oxygen left:", bubbleGrass.AbstrBubbleGrass.oxygenLeft);
                if (player.animation == Player.AnimationIndex.SurfaceSwim)
                {
                    bubbleGrass.AbstrBubbleGrass.oxygenLeft = Mathf.Max(0f, bubbleGrass.AbstrBubbleGrass.oxygenLeft - 0.0009090909f);
                }
                if (bubbleGrass.AbstrBubbleGrass.oxygenLeft > 0f) return;
            }
        }
        // 咋说，这玩意儿应该不能被放在肚子里，这很奇怪（
        // if (player.objectInStomach is OxygenMaskModules.OxygenMaskAbstract) { return; }

        Plugin.Log("waterdeath");
        Vector2 vector = Vector2.Lerp(player.firstChunk.pos, player.firstChunk.lastPos, 0.35f);
        room.PlaySound(SoundID.Firecracker_Burn, vector);
        room.ScreenMovement(new Vector2?(vector), default(Vector2), 1.3f);
        room.InGameNoise(new InGameNoise(vector, 8000f, player, 1f));
        player.Die();
    }


}

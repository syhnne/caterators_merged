using System;
using System.Collections.Generic;
using MoreSlugcats;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Caterators_by_syhnne.moon;



public class PlayerGraphicsModule
{

    // 添加一个很亮的光效，但光下不亮，而且没有保暖作用（
    public static void PlayerGraphics_Update(PlayerGraphics self)
    {
        if (self.lightSource == null)
        {
            self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, Color.Lerp(self.player.ShortCutColor(), Color.white, 0.5f), self.player);
            self.lightSource.requireUpKeep = true;
            self.lightSource.setRad = 600f;
            self.lightSource.setAlpha = 1.5f;
            self.player.room.AddObject(self.lightSource);
        }
    }

}

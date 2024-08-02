using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caterators_by_syhnne.fp.Daddy;
using UnityEngine;

namespace Caterators_by_syhnne.fp;

// 现在是2024年8月1日晚上17点51分，我在此隆重宣布：写完这玩意儿我就开摆
// 他妈的，两个月前说这个东西只是想口嗨来着，怎么就写起来了
// 他妈的。我给自己立了什么见鬼的flag。这可不是两三下就能完事的东西啊。这比月姐那个神经元还恐怖。

public class DaddyModule : IDrawable
{
    public WeakReference<Player> owner;
    public Player player { get {
            if (owner.TryGetTarget(out Player player)) { return player; }
            return null;
    }}
    public CustomDaddyTentacle[] tentacles;

    public int numberOfSprites;
    public int startSprite;
    

    // 后面这个数会改
    public DaddyModule(Player player, float tentacleLength)
    {
        this.owner = new(player);
        tentacles = new CustomDaddyTentacle[2]
        {
            new(this, player.bodyChunks[1], tentacleLength),
            new(this, player.bodyChunks[1], tentacleLength * 0.7f),
        };
        foreach (var t in tentacles)
        {
            numberOfSprites += t.graphics.numberOfSprites;
        }
    }

    public void Update()
    {
        foreach (var t in tentacles)
        {
            t.Update();
            t.limp = !player.Consious;
            
        }
        
        // 好吧，数据告诉我这个触手穿过了地板，正在玩家下方做单摆运动
    }

    public void NewRoom(Room room)
    {
        foreach (var t in tentacles)
        {
            t.NewRoom(room);
        }
            
    }




    #region graphics

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        int sprite = startSprite;
        foreach (var t in tentacles)
        {
            t.graphics.startSprite = sprite;
            sprite += t.graphics.numberOfSprites;
            t.graphics.InitiateSprites(sLeaser, rCam);
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        foreach (var t in tentacles)
        {
            t.graphics.DrawSprite(sLeaser, rCam, timeStacker, camPos);
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        foreach(var t in tentacles)
        {
            t.graphics.ApplyPalette(sLeaser, rCam, palette);
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
    }

    #endregion

}

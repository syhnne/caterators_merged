using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Drawing.Text;
using RWCustom;
using Noise;
using System.Collections.Generic;
using MoreSlugcats;
using BepInEx.Logging;
using Smoke;
using Random = UnityEngine.Random;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Menu.Remix.MixedUI;
using System.ComponentModel;
using System.Linq;

namespace Caterators_by_syhnne._public;



// 想了一些阴间小寄巧解决applypalette的问题，那就是用减法，在贴图上只画缺的颜色，他和玩家自身颜色正片叠底混合起来后就是我要的颜色。
// 对于nsh的围巾来说意外地非常好用。因为我选了一个很深的围巾颜色，他的绿色跟一种发紫的亮粉色叠起来刚好就是那个颜色
// srs那个更好办了，红色和什么叠起来都是红的
// 这个办法唯独对月姐不生效（实际上fp那边效果也不是很好，但他那个时间线冻不死猫，所以我不管了（）所以脑门上的标志得单独贴图（
public class PlayerGraphicsModule
{

    public static void Apply()
    {
        On.Player.SlugOnBack.ChangeOverlap += Player_SlugOnBack_ChangeOverlap;

        On.PlayerGraphics.ctor += PlayerGraphics_ctor;
        On.PlayerGraphics.Update += PlayerGraphics_Update;

        On.PlayerGraphics.InitiateSprites += InitiateSprites;
        On.PlayerGraphics.AddToContainer += AddToContainer;
        On.PlayerGraphics.DrawSprites += DrawSprites;
        On.PlayerGraphics.ApplyPalette += ApplyPalette;

        srs.PlayerGraphicsModule.Apply();
    }




    // 防止联机背背时图层出问题
    private static void Player_SlugOnBack_ChangeOverlap(On.Player.SlugOnBack.orig_ChangeOverlap orig, Player.SlugOnBack self, bool newOverlap)
    {
        orig(self, newOverlap);
        if (self.slugcat == null) { return; }
        if (self.slugcat.SlugCatClass == Enums.SRSname && (self.slugcat.graphicsModule as PlayerGraphics).tailSpecks != null)
        {
            for (int i = 0; i < self.owner.room.game.cameras.Length; i++)
            {
                // 这个会有用吗
                self.owner.room.game.cameras[i].MoveObjectToContainer(self.slugcat.graphicsModule as PlayerGraphics, self.owner.room.game.cameras[i].ReturnFContainer(!newOverlap ? "Background" : "Midground"));
            }

        }
        else if (self.slugcat.SlugCatClass == Enums.NSHname && Plugin.playerModules.TryGetValue(self.slugcat, out var module) && module.nshScarf != null)
        {
            for (int i = 0; i < self.owner.room.game.cameras.Length; i++)
            {
                self.owner.room.game.cameras[i].MoveObjectToContainer(module.nshScarf, self.owner.room.game.cameras[i].ReturnFContainer(!newOverlap ? "Background" : "Midground"));
            }
        }
    }





    private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.player == null) { return; }
        if (self.player.SlugCatClass == Enums.SRSname)
        {
            srs.PlayerGraphicsModule.PlayerGraphics_ctor(self, ow);
        }
        else if (self.player.SlugCatClass == Enums.NSHname)
        {
            nsh.PlayerGraphicsModule.PlayerGraphics_ctor(self, ow);
        }
    }



    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        if (self.player.SlugCatClass == Enums.SRSname && Plugin.playerModules.TryGetValue(self.player, out var module))
        {
            srs.PlayerGraphicsModule.PlayerGraphics_Update(self, module);
        }
        else if (self.player.SlugCatClass == Enums.Moonname)
        {
            moon.PlayerGraphicsModule.PlayerGraphics_Update(self);
        }
        orig(self);
        if (self.player.SlugCatClass == Enums.NSHname && Plugin.playerModules.TryGetValue(self.player, out var module2))
        {
            module2.nshScarf?.Update();
        }
    }


    private static void InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.player.SlugCatClass == Enums.SRSname && self.tailSpecks != null)
        {
            srs.PlayerGraphicsModule.InitiateSprites(self, sLeaser, rCam);
        }
        else if (self.player.SlugCatClass == Enums.FPname)
        {
            fp.PlayerGraphicsModule.InitiateSprites(self, sLeaser, rCam);
        }
        else if (self.player.SlugCatClass == Enums.NSHname)
        {
            nsh.PlayerGraphicsModule.InitiateSprites(self, sLeaser, rCam);
        }
    }



    private static void AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);
        if (self.player.SlugCatClass == Enums.SRSname && self.tailSpecks != null)
        {
            srs.PlayerGraphicsModule.AddToContainer(self, sLeaser, rCam, newContatiner);
        }
        else if (self.player.SlugCatClass == Enums.NSHname)
        {
            nsh.PlayerGraphicsModule.AddToContainer(self, sLeaser, rCam, newContatiner);
        }
    }


    private static void DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.player.SlugCatClass == Enums.SRSname && self.tailSpecks != null)
        {
            srs.PlayerGraphicsModule.DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
        }
        else if (self.player.SlugCatClass == Enums.FPname)
        {
            fp.PlayerGraphicsModule.DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
        }
        else if (self.player.SlugCatClass == Enums.NSHname)
        {
            nsh.PlayerGraphicsModule.DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
        }
    }


    private static void ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.player.SlugCatClass == Enums.SRSname)
        {
            srs.PlayerGraphicsModule.ApplyPalette(self, sLeaser, rCam, palette);
        }
        else if (self.player.SlugCatClass == Enums.FPname)
        {
            fp.PlayerGraphicsModule.ApplyPalette(self, sLeaser, rCam, palette);
        }
        else if (self.player.SlugCatClass == Enums.NSHname)
        {
            nsh.PlayerGraphicsModule.ApplyPalette(self, sLeaser, rCam, palette);
        }
    }
}

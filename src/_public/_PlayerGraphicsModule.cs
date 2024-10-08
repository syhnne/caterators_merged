﻿using System;
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
using System.Xml.Schema;

namespace Caterators_by_syhnne._public;




/*
     * 0: "BodyA"
     * 1: "HipsA"
     * 2: tail
     * 3: "HeadA0"
     * 4: "LegsA0"
     * 5: "PlayerArm0", sLeaser.sprites[5].scaleY = -1f;
     * 6: "PlayerArm0"
     * 7: "OnTopOfTerrainHand"
     * 8: "OnTopOfTerrainHand", sLeaser.sprites[8].scaleX = -1f;
     * 9: "FaceA0"
     * 10: "Futile_White", sLeaser.sprites[10].shader = rCam.game.rainWorld.Shaders["FlatLight"];
     * 11: "pixel"
     */




// 想了一些阴间小寄巧解决applypalette的问题，那就是用减法，在贴图上只画缺的颜色，他和玩家自身颜色正片叠底混合起来后就是我要的颜色。
// 对于nsh的围巾来说意外地非常好用。因为我选了一个很深的围巾颜色，他的绿色跟一种发紫的亮粉色叠起来刚好就是那个颜色
// srs那个更好办了，红色和什么叠起来都是红的
// 这个办法唯独对月姐不生效（实际上fp那边效果也不是很好，但他那个时间线冻不死猫，所以我不管了（）所以脑门上的标志得单独贴图（
public class PlayerGraphicsModule
{

    public static void Apply()
    {

        On.PlayerGraphics.ctor += PlayerGraphics_ctor;
        On.PlayerGraphics.Update += PlayerGraphics_Update;

        On.PlayerGraphics.InitiateSprites += InitiateSprites;
        On.PlayerGraphics.AddToContainer += AddToContainer;
        On.PlayerGraphics.DrawSprites += DrawSprites;
        On.PlayerGraphics.ApplyPalette += ApplyPalette;

        // On.RoomCamera.SpriteLeaser.RemoveAllSpritesFromContainer += RoomCamera_SpriteLeaser_RemoveAllSpritesFromContainer;

        srs.PlayerGraphicsModule.Apply();
    }


    // 试一下能不能修
    /*private static void RoomCamera_SpriteLeaser_RemoveAllSpritesFromContainer(On.RoomCamera.SpriteLeaser.orig_RemoveAllSpritesFromContainer orig, RoomCamera.SpriteLeaser self)
    {
        for (int i = 0; i < self.sprites.Length; i++)
        {
            self.sprites[i]?.RemoveFromContainer();
        }
        if (self.containers != null)
        {
            for (int j = 0; j < self.containers.Length; j++)
            {
                self.containers[j]?.RemoveFromContainer();
            }
        }
    }*/











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
        else if (self.player.SlugCatClass == Enums.Moonname)
        {
            moon.PlayerGraphicsModule.PlayerGraphics_ctor(self, ow);
        }
        else if (self.player.SlugCatClass == Enums.FPname)
        {
            fp.PlayerGraphicsModule.PlayerGraphics_ctor(self, ow);
        }
    }



    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        bool getModule = Plugin.playerModules.TryGetValue(self.player, out var module);
        if (self.player.SlugCatClass == Enums.SRSname && getModule)
        {
            srs.PlayerGraphicsModule.PlayerGraphics_Update(self, module);
        }
        else if (self.player.SlugCatClass == Enums.Moonname)
        {
            moon.PlayerGraphicsModule.PlayerGraphics_Update(self);
        }
        orig(self);
        if (self.player.SlugCatClass == Enums.NSHname && getModule)
        {
            module.nshScarf?.Update();
        }
        if (self.player.SlugCatClass == Enums.Moonname && getModule)
        {
            module.gills?.Update();
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
        else if (self.player.SlugCatClass == Enums.Moonname)
        {
            moon.PlayerGraphicsModule.InitiateSprites(self, sLeaser, rCam);
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
        else if (self.player.SlugCatClass == Enums.Moonname)
        {
            moon.PlayerGraphicsModule.AddToContainer(self, sLeaser, rCam, newContatiner);
        }
        else if (self.player.SlugCatClass == Enums.FPname)
        {
            fp.PlayerGraphicsModule.AddToContainer(self, sLeaser, rCam, newContatiner);
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
        else if (self.player.SlugCatClass == Enums.Moonname)
        {
            moon.PlayerGraphicsModule.DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
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
        else if (self.player.SlugCatClass == Enums.Moonname)
        {
            moon.PlayerGraphicsModule.ApplyPalette(self, sLeaser, rCam, palette);
        }
    }
}

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

namespace Caterators_merged;

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

        srs.PlayerGraphicsModule.Apply();
    }




    private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if(self.player == null) { return; }
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

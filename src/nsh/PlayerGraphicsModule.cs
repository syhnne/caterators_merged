using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_merged.nsh;

public class PlayerGraphicsModule
{
    internal static readonly Color32 bodyColor = new Color32(122, 208, 109, 255);
    internal static readonly Color32 scarfColor = new Color32(144, 92, 157, 255);
    internal static readonly List<int> ColoredBodyParts = new List<int>() { 3, };



    public static void PlayerGraphics_ctor(PlayerGraphics self, PhysicalObject ow)
    {
        
        if (Plugin.playerModules.TryGetValue(self.player, out var module) && module.playerName == Enums.NSHname)
        {
            Plugin.Log("nshScarf added");
            module.nshScarf = new Scarf(self, 13);
        }
        
    }








    public static void InitiateSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (!Plugin.playerModules.TryGetValue(self.player, out var module) || module.playerName != Enums.NSHname) { return; }
        sLeaser.RemoveAllSpritesFromContainer();
        sLeaser.sprites = new FSprite[13 + module.nshScarf.numberOfSprites];

        sLeaser.sprites[0] = new FSprite("BodyA", true);
        sLeaser.sprites[0].anchorY = 0.7894737f;
        if (self.RenderAsPup)
        {
            sLeaser.sprites[0].scaleY = 0.5f;
        }
        sLeaser.sprites[1] = new FSprite("HipsA", true);
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
        {
            new TriangleMesh.Triangle(0, 1, 2),
            new TriangleMesh.Triangle(1, 2, 3),
            new TriangleMesh.Triangle(4, 5, 6),
            new TriangleMesh.Triangle(5, 6, 7),
            new TriangleMesh.Triangle(8, 9, 10),
            new TriangleMesh.Triangle(9, 10, 11),
            new TriangleMesh.Triangle(12, 13, 14),
            new TriangleMesh.Triangle(2, 3, 4),
            new TriangleMesh.Triangle(3, 4, 5),
            new TriangleMesh.Triangle(6, 7, 8),
            new TriangleMesh.Triangle(7, 8, 9),
            new TriangleMesh.Triangle(10, 11, 12),
            new TriangleMesh.Triangle(11, 12, 13)
        };
        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[2] = triangleMesh;
        if (Futile.atlasManager.DoesContainElementWithName("nsh_HeadA0"))
        {
            Plugin.Log("nsh_HeadA0 loaded");
            sLeaser.sprites[3] = new FSprite("nsh_HeadA0", true);
        }
        else
        {
            Plugin.Log("nsh_HeadA0 NOT FOUND");
            sLeaser.sprites[3] = new FSprite("HeadA0", true);
        }
        sLeaser.sprites[4] = new FSprite("LegsA0", true);
        sLeaser.sprites[4].anchorY = 0.25f;
        sLeaser.sprites[5] = new FSprite("PlayerArm0", true);
        sLeaser.sprites[5].anchorX = 0.9f;
        sLeaser.sprites[5].scaleY = -1f;
        sLeaser.sprites[6] = new FSprite("PlayerArm0", true);
        sLeaser.sprites[6].anchorX = 0.9f;
        sLeaser.sprites[7] = new FSprite("OnTopOfTerrainHand", true);
        sLeaser.sprites[8] = new FSprite("OnTopOfTerrainHand", true);
        sLeaser.sprites[8].scaleX = -1f;
        sLeaser.sprites[9] = new FSprite("FaceA0", true);
        sLeaser.sprites[11] = new FSprite("pixel", true);
        sLeaser.sprites[11].scale = 5f;
        sLeaser.sprites[10] = new FSprite("Futile_White", true);
        sLeaser.sprites[10].shader = rCam.game.rainWorld.Shaders["FlatLight"];

        self.gown.InitiateSprite(self.gownIndex, sLeaser, rCam);

        module.nshScarf.InitiateSprite(sLeaser, rCam);

        self.AddToContainer(sLeaser, rCam, null);
    }






    public static void AddToContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (!Plugin.playerModules.TryGetValue(self.player, out var module) || module.playerName != Enums.NSHname) { return; }
        sLeaser.RemoveAllSpritesFromContainer();
        Plugin.Log(sLeaser.sprites.Count());
        // if (sLeaser.sprites.Count() <= module.nshScarf.startSprite) return;
        // rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[module.nshScarf.startSprite]);

        // 确认了 就是orig给我把围巾添加到foreground图层导致的 srs的尾巴也一个道理
        // 回头我自己把orig里的功能都复现一遍 不用他的了
        // return;

        sLeaser.RemoveAllSpritesFromContainer();
        newContatiner ??= rCam.ReturnFContainer("Midground");
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i == module.nshScarf.startSprite)
            {
                rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[i]);
            }
            else if (i == module.nshScarf.startSprite + 1)
            {
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[i]);
            }
            else if (i == self.gownIndex)
            {
                newContatiner = rCam.ReturnFContainer("Items");
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
            else
            {
                if ((i <= 6 || i >= 9) && i <= 9)
                {
                    newContatiner.AddChild(sLeaser.sprites[i]);
                }
                else
                {
                    rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
                }
            }

        }
    }





    public static void DrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (!Plugin.playerModules.TryGetValue(self.player, out var module) || module.playerName != Enums.NSHname) { return; }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (sLeaser.sprites[i].element.name.StartsWith(sLeaser.sprites[i].element.name))
            {
                if (Futile.atlasManager.DoesContainElementWithName("nsh_" + sLeaser.sprites[i].element.name))
                {
                    sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(sLeaser.sprites[i].element.name.Replace(sLeaser.sprites[i].element.name, "nsh_" + sLeaser.sprites[i].element.name));
                }

            }
        }
        module.nshScarf?.DrawSprite(sLeaser, rCam, timeStacker, camPos);
    }









    public static void ApplyPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        for (int i = 0; i < 12; i++)
        {
            if (ColoredBodyParts.Contains(i))
            {
                sLeaser.sprites[i].color = new Color(1f, 1f, 1f);
            }
            else
            {
                sLeaser.sprites[i].color = bodyColor;
            }
        }
        if (Plugin.playerModules.TryGetValue(self.player, out var module))
        {
            module.nshScarf?.ApplyPalette(sLeaser, rCam, palette);
        }
    }

}

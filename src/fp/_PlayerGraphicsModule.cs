using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace Caterators_by_syhnne.fp;

public class PlayerGraphicsModule
{
    private static readonly Color32 bodyColor_hard = new Color32(254, 104, 202, 255);
    private static readonly Color eyesColor_hard = new Color(1f, 1f, 1f);
    private static readonly List<int> ColoredBodyParts = new List<int>() { 2, 3, 5, 6, 7, 8, 9, };

    public static void InitiateSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.RemoveAllSpritesFromContainer();
        sLeaser.sprites = new FSprite[13];

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
        if (Futile.atlasManager.DoesContainElementWithName("fp_tail"))
        {
            triangleMesh = new TriangleMesh("fp_tail", tris, false, false);
        }
        else
        {
            Plugin.Log("sprite fp_tail NOT FOUND");
        }
        FAtlas fatlas = Futile.atlasManager.LoadAtlas("atlases/fp_tail");
        triangleMesh.UVvertices[0] = fatlas._elementsByName["fp_tail"].uvBottomLeft;
        triangleMesh.UVvertices[1] = fatlas._elementsByName["fp_tail"].uvTopLeft;
        triangleMesh.UVvertices[13] = fatlas._elementsByName["fp_tail"].uvTopRight;
        triangleMesh.UVvertices[14] = fatlas._elementsByName["fp_tail"].uvBottomRight;
        float num = (triangleMesh.UVvertices[13].x - triangleMesh.UVvertices[1].x) / 6f;
        for (int i = 2; i < 14; i += 2)
        {
            triangleMesh.UVvertices[i].x = (float)((double)fatlas._elementsByName["fp_tail"].uvBottomLeft.x + (double)num * 0.5 * (double)i);
            triangleMesh.UVvertices[i].y = fatlas._elementsByName["fp_tail"].uvBottomLeft.y;
        }
        for (int j = 3; j < 13; j += 2)
        {
            triangleMesh.UVvertices[j].x = (float)((double)fatlas._elementsByName["fp_tail"].uvTopLeft.x + (double)num * 0.5 * (double)(j - 1));
            triangleMesh.UVvertices[j].y = fatlas._elementsByName["fp_tail"].uvTopLeft.y;
        }
        sLeaser.sprites[2] = triangleMesh;

        sLeaser.sprites[3] = new FSprite("HeadA0", true);
        if (Futile.atlasManager.DoesContainElementWithName("fp_HeadA0"))
        {
            sLeaser.sprites[3] = new FSprite("fp_HeadA0", true);
        }
        else
        {
            Plugin.Log("fp_HeadA0 NOT FOUND");
        }
        sLeaser.sprites[4] = new FSprite("LegsA0", true);
        sLeaser.sprites[4].anchorY = 0.25f;

        sLeaser.sprites[5] = new FSprite("PlayerArm0", true);
        sLeaser.sprites[6] = new FSprite("PlayerArm0", true);
        if (Futile.atlasManager.DoesContainElementWithName("fp_PlayerArm0"))
        {
            sLeaser.sprites[5] = new FSprite("fp_PlayerArm0", true);
            sLeaser.sprites[6] = new FSprite("fp_PlayerArm0", true);
        }
        else
        {
            Plugin.Log("fp_PlayerArm0 NOT FOUND");
        }
        sLeaser.sprites[5].anchorX = 0.9f;
        sLeaser.sprites[5].scaleY = -1f;
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

        self.AddToContainer(sLeaser, rCam, null);



        /*FAtlas fatlas = Futile.atlasManager.LoadAtlas("atlases/fp_tail");
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
        {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(1, 2, 3),
                new TriangleMesh.Triangle(2, 3, 4),
                new TriangleMesh.Triangle(3, 4, 5),
                new TriangleMesh.Triangle(4, 5, 6),
                new TriangleMesh.Triangle(5, 6, 7),
                new TriangleMesh.Triangle(6, 7, 8),
                new TriangleMesh.Triangle(7, 8, 9),
                new TriangleMesh.Triangle(8, 9, 10),
                new TriangleMesh.Triangle(9, 10, 11),
                new TriangleMesh.Triangle(10, 11, 12),
                new TriangleMesh.Triangle(11, 12, 13),
                new TriangleMesh.Triangle(12, 13, 14),
        };
        TriangleMesh triangleMesh = new TriangleMesh("fp_tail", tris, false, false);
        triangleMesh.UVvertices[0] = fatlas._elementsByName["fp_tail"].uvBottomLeft;
        triangleMesh.UVvertices[1] = fatlas._elementsByName["fp_tail"].uvTopLeft;
        triangleMesh.UVvertices[13] = fatlas._elementsByName["fp_tail"].uvTopRight;
        triangleMesh.UVvertices[14] = fatlas._elementsByName["fp_tail"].uvBottomRight;
        float num = (triangleMesh.UVvertices[13].x - triangleMesh.UVvertices[1].x) / 6f;
        for (int i = 2; i < 14; i += 2)
        {
            triangleMesh.UVvertices[i].x = (float)((double)fatlas._elementsByName["fp_tail"].uvBottomLeft.x + (double)num * 0.5 * (double)i);
            triangleMesh.UVvertices[i].y = fatlas._elementsByName["fp_tail"].uvBottomLeft.y;
        }
        for (int j = 3; j < 13; j += 2)
        {
            triangleMesh.UVvertices[j].x = (float)((double)fatlas._elementsByName["fp_tail"].uvTopLeft.x + (double)num * 0.5 * (double)(j - 1));
            triangleMesh.UVvertices[j].y = fatlas._elementsByName["fp_tail"].uvTopLeft.y;
        }
        sLeaser.sprites[2] = triangleMesh;

        self.AddToContainer(sLeaser, rCam, null);*/
    }



    public static void DrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        // 理论上这个代码能简化一下，但我要先让它跑起来，剩下的我不敢动
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i == 2)
            {
                sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName("fp_tail");
            }
            else
            {
                if (Futile.atlasManager.DoesContainElementWithName("fp_" + sLeaser.sprites[i].element.name))
                {
                    // Plugin.Log("element:          ", sLeaser.sprites[i].element.name);
                    sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(sLeaser.sprites[i].element.name.Replace(sLeaser.sprites[i].element.name, "fp_" + sLeaser.sprites[i].element.name));
                }

            }


        }
    }



    public static void ApplyPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            // 2是尾巴，9是眼睛，56是手，除此以外都涂成粉色。。
            // 这个只能硬编码了，不管了（
            if (ColoredBodyParts.Contains(i))
            {
                sLeaser.sprites[i].color = eyesColor_hard;
            }
            else
            {
                sLeaser.sprites[i].color = bodyColor_hard;
            }
        }
    }

}

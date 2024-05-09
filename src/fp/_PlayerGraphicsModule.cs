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
    private const string tailAtlasName = "fp_tail_2";

    public static void InitiateSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.RemoveAllSpritesFromContainer();
        sLeaser.sprites = new FSprite[14];

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




        TriangleMesh.Triangle[] tris2 = new TriangleMesh.Triangle[]
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
        TriangleMesh triangleMesh2 = new TriangleMesh("Futile_White", tris2, false, false);

        /*sLeaser.sprites[13] = new FSprite();
        if (Futile.atlasManager.DoesContainElementWithName(tailAtlasName))
        {
            triangleMesh = new TriangleMesh(tailAtlasName, tris, false, false);
        }
        else
        {
            Plugin.Log("sprite fp_tail_2 NOT FOUND");
        }*/
        FAtlas fatlas = Futile.atlasManager.LoadAtlas("atlases/fp_tail_2");
        triangleMesh2.UVvertices[0] = fatlas._elementsByName[tailAtlasName].uvBottomLeft;
        triangleMesh2.UVvertices[1] = fatlas._elementsByName[tailAtlasName].uvTopLeft;
        triangleMesh2.UVvertices[13] = fatlas._elementsByName[tailAtlasName].uvTopRight;
        triangleMesh2.UVvertices[14] = fatlas._elementsByName[tailAtlasName].uvBottomRight;
        float num = (triangleMesh2.UVvertices[13].x - triangleMesh2.UVvertices[1].x) / 6f;
        for (int i = 2; i < 14; i += 2)
        {
            triangleMesh2.UVvertices[i].x = (float)((double)fatlas._elementsByName[tailAtlasName].uvBottomLeft.x + (double)num * 0.5 * (double)i);
            triangleMesh2.UVvertices[i].y = fatlas._elementsByName[tailAtlasName].uvBottomLeft.y;
        }
        for (int j = 3; j < 13; j += 2)
        {
            triangleMesh2.UVvertices[j].x = (float)((double)fatlas._elementsByName[tailAtlasName].uvTopLeft.x + (double)num * 0.5 * (double)(j - 1));
            triangleMesh2.UVvertices[j].y = fatlas._elementsByName[tailAtlasName].uvTopLeft.y;
        }
        sLeaser.sprites[13] = triangleMesh2;
        sLeaser.sprites[13].isVisible = true;




        self.gown.InitiateSprite(self.gownIndex, sLeaser, rCam);

        self.AddToContainer(sLeaser, rCam, null);


    }










    public static void AddToContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {

        sLeaser.RemoveAllSpritesFromContainer();
        newContatiner ??= rCam.ReturnFContainer("Midground");
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i == 13)
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
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i == 13)
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(tailAtlasName);
                sLeaser.sprites[i].color = new Color32(255, 237, 150, 255);
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

        sLeaser.sprites[13].alpha = Mathf.Lerp(sLeaser.sprites[13].alpha, 1 / (float)self.player.pyroJumpCounter, 0.05f);
        Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
        Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
        float num3 = 1f - 0.2f * self.malnourished;
        float num4 = 6f;
        Vector2 vector4 = (vector2 * 3f + vector) / 4f;
        for (int i = 0; i < 4; i++)
        {
            Vector2 vector5 = Vector2.Lerp(self.tail[i].lastPos, self.tail[i].pos, timeStacker);
            Vector2 normalized = (vector5 - vector4).normalized;
            Vector2 vector6 = Custom.PerpendicularVector(normalized);
            float num5 = Vector2.Distance(vector5, vector4) / 5f;
            if (i == 0)
            {
                num5 = 0f;
            }
            (sLeaser.sprites[13] as TriangleMesh).MoveVertice(i * 4, vector4 - vector6 * num4 * num3 + normalized * num5 - camPos);
            (sLeaser.sprites[13] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 + vector6 * num4 * num3 + normalized * num5 - camPos);
            if (i < 3)
            {
                (sLeaser.sprites[13] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - vector6 * self.tail[i].StretchedRad * num3 - normalized * num5 - camPos);
                (sLeaser.sprites[13] as TriangleMesh).MoveVertice(i * 4 + 3, vector5 + vector6 * self.tail[i].StretchedRad * num3 - normalized * num5 - camPos);
            }
            else
            {
                (sLeaser.sprites[13] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - camPos);
            }
            num4 = self.tail[i].StretchedRad;
            vector4 = vector5;
        }
    }



    public static void ApplyPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        /*for (int i = 0; i < sLeaser.sprites.Length; i++)
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
        }*/
    }

}

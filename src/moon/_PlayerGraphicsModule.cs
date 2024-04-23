using System;
using System.Collections.Generic;
using MoreSlugcats;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Caterators_by_syhnne.moon;


// 十分甚至九分的好写啊（喜）只需：复制srs的代码，ctrl+f搜索tailSpeckles 替换成gills
public class PlayerGraphicsModule
{
    // 纯盲猜的 不知道是不是这个颜色 回头再说罢
    internal static Color redColor = new Color(0.8f, 0.1f, 0.1f);


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
        self.gills.Update();
    }




    public static void PlayerGraphics_ctor(PlayerGraphics self, PhysicalObject ow)
    {
        self.gills = new PlayerGraphics.AxolotlGills(self, 14);
    }


    public static void InitiateSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.RemoveAllSpritesFromContainer();
        sLeaser.sprites = new FSprite[14 + self.gills.numberOfSprites];
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
        TriangleMesh triangleMesh;
        triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[2] = triangleMesh;
        if (Futile.atlasManager.DoesContainElementWithName("moon_HeadA0"))
        {
            sLeaser.sprites[3] = new FSprite("moon_HeadA0", true);
        }
        else
        {
            Plugin.Log("sprite moon_HeadA0 NOT FOUND");
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
        // 这是留给脑门上那个圆点的，回头再写（
        sLeaser.sprites[13] = new FSprite("Futile_White", true);
        sLeaser.sprites[13].isVisible = false;

        self.gills.InitiateSprites(sLeaser, rCam);
        self.gown.InitiateSprite(self.gownIndex, sLeaser, rCam);
        self.AddToContainer(sLeaser, rCam, null);
    }


    public static void AddToContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();
        newContatiner ??= rCam.ReturnFContainer("Midground");
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i >= self.gills.startSprite && i < self.gills.startSprite + self.gills.numberOfSprites)
            {
                rCam.ReturnFContainer(self.player.onBack != null ? "Background" : "Midground").AddChild(sLeaser.sprites[i]);
            }
            else if (i == self.gownIndex)
            {
                newContatiner = rCam.ReturnFContainer("Items");
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
            else
            {
                if (i < 12)
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
    }



    public static void DrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        self.gills.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (Futile.atlasManager.DoesContainElementWithName("moon_" + sLeaser.sprites[i].element.name))
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(sLeaser.sprites[i].element.name.Replace(sLeaser.sprites[i].element.name, "moon_" + sLeaser.sprites[i].element.name));
            }
        }
    }


}
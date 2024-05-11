using System;
using System.Collections.Generic;
using MoreSlugcats;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;



namespace Caterators_by_syhnne.moon;


// 十分甚至九分的好写啊（喜）只需：复制srs的代码，ctrl+f搜索tailSpeckles 替换成gills
// 错误的，dms把gills那里改了，导致我在这卡bug，而且我完全不知道为啥

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
            self.lightSource.setAlpha = 1f;
            self.player.room.AddObject(self.lightSource);
        }
        /*if (Plugin.playerModules.TryGetValue(self.player, out var mod))
        {
            mod.gills?.Update();
        }
        else
        {
            Plugin.Log("PlayerGraphics_Update module not found");
        }*/
        
    }




    public static void PlayerGraphics_ctor(PlayerGraphics self, PhysicalObject ow)
    {
        // self.gills = new PlayerGraphics.AxolotlGills(self, 14);
        if (Plugin.playerModules.TryGetValue(self.player, out var mod))
        {
            mod.gills = new PlayerGraphics.AxolotlGills(self, 14);
        }
    }


    public static void InitiateSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (!Plugin.playerModules.TryGetValue(self.player, out var mod) || self.player.SlugCatClass != Enums.Moonname) 
        {
            Plugin.Log("InitiateSprites module not found");
            return; 
        }
        sLeaser.RemoveAllSpritesFromContainer();
        sLeaser.sprites = new FSprite[14 + mod.gills.numberOfSprites];
        // sLeaser.sprites = new FSprite[14 + self.gills.numberOfSprites];
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


        sLeaser.sprites[13] = new FSprite("Futile_White", true);
        if (Futile.atlasManager.DoesContainElementWithName("moon_dot_HeadA0"))
        {
            sLeaser.sprites[13] = new FSprite("moon_dot_HeadA0", true);
            
        }
        else
        {
            Plugin.Log("sprite moon_dot_HeadA0 NOT FOUND");
            
        }
        // 这个不太好看，还是先别显示了罢
        sLeaser.sprites[13].isVisible = false;

        mod.gills.InitiateSprites(sLeaser, rCam);
        self.gown.InitiateSprite(self.gownIndex, sLeaser, rCam);
        self.AddToContainer(sLeaser, rCam, null);
    }


    public static void AddToContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (!Plugin.playerModules.TryGetValue(self.player, out var mod) || self.player.SlugCatClass != Enums.Moonname)
        {
            Plugin.Log("AddToContainer module not found");
            return;
        }
            
        sLeaser.RemoveAllSpritesFromContainer();
        newContatiner ??= rCam.ReturnFContainer("Midground");
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i >= mod.gills.startSprite && i < mod.gills.startSprite + mod.gills.numberOfSprites)
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
                else
                {
                    newContatiner.AddChild((sLeaser.sprites[i]));
                }
            }

        }
    }



    public static void DrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (!Plugin.playerModules.TryGetValue(self.player, out var mod) || self.player.SlugCatClass != Enums.Moonname)
        {
            Plugin.Log("DrawSprites module not found");
            return;
        }
        mod.gills.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i != 13 && Futile.atlasManager.DoesContainElementWithName("moon_" + sLeaser.sprites[i].element.name))
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(sLeaser.sprites[i].element.name.Replace(sLeaser.sprites[i].element.name, "moon_" + sLeaser.sprites[i].element.name));
            }
        }



        // 不想写ilhook，而且il可能也没法达到效果，不得不复制了
        Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
        Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
        Vector2 vector3 = Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker);
        float num6 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector, 0.5f), vector3);
        int num7 = Mathf.RoundToInt(Mathf.Abs(num6 / 360f * 34f));
        sLeaser.sprites[13].x = vector3.x - camPos.x;
        sLeaser.sprites[13].y = vector3.y - camPos.y;
        sLeaser.sprites[13].rotation = num6;
        sLeaser.sprites[13].scaleX = ((num6 < 0f) ? -1f : 1f);
        if (self.RenderAsPup && Futile.atlasManager.DoesContainElementWithName("moon_dot_HeadA0"))
        {
            sLeaser.sprites[13].element = Futile.atlasManager.GetElementWithName("moon_dot_" + self._cachedHeads[2, num7]);
        }
        else if (Futile.atlasManager.DoesContainElementWithName("moon_dot_HeadA0"))
        {
            sLeaser.sprites[13].element = Futile.atlasManager.GetElementWithName("moon_dot_" + self._cachedHeads[0, num7]);
        }

        if (self.player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
        {
            sLeaser.sprites[13].scaleX *= 0.9f + 0.2f * Mathf.Lerp(self.player.npcStats.Wideness, 0.5f, self.player.playerState.isPup ? 0.5f : 0f);
        }
        sLeaser.sprites[13].color = Color.white;

        sLeaser.sprites[13].isVisible = false;
    }





    public static void ApplyPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (Plugin.playerModules.TryGetValue(self.player, out var module) && module.gills != null)
        {
            

            Color color2 = (ModManager.MSC && self.player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? self.player.ShortCutColor() : PlayerGraphics.SlugcatColor(self.CharacterForColor);
            if (self.malnourished > 0f)
            {
                float num = self.player.Malnourished ? self.malnourished : Mathf.Max(0f, self.malnourished - 0.005f);
                color2 = Color.Lerp(color2, Color.gray, 0.4f * num);
            }
            color2 = self.HypothermiaColorBlend(color2);
            Color effectCol = new(0.87451f, 0.17647f, 0.91765f);
            if (!rCam.room.game.setupValues.arenaDefaultColors && !ModManager.CoopAvailable)
            {
                switch (self.player.playerState.playerNumber)
                {
                    case 0:
                        if (rCam.room.game.IsArenaSession && rCam.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge)
                        {
                            effectCol = new(0.25f, 0.65f, 0.82f);
                        }
                        break;
                    case 1:
                        effectCol = new(0.31f, 0.73f, 0.26f);
                        break;
                    case 2:
                        effectCol = new(0.6f, 0.16f, 0.6f);
                        break;
                    case 3:
                        effectCol = new(0.96f, 0.75f, 0.95f);
                        break;
                }
            }
            module.gills.SetGillColors(color2, effectCol);
            module.gills.ApplyPalette(sLeaser, rCam, palette);

            try
            {
                module.gills?.ApplyPalette(sLeaser, rCam, palette);
            }
            catch (Exception e) 
            { 
                Plugin.LogException(e); 
                return;
            }
        }
    }


}
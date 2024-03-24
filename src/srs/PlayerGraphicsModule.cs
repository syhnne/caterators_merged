using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Caterators_merged.srs;

public class PlayerGraphicsModule
{

    internal static readonly Color spearColor = new Color(1f, 0.2f, 0.1f);
    internal static readonly Color32 bodyColor = new Color32(255, 207, 88, 255);
    internal static readonly List<int> ColoredBodyParts = new List<int>() { 2, 3, };


    




    // 整一个超亮的自发光
    public static void PlayerGraphics_Update(PlayerGraphics self, PlayerModule module)
    {
        /*if (self.lightSource == null)
        {
            self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, Color.Lerp(spearColor, bodyColor, 0.8f), self.player);
            self.lightSource.requireUpKeep = true;
            self.lightSource.setRad = new float?(700f);
            self.lightSource.setAlpha = new float?(3f);
            self.player.room.AddObject(self.lightSource);
        }*/

        
        if (self.player.room != null && module.srsLightSource == null)
        {
            module.srsLightSource = new LightSourceModule(self.player);
        }
        else
        {
            module.srsLightSource.Update();
        }
    }



    public static void PlayerGraphics_ctor(PlayerGraphics self, PhysicalObject ow)
    {
        self.tailSpecks = new PlayerGraphics.TailSpeckles(self, ModManager.MSC ? 13 : 12);

        // 这个数据回头再改（
        if (self.player.playerState.isPup)
        {
            self.tail[0] = new TailSegment(self, 7f, 2f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 5f, 3.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 4f, 3.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 2f, 3.5f, self.tail[2], 0.85f, 1f, 0.5f, true);
        }
        else
        {
            self.tail[0] = new TailSegment(self, 7f, 4f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 6f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 4f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 3.5f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
            self.tail.Append(new TailSegment(self, 2f, 5f, self.tail[3], 0.85f, 1f, 0.5f, true));
        }
        if (self.player.room == null) { return; }
        Plugin.playerModules.TryGetValue(self.player, out var module);
        module.srsLightSource = new LightSourceModule(self.player);
    }


    public static void InitiateSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        /*for (int i = 0; i < sLeaser.sprites.Length + self.tailSpecks.numberOfSprites; i++)
            {
                sLeaser.sprites.Append(new FSprite());
            }
            self.tailSpecks.InitiateSprites(sLeaser, rCam);

            self.AddToContainer(sLeaser, rCam, null);*/

        sLeaser.RemoveAllSpritesFromContainer();
        sLeaser.sprites = new FSprite[13 + self.tailSpecks.numberOfSprites];

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
        if (Futile.atlasManager.DoesContainElementWithName("srs_tail"))
        {
            Plugin.Log("sprite srs_tail NOT FOUND");
            triangleMesh = new TriangleMesh("srs_tail", tris, false, false);
        }
        else
        {
            triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        }
        sLeaser.sprites[2] = triangleMesh;
        if (Futile.atlasManager.DoesContainElementWithName("srs_HeadA0"))
        {
            sLeaser.sprites[3] = new FSprite("srs_HeadA0", true);
        }
        else
        {
            Plugin.Log("sprite srs_HeadA0 NOT FOUND");
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

        self.tailSpecks.InitiateSprites(sLeaser, rCam);
        self.gown.InitiateSprite(self.gownIndex, sLeaser, rCam);

        // 太烧脑了，我终于懂了，0-11是原版sprite，12是msc加的那个gown，13开始才是我需要的


        self.AddToContainer(sLeaser, rCam, null);
    }



    public static void AddToContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        // 就是这一句话导致的，但这是咋回事
        // 不是 为什么我在这随便加一句什么涉及到newContainer的语句 就会出问题啊
        // 噢噢 他是null啊（擦汗
        // Plugin.Log(newContatiner.GetChildCount());
        /*for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            newContatiner.AddChild(sLeaser.sprites[i]);
        }*/
        // self.tailSpecks.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));

        sLeaser.RemoveAllSpritesFromContainer();
        newContatiner ??= rCam.ReturnFContainer("Midground");
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i >= self.tailSpecks.startSprite && i < self.tailSpecks.startSprite + self.tailSpecks.numberOfSprites)
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
        self.tailSpecks.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (Futile.atlasManager.DoesContainElementWithName("srs_" + sLeaser.sprites[i].element.name))
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(sLeaser.sprites[i].element.name.Replace(sLeaser.sprites[i].element.name, "srs_" + sLeaser.sprites[i].element.name));
            }
        }
    }


    // 呃 问题不是我不会写这个函数，而是贴图的混合模式，是正片叠底
    // 如果能让贴图不按正片叠底混合，而是覆盖在原本的颜色之上，那我简直用不着动这个函数
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
    }






























    public static void Apply()
    {
        On.PlayerGraphics.TailSpeckles.DrawSprites += TailSpecks_DrawSprites;
        // IL.PlayerGraphics.Update += IL_PlayerGraphics_Update;
        On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut;
    }


    // 防止玩家离开房间之后没有把自发光带过去
    private static void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        orig(self, pos, newRoom, spitOutAllSticks);
        if (self.SlugCatClass == Enums.SRSname && Plugin.playerModules.TryGetValue(self, out var module) && module.srsLightSource != null)
        {
            module.srsLightSource.AddLightSource();
        }
    }






    // 我谢谢你嗷，以后再也不写ilhook了
    // 非黑暗条件下仍然显示光照
    // 不知道效果如何……按理说应该就是这样吧（
    private static void IL_PlayerGraphics_Update(ILContext il)
    {
        ILCursor c = new(il);
        if (c.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Ldfld),
            i => i.Match(OpCodes.Ldfld),
            i => i.Match(OpCodes.Ldarg_0),
            i => i.Match(OpCodes.Ldfld),
            i => i.Match(OpCodes.Callvirt),
            i => i.Match(OpCodes.Ldfld),
            i => i.Match(OpCodes.Callvirt),
            i => i.Match(OpCodes.Ldc_R4)
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, Player, bool>>((orig, player) =>
            {
                return player.SlugCatClass == Enums.SRSname ? false : orig;
            });
        }
    }



    private static void TailSpecks_DrawSprites(On.PlayerGraphics.TailSpeckles.orig_DrawSprites orig, PlayerGraphics.TailSpeckles self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.pGraphics.player.SlugCatClass == Enums.SRSname)
        {
            for (int i = 0; i < self.rows; i++)
            {
                for (int j = 0; j < self.lines; j++)
                {
                    sLeaser.sprites[self.startSprite + i * self.lines + j].color = spearColor;
                    sLeaser.sprites[self.startSprite + i * self.lines + j].alpha = 0.7f;

                    if (i == self.spearRow && j == self.spearLine)
                    {
                        if (ModManager.CoopAvailable && self.pGraphics.player.IsJollyPlayer)
                        {
                            sLeaser.sprites[self.startSprite + self.lines * self.rows].color = PlayerGraphics.JollyColor(self.pGraphics.player.playerState.playerNumber, 2);
                        }
                        else if (PlayerGraphics.CustomColorsEnabled())
                        {
                            sLeaser.sprites[self.startSprite + self.lines * self.rows].color = PlayerGraphics.CustomColorSafety(2);
                        }
                        else if (self.pGraphics.player.Malnourished)
                        {
                            sLeaser.sprites[self.startSprite + self.lines * self.rows].color = Color.white;
                        }
                        else
                        {
                            sLeaser.sprites[self.startSprite + self.lines * self.rows].color = spearColor;
                        }

                    }
                }
            }


        }
    }

}

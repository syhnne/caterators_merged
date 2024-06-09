using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Caterators_by_syhnne.srs;

public class PlayerGraphicsModule
{
    internal static readonly Color spearColorDark = new Color(0.7f, 0.1f, 0.05f);
    internal static readonly Color spearColor = new Color(1f, 0.2f, 0.1f);
    internal static readonly Color32 bodyColor = new Color32(255, 207, 88, 255);
    internal static readonly List<int> ColoredBodyParts = new List<int>() { 2, 3, };
    private const string tailAtlasName = "srs_tail";

    public static void PlayerGraphics_Update(PlayerGraphics self, _public.PlayerModule module)
    {

    }





    public static void PlayerGraphics_ctor(PlayerGraphics self, PhysicalObject ow)
    {
        self.tailSpecks = new PlayerGraphics.TailSpeckles(self, ModManager.MSC ? 13 : 12);
        self.bodyPearl = new PlayerGraphics.CosmeticPearl(self, (ModManager.MSC? 13 : 12) + self.tailSpecks.numberOfSprites);
        // 这个数据回头再改（
        if (self.player.playerState.isPup)
        {
            self.tail[0] = new TailSegment(self, 7f, 2f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 6f, 3.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 4.5f, 3.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 2.5f, 3.5f, self.tail[2], 0.85f, 1f, 0.5f, true);
        }
        else
        {
            self.tail[0] = new TailSegment(self, 7f, 4f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 6f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 4f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 3.5f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
            self.tail.Append(new TailSegment(self, 2f, 5f, self.tail[3], 0.85f, 1f, 0.5f, true));
        }
        
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
        sLeaser.sprites = new FSprite[13 + self.tailSpecks.numberOfSprites + self.bodyPearl.numberOfSprites];

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
        if (Futile.atlasManager.DoesContainElementWithName(tailAtlasName))
        {
            triangleMesh = new TriangleMesh(tailAtlasName, tris, false, true);
        }
        else
        {
            Plugin.Log("sprite srs_tail NOT FOUND");
        }

        // 这是反编译了伪装者的代码复制过来的，我是一点也没看懂，但不加这几行就会导致尾巴材质不显示。
        // 盲猜一波，这应该是决定了材质按什么样的方式拉伸铺在triangleMesh上面
        FAtlas fatlas = Futile.atlasManager.LoadAtlas("atlases/srs_tail");
        triangleMesh.UVvertices[0] = fatlas._elementsByName[tailAtlasName].uvBottomLeft;
        triangleMesh.UVvertices[1] = fatlas._elementsByName[tailAtlasName].uvTopLeft;
        triangleMesh.UVvertices[13] = fatlas._elementsByName[tailAtlasName].uvTopRight;
        triangleMesh.UVvertices[14] = fatlas._elementsByName[tailAtlasName].uvBottomRight;
        float num = (triangleMesh.UVvertices[13].x - triangleMesh.UVvertices[1].x) / 6f;
        for (int i = 2; i < 14; i += 2)
        {
            triangleMesh.UVvertices[i].x = (float)((double)fatlas._elementsByName[tailAtlasName].uvBottomLeft.x + (double)num * 0.5 * (double)i);
            triangleMesh.UVvertices[i].y = fatlas._elementsByName[tailAtlasName].uvBottomLeft.y;
        }
        for (int j = 3; j < 13; j += 2)
        {
            triangleMesh.UVvertices[j].x = (float)((double)fatlas._elementsByName[tailAtlasName].uvTopLeft.x + (double)num * 0.5 * (double)(j - 1));
            triangleMesh.UVvertices[j].y = fatlas._elementsByName[tailAtlasName].uvTopLeft.y;
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
        self.bodyPearl.InitiateSprites(sLeaser, rCam);

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


        // Plugin.Log("srs_add to container");

        sLeaser.RemoveAllSpritesFromContainer();

        // 这基本上就是把RemoveAllSpritesFromContainer()重写一遍，只不过加了null检查
        /*for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i]?.RemoveFromContainer();
        }

        if (sLeaser.containers != null)
        {
            for (int j = 0; j < sLeaser.containers.Length; j++)
            {
                sLeaser.containers[j]?.RemoveFromContainer();
            }
        }*/


        newContatiner ??= rCam.ReturnFContainer("Midground");
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i >= self.tailSpecks.startSprite && i < self.tailSpecks.startSprite + self.tailSpecks.numberOfSprites + self.bodyPearl.numberOfSprites)
            {
                // 可喜可贺，输出日志显示玩家在背上的时候这个函数每帧都被调用一次，那么问题就很好解决了
                rCam.ReturnFContainer(self.player.onBack != null ? "Background" : "Midground").AddChild(sLeaser.sprites[i]);
            }
            else if (i == self.gownIndex)
            {
                newContatiner = rCam.ReturnFContainer("Items");
                newContatiner.AddChild(sLeaser.sprites[i]);
                // 没看懂游戏自己的代码写2和3上面啥意思，是单纯地为了只添加一次吗
                // self.bodyPearl.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(self.player.onBack != null ? "Background" : "Midground"));
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
        if (self.player.room != null)
        {
            self.tailSpecks.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            self.bodyPearl.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (i == 2)
            {
                sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName("srs_tail");
            }
            else if (Futile.atlasManager.DoesContainElementWithName("srs_" + sLeaser.sprites[i].element.name))
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(sLeaser.sprites[i].element.name.Replace(sLeaser.sprites[i].element.name, "srs_" + sLeaser.sprites[i].element.name));
            }
        }
    }


    // 呃 问题不是我不会写这个函数，而是贴图的混合模式，是正片叠底
    // 如果能让贴图不按正片叠底混合，而是覆盖在原本的颜色之上，那我简直用不着动这个函数
    public static void ApplyPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        self.bodyPearl.ApplyPalette(sLeaser, rCam, palette);
        
    }






























    public static void Apply()
    {
        On.PlayerGraphics.TailSpeckles.DrawSprites += TailSpecks_DrawSprites;
        // IL.PlayerGraphics.Update += IL_PlayerGraphics_Update;
        
    }


    // 防止玩家离开房间之后没有把自发光带过去
    






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
                    sLeaser.sprites[self.startSprite + i * self.lines + j].color = Color.Lerp(self.pGraphics.HypothermiaColorBlend(spearColor), spearColor, 0.8f);
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
                            sLeaser.sprites[self.startSprite + self.lines * self.rows].color = spearColorDark;
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

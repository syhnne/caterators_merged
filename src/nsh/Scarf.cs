using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;
using System.Drawing;

namespace Caterators_by_syhnne.nsh;

// 改的炸矛飘带的代码
// 这应该只包括围巾飘在空中的那部分，剩下的准备直接画贴图上（
// 出问题的话以后再改吧（（

// 血压上来了，为什么这玩意儿跟srs的尾巴一样能穿墙
// 而且他这是双重意义上的穿墙，他在物理上穿墙，在贴图上也穿墙，，
// 他不仅能穿墙，还特么能瞬移，，我真的谢
// 好了，修好了。看来我以前写的代码已经很好了，只是忘了先更新attachPos
// TODO: 但联机队友被围巾遮挡这事我真没办法（汗）因为它本来就在background上面了，我横不能给这个围巾另加一个图层罢
public class Scarf : IDrawable
{

    public PlayerGraphics owner;
    public Vector2[,] rag;
    public int startSprite;
    public int numberOfSprites;
    public Vector2 AttachPos;
    private float conRad = 8f;
    private bool spriteVisible;
    private bool spriteLastVisible;
    private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

    public Scarf(PlayerGraphics owner, int startSprite)
    {
        this.owner = owner;
        rag = new Vector2[5, 6];
        this.startSprite = startSprite;
        numberOfSprites = 2;
        scratchTerrainCollisionData = new();
    }



    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        // Plugin.Log("nshScarf initesprite");
        sLeaser.sprites[startSprite] = TriangleMesh.MakeLongMesh(this.rag.GetLength(0), false, false);
        sLeaser.sprites[startSprite + 1] = new FSprite("Futile_White", true);
        // sLeaser.sprites[startSprite].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
    }




    // 全复制的，一点也不敢动（。
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        // attach pos
        Vector2 vector = Vector2.Lerp(owner.player.firstChunk.lastPos, owner.player.firstChunk.pos, timeStacker);
        PlayerGraphics.PlayerSpineData playerSpineData = owner.SpinePosition(1f, timeStacker);
        vector += playerSpineData.dir * playerSpineData.rad;
        AttachPos = vector;


        spriteLastVisible = spriteVisible;
        spriteVisible = owner.player.room != null;
        if (spriteLastVisible != spriteVisible)
        {
            ResetRag();
        }
        sLeaser.sprites[startSprite].isVisible = spriteVisible && spriteLastVisible;
        sLeaser.sprites[startSprite + 1].isVisible = false;
        if (!spriteVisible && !spriteLastVisible) { return; }


        


        float num = 0f;
        for (int i = 0; i < this.rag.GetLength(0); i++)
        {
            float num2 = (float)i / (float)(this.rag.GetLength(0) - 1);
            Vector2 vector2 = Vector2.Lerp(this.rag[i, 1], this.rag[i, 0], timeStacker);
            float num3 = (2f + 2f * Mathf.Sin(Mathf.Pow(num2, 2f) * 3.1415927f)) * Vector3.Slerp(this.rag[i, 4], this.rag[i, 3], timeStacker).x;
            Vector2 normalized = (vector - vector2).normalized;
            Vector2 vector3 = Custom.PerpendicularVector(normalized);
            float num4 = Vector2.Distance(vector, vector2) / 5f;
            (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num4 - vector3 * (num3 + num) * 0.5f - camPos);
            (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num4 + vector3 * (num3 + num) * 0.5f - camPos);
            (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num4 - vector3 * num3 - camPos);
            (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num4 + vector3 * num3 - camPos);
            vector = vector2;
            num = num3;
        }

    }


    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[startSprite].color = PlayerGraphicsModule.scarfColor;
        sLeaser.sprites[startSprite + 1].color = PlayerGraphicsModule.scarfColor;
    }


    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[startSprite]);
        rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[startSprite + 1]);
    }




    public void ResetRag()
    {
        Vector2 vector = this.AttachPos;
        for (int i = 0; i < this.rag.GetLength(0); i++)
        {
            this.rag[i, 0] = vector;
            this.rag[i, 1] = vector;
            this.rag[i, 2] *= 0f;
        }
    }



    public void Update()
    {
        if (spriteLastVisible != spriteVisible)
        {
            ResetRag();
        }



        for (int i = 0; i < this.rag.GetLength(0); i++)
        {


            float num = (float)i / (float)(this.rag.GetLength(0) - 1);
            this.rag[i, 1] = this.rag[i, 0];
            this.rag[i, 0] += this.rag[i, 2];
            this.rag[i, 4] = this.rag[i, 3];
            this.rag[i, 3] = (this.rag[i, 3] + this.rag[i, 5] * Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
            this.rag[i, 5] = (this.rag[i, 5] + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(this.rag[i, 0], this.rag[i, 1])), 0.3f)).normalized;

            if (owner.player.room.PointSubmerged(this.rag[i, 0]))
            {
                this.rag[i, 2] *= Custom.LerpMap(this.rag[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, num));
                this.rag[i, 2].y += 0.05f;
                this.rag[i, 2] += Custom.RNV() * 0.1f;
            }
            else
            {
                this.rag[i, 2] *= Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, num));
                this.rag[i, 2].y -= owner.player.gravity * Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 6f, 0.6f, 0f);

                if (i % 3 == 2 || i == this.rag.GetLength(0) - 1)
                {
                    SharedPhysics.TerrainCollisionData terrainCollisionData = this.scratchTerrainCollisionData.Set(this.rag[i, 0], this.rag[i, 1], this.rag[i, 2], 1f, new IntVector2(0, 0), false);
                    terrainCollisionData = SharedPhysics.HorizontalCollision(owner.player.room, terrainCollisionData);
                    terrainCollisionData = SharedPhysics.VerticalCollision(owner.player.room, terrainCollisionData);
                    terrainCollisionData = SharedPhysics.SlopesVertically(owner.player.room, terrainCollisionData);
                    this.rag[i, 0] = terrainCollisionData.pos;
                    this.rag[i, 2] = terrainCollisionData.vel;
                    if (terrainCollisionData.contactPoint.x != 0)
                    {
                        this.rag[i, 2].y *= 0.6f;
                    }
                    if (terrainCollisionData.contactPoint.y != 0)
                    {
                        this.rag[i, 2].x *= 0.6f;
                    }
                }
            }
        }
        for (int j = 0; j < this.rag.GetLength(0); j++)
        {
            if (j > 0)
            {
                Vector2 normalized = (this.rag[j, 0] - this.rag[j - 1, 0]).normalized;
                float num2 = Vector2.Distance(this.rag[j, 0], this.rag[j - 1, 0]);
                float num3 = (num2 > this.conRad) ? 0.5f : 0.25f;
                this.rag[j, 0] += normalized * (this.conRad - num2) * num3;
                this.rag[j, 2] += normalized * (this.conRad - num2) * num3;
                this.rag[j - 1, 0] -= normalized * (this.conRad - num2) * num3;
                this.rag[j - 1, 2] -= normalized * (this.conRad - num2) * num3;
                if (j > 1)
                {
                    normalized = (this.rag[j, 0] - this.rag[j - 2, 0]).normalized;
                    this.rag[j, 2] += normalized * 0.2f;
                    this.rag[j - 2, 2] -= normalized * 0.2f;
                }
                if (j < this.rag.GetLength(0) - 1)
                {
                    this.rag[j, 3] = Vector3.Slerp(this.rag[j, 3], (this.rag[j - 1, 3] * 2f + this.rag[j + 1, 3]) / 3f, 0.1f);
                    this.rag[j, 5] = Vector3.Slerp(this.rag[j, 5], (this.rag[j - 1, 5] * 2f + this.rag[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(this.rag[j, 1], this.rag[j, 0]), 1f, 8f, 0.05f, 0.5f));
                }
            }
            else
            {
                this.rag[j, 0] = AttachPos;
                this.rag[j, 2] *= 0f;
            }
        }




    }

}
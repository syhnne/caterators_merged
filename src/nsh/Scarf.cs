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
// TODO: 修好这个瞬移bug
public class Scarf
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



    public void InitiateSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        // Plugin.Log("nshScarf initesprite");
        sLeaser.sprites[startSprite] = TriangleMesh.MakeLongMesh(this.rag.GetLength(0), false, false);
        sLeaser.sprites[startSprite + 1] = new FSprite("Futile_White", true);
        // sLeaser.sprites[startSprite].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
    }




    // 全复制的，一点也不敢动（。
    public void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        spriteLastVisible = spriteVisible;
        spriteVisible = owner.player.room != null;
        sLeaser.sprites[startSprite].isVisible = spriteVisible && spriteLastVisible;
        sLeaser.sprites[startSprite + 1].isVisible = false;
        if (!spriteVisible) { return; }

        // attach pos
        Vector2 vector = Vector2.Lerp(owner.player.firstChunk.lastPos, owner.player.firstChunk.pos, timeStacker);
        PlayerGraphics.PlayerSpineData playerSpineData = owner.SpinePosition(1f, timeStacker);
        vector += playerSpineData.dir * playerSpineData.rad;
        AttachPos = vector;

        /*sLeaser.sprites[startSprite + 1].x = vector.x - camPos.x;
        sLeaser.sprites[startSprite + 1].y = vector.y - camPos.y;
        sLeaser.sprites[startSprite + 1].scaleY = 0.2f;
        sLeaser.sprites[startSprite + 1].scaleX = 0.8f;
        Plugin.Log(owner.player.bodyChunks[0].Rotation, owner.player.bodyChunks[1].Rotation);
        sLeaser.sprites[startSprite + 1].rotation = Custom.VecToDeg(owner.player.bodyChunks[1].Rotation);*/

        float num = 0f;
        for (int i = 0; i < this.rag.GetLength(0); i++)
        {
            float num2 = (float)i / (float)(this.rag.GetLength(0) - 1);
            Vector2 vector2 = Vector2.Lerp(this.rag[i, 1], this.rag[i, 0], timeStacker);
            float num3 = (2f + 2f * Mathf.Sin(Mathf.Pow(num2, 2f) * 3.1415927f)) * Vector3.Slerp(this.rag[i, 4], this.rag[i, 3], timeStacker).x;
            Vector2 normalized = (vector - vector2).normalized;
            Vector2 vector3 = Custom.PerpendicularVector(normalized);
            /*// 无奈出此下策。你就说是不是垂直向量吧。长度怎么样我不太清楚。
            // 没事了，是程序引用集的问题
            Vector2 vector3;
            vector3.x = -normalized.y;
            vector3.y = normalized.x;*/
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



    public void Update()
    {
        if (owner.player.room == null)
        {
            for (int i = 0; i < this.rag.GetLength(0); i++)
            {
                this.rag[i, 0] = AttachPos;
                this.rag[i, 1] = AttachPos;
                this.rag[i, 2] *= 0f;
            }
            return;
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
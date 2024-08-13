using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace Caterators_by_syhnne.fp.Daddy;

// 抄的红树代码，不敢改（
public class TentacleRopeGraphics : RopeGraphic
{
    public CustomDaddyTentacle owner;
    public int numberOfSprites = 1;
    public int startSprite;
    public TentacleRopeGraphics(CustomDaddyTentacle owner) : base(12)
    {
        this.owner = owner;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites[startSprite] = TriangleMesh.MakeLongMesh(this.segments.Length, false, false);
        sLeaser.sprites[startSprite].shader = rCam.room.game.rainWorld.Shaders["JaggedSquare"];
        sLeaser.sprites[startSprite].alpha = 0.8f;
        numberOfSprites = 1;
    }

    public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = this.owner.connectedChunk.pos;
        vector += Custom.DirVec(Vector2.Lerp(this.segments[1].lastPos, this.segments[1].pos, timeStacker), vector) * 1f;
        float a = this.owner.Rad(0f) * 1.2f;
        for (int i = 0; i < this.segments.Length; i++)
        {
            float f = (float)i / (float)(this.segments.Length - 1);
            Vector2 vector2 = Vector2.Lerp(this.segments[i].lastPos, this.segments[i].pos, timeStacker);
            Vector2 a2 = Custom.PerpendicularVector((vector - vector2).normalized);
            float num = this.owner.Rad(f) * 1.2f;
            // bug记录：复制粘贴的时候忘了改sprites索引
            (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4, vector - a2 * Mathf.Lerp(a, num, 0.5f) - camPos);
            (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + a2 * Mathf.Lerp(a, num, 0.5f) - camPos);
            (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - a2 * num - camPos);
            (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + a2 * num - camPos);
            vector = vector2;
            a = num;
        }
    }


    public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
    {
        this.segments[segment].vel *= 0f;
        if (this.owner.room.GetTile(smoothedGoalPos).Solid && !this.owner.room.GetTile(goalPos).Solid)
        {
            FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, this.owner.room.TileRect(this.owner.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
            this.segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
            return;
        }
        this.segments[segment].pos = smoothedGoalPos;
    }


    public override void Reset(Vector2 ps)
    {
        AddToPositionsList(0, owner.firstChunk.pos);
        AddToPositionsList(1, owner.lastChunk.pos);
        AlignAndConnect(2);
        base.Reset(ps);

    }

    public override void Update()
    {
        base.Update();
        int listCount = 0;
        base.AddToPositionsList(listCount++, owner.firstChunk.pos);
        for (int i = 0; i < this.owner.tChunks.Length; i++)
        {
            for (int j = 1; j < this.owner.tChunks[i].rope.TotalPositions; j++)
            {
                base.AddToPositionsList(listCount++, this.owner.tChunks[i].rope.GetPosition(j));
            }
        }
        base.AlignAndConnect(listCount);
    }


}
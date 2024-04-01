using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;



namespace Caterators_by_syhnne.nsh;


// 妈的他的spear是实体的
// 好吧 我洗澡的时候想出来一个小阴招 就是牺牲一下放进背包那一瞬间的动画效果，直接把矛的实体tp到这里来
// 啊这 这就写完了（挠头）我以为这个耗时会很长的 可能以后会加点别的东西的显示吧
public class InventoryItemsOnBack
{
    public Inventory inventory;
    public List<Spear> spears;
    public int spearCapacity;
    public Player player;

    public bool increment;
    public int counter;

    public InventoryItemsOnBack(Inventory owner)
    {
        this.inventory = owner; 
        player = owner.player;
        spears = new List<Spear>();
        spearCapacity = 3;
        increment = false;
        counter = 0;
    }


    private void LogOutput()
    {
        Plugin.Log("InventoryItemsOnBack.spears:", spears.Count());
    }



    public bool AddSpear(Spear spear)
    {
        if (spears.Count >= spearCapacity) { return false; }
        spears.Add(spear);
        spear.ChangeMode(Weapon.Mode.OnBack);
        LogOutput();
        return true;
    }


    public void RemoveSpear(Spear spear)
    {
        if (spears.Count <= 0) { return; }
        spears.Remove(spear);
        spear.ChangeMode(Weapon.Mode.Free);
        LogOutput();
    }



    public void DrawSpears(bool actuallyViewed, bool eu)
    {
        if (spears == null || spears.Count == 0)
        {
            return;
        }
        
        for (int i = 0; i < spears.Count; i++)
        {
            Spear spear = spears[i];
            if (spear.slatedForDeletetion || spear.grabbedBy.Count > 0)
            {
                spears.Remove(spear);
                continue;
            }
            Vector2 mainChunkPos = player.mainBodyChunk.pos;
            Vector2 chunk1Pos = player.bodyChunks[1].pos;
            if (player.graphicsModule != null)
            {
                mainChunkPos = Vector2.Lerp((player.graphicsModule as PlayerGraphics).drawPositions[0, 0], (player.graphicsModule as PlayerGraphics).head.pos, 0.2f);
                chunk1Pos = (player.graphicsModule as PlayerGraphics).drawPositions[1, 0];
            }
            Vector2 chunkDir = Custom.DirVec(chunk1Pos, mainChunkPos);

            float flip = 0f;


            if (player.Consious && player.bodyMode != Player.BodyModeIndex.ZeroG && player.EffectiveRoomGravity > 0f)
            {
                if (player.bodyMode == Player.BodyModeIndex.Default && player.animation == Player.AnimationIndex.None && player.standing && player.bodyChunks[1].pos.y < player.bodyChunks[0].pos.y - 6f)
                {
                    flip = Custom.LerpAndTick(flip, player.input[0].x * 0.3f, 0.05f, 0.02f);
                }
                else if (player.bodyMode == Player.BodyModeIndex.Stand && player.input[0].x != 0)
                {
                    flip = Custom.LerpAndTick(flip, (float)player.input[0].x, 0.02f, 0.1f);
                }
                else
                {
                    flip = Custom.LerpAndTick(flip, (float)player.flipDirection * Mathf.Abs(chunkDir.x), 0.15f, 0.16666667f);
                }
                if (player.input[0].x != 0 && player.standing)
                {
                    float num = 0f;
                    for (int j = 0; j < player.grasps.Length; j++)
                    {
                        if (player.grasps[j] == null)
                        {
                            num = ((j == 0) ? -1f : 1f);
                            break;
                        }
                    }
                    spear.setRotation = new Vector2?(Custom.DegToVec(Custom.AimFromOneVectorToAnother(chunk1Pos, mainChunkPos) + Custom.LerpMap((float)this.counter, 12f, 20f, 0f, 360f * num)));
                }
                else
                {
                    spear.setRotation = new Vector2?((chunkDir - Custom.PerpendicularVector(chunkDir) * 0.9f * (1f - Mathf.Abs(flip))).normalized);
                }
                // spear.setRotation = new Vector2?((chunkDir - Custom.PerpendicularVector(chunkDir) * 0.9f * (1f - Mathf.Abs(flip))).normalized);
                spear.ChangeOverlap(chunkDir.y < -0.1f && player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam);
            }
            else
            {
                flip = Custom.LerpAndTick(flip, 0f, 0.15f, 0.14285715f);
                spear.setRotation = new Vector2?(chunkDir - Custom.PerpendicularVector(chunkDir) * 0.9f);
                spear.ChangeOverlap(false);
            }
            spear.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(chunk1Pos, mainChunkPos, 0.6f) - Custom.PerpendicularVector(chunk1Pos, mainChunkPos) * 7.5f * flip);
            spear.firstChunk.vel = player.mainBodyChunk.vel;
            spear.rotationSpeed = 0f;
        }










    }

}

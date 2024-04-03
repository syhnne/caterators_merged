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

// 妈的这东西能卡一万个bug出来
// 现在大体上修好了，但有这样一个现象：当我不通过玩家的手，直接把矛扔到地上，然后离开房间，再回来的时候，这个矛会显示在玩家前方，这代表着这根矛处于一个bug状态
// 我怀疑这是由于他realize了两次导致的，因为同样的bug还发生在我单纯地对物品使用realize（）的时候，
// 而且那时候效果可以叠加，使得这个矛随着从包里被拿进拿出的次数增多，会逐渐开始抽搐，获得越来越大的动能
// 怎么回事捏，，
// 关键是，任何离开背包的矛，无论是去了手里还是直接掉地上，调用的都是同一段代码，所以应该是我写的有什么问题，但玩家拿一下，或者钻一下管道就恢复了
// 虽然我感觉不会有人蓄意在管道间来回钻并收放手中的矛（正常人应该是背着三根矛跑路，不到关键时候不拿下来吧）但看上去总归是不太好，能修还是修一下
// TODO: 真是最烧脑的一集，以后还得修那个fisobs物品不适配的bug

public class InventoryItemsOnBack
{
    public Inventory inventory;
    public List<SpearOnBack> spears;
    public int spearCapacity;
    public Player player;

    public bool increment;
    public int counter;

    public InventoryItemsOnBack(Inventory owner)
    {
        this.inventory = owner; 
        player = owner.player;
        spears = new();
        spearCapacity = 3;
        for (int i = 0; i < spearCapacity; i++)
        {
            spears.Add(new SpearOnBack(this));
    }

    }


    #nullable enable
    public SpearOnBack? CanAddASpear()
    {
        foreach (SpearOnBack spearOnBack in spears)
        {
            if (!spearOnBack.HasASpear)
            {
                return spearOnBack;
    }
        }
        return null;
    }
    #nullable disable



    public bool AddSpear(Spear spear)
    {
        SpearOnBack sprOnBack = CanAddASpear();
        if ( sprOnBack != null)
        {
            sprOnBack.Add(spear);
        return true;
    }
        return false;
    }


    public bool RemoveSpear(Spear spear)
    {
        if (spears.Count <= 0 || player.room == null) { return false; }
        foreach (var sprOnBack in spears)
        {
            if (sprOnBack.HasASpear && sprOnBack.spear == spear)
            {
                sprOnBack.Remove();
                return true;
            }
        }
        return false;
    }

    public void Update(bool eu)
    {
        if (spears.Count < 0 || player.room == null) return;
        foreach (SpearOnBack spear in spears)
        {
            if (spear.HasASpear)
            {
                spear.Draw(true, eu);
            }
            
        }
    }







}





// 没事了，那玩意儿好像卡bug，现在这个类是一开始就会建3个实例
public class SpearOnBack : ItemsOnBack
{
    public Spear spear;

    public SpearOnBack(InventoryItemsOnBack owner) : base(owner)
    {
        this.owner = owner;
    }

    public bool HasASpear
    {
        get { return (spear != null); }
    }


    public override void Add(PhysicalObject obj)
    {
        if (obj is not Spear) { return; }
        spear = obj as Spear;
        spear.firstChunk.vel = Vector2.zero;
        base.Add(obj);
        owner.player.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, owner.player.mainBodyChunk);
        spear.ChangeMode(Weapon.Mode.OnBack);
        Plugin.Log("spearonback.add ???!!!!!!!!!!!!!!!!!!", spear.mode);
    }


    public override void Remove()
        {
        if (spear == null) return;
        base.Remove();
        spear.ChangeMode(Weapon.Mode.Free);
        owner.player.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, owner.player.mainBodyChunk);
        // spear.PlaceInRoom(owner.player.room);
        Plugin.Log("spearonback.add ???!!!!!!!!!!!!!!!!!!", spear.mode);
        spear = null;
        }
        


    // Adapted from Player.SpearOnBack.GraphicsModuleUpdated()
    public override void Draw(bool actuallyViewed, bool eu)
    {
        if (!HasASpear) return;

        base.Draw(actuallyViewed, eu);

        Player player = owner.player;

        if (spear.mode != Weapon.Mode.OnBack)
        {
            spear.mode = Weapon.Mode.OnBack;
        }
            if (spear.slatedForDeletetion || spear.grabbedBy.Count > 0)
            {
            this.abstractStick?.Deactivate();
            this.spear = null;
            return;
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
            if (player.input[0].x != 0 && player.standing && player.animation != Player.AnimationIndex.ClimbOnBeam)
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
                spear.setRotation = new Vector2?(Custom.DegToVec(Custom.AimFromOneVectorToAnother(chunk1Pos, mainChunkPos) + Custom.LerpMap(1, 12f, 20f, 0f, 360f * num)));
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






public abstract class ItemsOnBack
{
    public InventoryItemsOnBack owner;

    // 懂了 这个stick我一开始以为是“杆子”的意思 其实它的含义类似于“胶水” 是把两个物体粘在一起的一种东西
    // 这也是非得单独写一个类并列表，而不是直接把physicalObject塞进一个列表的原因
    // 如果没有这个类，那么玩家离开房间的时候背上的东西还会留在原来的房间里
    public Player.AbstractOnBackStick abstractStick;




    public ItemsOnBack(InventoryItemsOnBack owner)
    {
        this.owner = owner;
        
    }




    public virtual void Add(PhysicalObject obj)
    {
        this.abstractStick?.Deactivate();
        abstractStick = new Player.AbstractOnBackStick(owner.player.abstractPhysicalObject, obj.abstractPhysicalObject);
    }


    public virtual void Remove()
    {
        if (abstractStick != null)
        {
            abstractStick.Deactivate();
            abstractStick = null;
        }
    }


    public virtual void Draw(bool actuallyViewed, bool eu)
    {

    }


}

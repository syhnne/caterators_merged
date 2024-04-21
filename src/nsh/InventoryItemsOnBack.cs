using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using MoreSlugcats;



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
// 没事了，他适配fisobs，而且适配的非常好（喜

// 好，我要换用方法（绝望）
// 算了算了 本来是想着把灯笼藏在玩家背后 给队友提供便利来着 只能放弃了（恼

// 卧槽 物品不能带进新区域啊

/// <summary>
/// 提供一些好看的视觉效果（但写得我cpu要烧了
/// </summary>
public class InventoryItemsOnBack
{
    public Inventory inventory;
    public List<SpearOnBack> spears;
    // public List<LanternOnBack> lanterns;
    public int spearCapacity;
    // public int lanternCapacity;
    public Player player;

    public int lanternCount = 0;
    public InventoryLantern lightSource;


    public InventoryItemsOnBack(Inventory owner)
    {
        this.inventory = owner; 
        player = owner.player;
        spears = new();
        spearCapacity = 3;
        // lanterns = new();
        // lanternCapacity = 5;
        for (int i = 0; i < spearCapacity; i++)
        {
            spears.Add(new SpearOnBack(this));
        }
        /*for (int i = 0;i < lanternCapacity; i++)
        {
            lanterns.Add(new LanternOnBack(this));
        }*/

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

    // 好好好 复制粘贴是吧
    // 虽然这个能跑 但是纯复制粘贴改名属实太狼狈了
    /*public LanternOnBack? CanAddALantern()
    {
        foreach (LanternOnBack lanternOnBack in lanterns)
        {
            if (!lanternOnBack.HasALantern)
            {
                return lanternOnBack;
            }
        }
        return null;
    }*/
#nullable disable



    public bool AddItem(PhysicalObject obj)
    {
        if (obj is Spear)
        {
            SpearOnBack sprOnBack = CanAddASpear();
            if (sprOnBack != null)
            {
                sprOnBack.Add(obj as Spear);
                return true;
            }
        }
        /*else if (obj is Lantern)
        {
            LanternOnBack lanternOnBack = CanAddALantern();
            if (lanternOnBack != null)
            {
                lanternOnBack.Add(obj as Lantern);
                return true;
            }
        }*/

        return false;
    }


    public bool RemoveItem(PhysicalObject obj)
    {



        if (player.room == null) { return false; }
        if (obj is Spear)
        {
            foreach (var sprOnBack in spears)
            {
                if (sprOnBack.HasASpear && sprOnBack.spear == obj)
                {
                    sprOnBack.Remove();
                    return true;
                }
            }
        }
        /*else if (obj is Lantern)
        {
            foreach (var lanternOnBack in lanterns)
            {
                if (lanternOnBack.HasALantern && lanternOnBack.lantern == obj)
                {
                    lanternOnBack.Remove();
                    return true;
                }
            }
        }*/


        
        return false;
    }

    public void Update(bool eu)
    {
        if (player.room == null) return;
        foreach (SpearOnBack spear in spears)
        {
            if (spear.HasASpear)
            {
                spear.Draw(true, eu);
            }
        }

        

        if (this.lightSource == null && lanternCount > 0)
        {
            this.lightSource = new InventoryLantern(this);
            this.lightSource.affectedByPaletteDarkness = 0.5f;
            player.room.AddObject(this.lightSource);
        }
        else if (lightSource != null)
        {
            if (lanternCount == 0)
            {
                lightSource.setRad = 0f;
                lightSource.setAlpha = 0f;
                lightSource.slatedForDeletetion = true;
                lightSource = null;
                
            }
            else
            {
                this.lightSource.setPos = new Vector2?(player.firstChunk.pos);
                this.lightSource.setRad = 250f * Mathf.Sqrt(lanternCount);
                this.lightSource.setAlpha = Mathf.Sqrt(lanternCount);
                if (this.lightSource.slatedForDeletetion || this.lightSource.room != player.room)
                {
                    this.lightSource = null;
                }
            }
            
        }


        /*foreach (var lantern in lanterns)
        {
            if (lantern.HasALantern)
            {
                lantern.Draw(true, eu);
            }
        }*/
    }







}







// 这写法多方便啊 srs那个应该给他改一改
public class InventoryLantern : LightSource, IProvideWarmth
{

    public InventoryItemsOnBack owner;
    public InventoryLantern(InventoryItemsOnBack owner) : base(owner.player.mainBodyChunk.pos, false, new Color(1f, 0.2f, 0f), owner.player)
    {
        this.owner = owner;
    }

    public Vector2 Position()
    {
        return owner.player.mainBodyChunk.pos;
    }

    public Room loadedRoom
    {
        get { return owner.player.room; }
    }


    public float warmth
    {
        get { return owner.lanternCount * RainWorldGame.DefaultHeatSourceWarmth; }
    }

    public float range
    {
        get {  return 350f; }
    }
}











// 没事了，那玩意儿好像卡bug，现在这个类是一开始就会建3个实例
public class SpearOnBack : ItemsOnBack
{
    public Spear spear;

    public SpearOnBack(InventoryItemsOnBack owner) : base(owner)
    {

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
    }


    public override void Remove()
    {
        if (spear == null) return;
        base.Remove();
        spear.ChangeMode(Weapon.Mode.Free);
        owner.player.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, owner.player.mainBodyChunk);
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






public class LanternOnBack : ItemsOnBack
{
    public Lantern lantern;

    public LanternOnBack(InventoryItemsOnBack owner) : base(owner)
    {

    }


    public bool HasALantern
    {
        get { return lantern != null; }
    }


    public override void Add(PhysicalObject obj)
    {
        if (obj is not Lantern) return;
        base.Add(obj);
        lantern = obj as Lantern;
        ChangeOverlap(false);
        
    }



    public override void Remove()
    {
        if (lantern == null) return;
        base.Remove();
        ChangeOverlap(true);
        lantern = null;
    }




    public override void Draw(bool actuallyViewed, bool eu)
    {
        if (!HasALantern) return;

        base.Draw(actuallyViewed, eu);
        lantern.grabbedBy.Clear();
        Player player = owner.player;

        if (lantern.slatedForDeletetion)
        {
            this.abstractStick?.Deactivate();
            this.lantern = null;
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


        // lantern.setRotation = new Vector2?((chunkDir - Custom.PerpendicularVector(chunkDir) * 0.9f * (1f - Mathf.Abs(flip))).normalized);
        // lantern.ChangeOverlap(chunkDir.y < -0.1f && player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam);
        lantern.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(chunk1Pos, mainChunkPos, 0.6f));
        lantern.firstChunk.vel = player.mainBodyChunk.vel;

    }



    public void ChangeOverlap(bool newOverlap)
    {
        // 妈的 这不都调用上了吗 你倒是别给我碰撞啊 为什么这个方法对蛞蝓猫好用对灯笼就不好用
        // 只能换方法了
        lantern.ChangeCollisionLayer(newOverlap ? 2 : 1);
        lantern.CollideWithObjects = newOverlap;
        lantern.CollideWithSlopes = newOverlap;
        lantern.CollideWithTerrain = newOverlap;
        lantern.canBeHitByWeapons = newOverlap;
        if (!newOverlap)
        {
            lantern.RemoveGraphicsModule();
        }
        else
        {
            lantern.InitiateGraphicsModule();
        }
        Plugin.Log("is this method being called?? ChangeOverlap()", newOverlap, lantern.CollideWithObjects, lantern.CollideWithSlopes, lantern.CollideWithTerrain, lantern.canBeHitByWeapons);
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

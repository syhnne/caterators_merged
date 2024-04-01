using System;
using System.Collections.Generic;
using HUD;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using static System.Net.Mime.MediaTypeNames;
using static MonoMod.InlineRT.MonoModRule;

namespace Caterators_by_syhnne.nsh;



public class Inventory
{
    /// <summary>
    /// 让nsh可以把任何东西装进背包，包括大型生物的尸体，包括联机队友。如果装了moregrabs我不知道会发生什么。
    /// </summary>
    /// 等我抽空试试能不能把五卵石打包带走
    private const bool unlimited = false;
    public List<AbstractPhysicalObject> Items {  get; set; }
    public Player player;
    public InventoryHUD hud;
    public InventoryItemsOnBack itemsOnBack;

    // 保证按一次下键只能添加一个物品，不会一股脑把东西全倒出来
    public int lastInputY;
    public int inputY;

    public static readonly int capacity = 50;
    public int currCapacity;

    public Inventory(Player owner)
    {
        this.player = owner;
        Items = new();
        lastInputY = 0;
        inputY = 0;
        itemsOnBack = new(this);
        currCapacity = 0;
        // isActive = false;
    }


    public bool IsActive
    {
        get
        {
            return (player.Consious && !player.dead && player.stun == 0
            && Input.GetKey(Plugin.instance.option.InventoryKey.Value)
            && player.animation != Player.AnimationIndex.ZeroGPoleGrab);
        }
    }


    public void UpdateLog()
    {
        string str = "--inventory:";
        foreach (AbstractPhysicalObject item in Items)
        {
            str += item.GetType().Name + " ";
        }
        Plugin.Log(str);
        Plugin.Log("--capacity:", currCapacity);
    }


    public void Update(bool eu)
    {

        itemsOnBack?.Update(eu);
       
        if (IsActive && inputY == 1 && lastInputY != 1)
        {
            for (int i = 0; i < 2; i++)
            {
                if (player.grasps[i] != null && CanBePutIntoBag(player.grasps[i].grabbed.abstractPhysicalObject))
                {
                    AddObject(player.grasps[i].grabbed);
                    break;
                }
            }
        }
        if (IsActive && inputY == -1 && lastInputY != -1)
        {
            RemoveObject(eu);
        }

        lastInputY = inputY;
    }





    public void AddObject(PhysicalObject obj)
    {
        if (obj == null || obj.abstractPhysicalObject == null)
        { 
            throw new ArgumentNullException("null obj:" + nameof(obj));
        }
        if (player.room == null || obj.room == null || player.room != obj.room) return;

        obj.AllGraspsLetGoOfThisObject(true);
        Items.Add(obj.abstractPhysicalObject);
        // currCapacity += ItemVolume(obj);

        
        if (obj is Spear && itemsOnBack.AddSpear(obj as Spear))
        {
        }
        else 
        {
            player.room.RemoveObject(obj);
            player.room.abstractRoom.RemoveEntity(obj.abstractPhysicalObject);
            obj.abstractPhysicalObject.realizedObject = null;
        }


        ReloadCapacity();
        hud?.ResetObjects();

        Plugin.Log("inventory add obj:", obj.GetType().Name);
        UpdateLog();
    }




    public void RemoveObject(bool eu)
    {
        if (player.room == null || Items.Count <= 0) {  return; }

        AbstractPhysicalObject obj = Items[Items.Count - 1];
        
        Items.Remove(obj);
        
        
        if (obj.realizedObject != null && obj.realizedObject is Spear)
        {
            if (player.graphicsModule != null && player.FreeHand() > -1)
            {
                (obj.realizedObject as Spear).firstChunk.MoveFromOutsideMyUpdate(eu, (player.graphicsModule as PlayerGraphics).hands[player.FreeHand()].pos);
            }
            // 别删这输出日志 他是代码而不是单纯的输出日志
            Plugin.Log("retrieve spear from back:", itemsOnBack.RemoveSpear(obj.realizedObject as Spear));
        }
        else
        {
            player.room.abstractRoom.AddEntity(obj);
            obj.pos = player.abstractCreature.pos;
            obj.RealizeInRoom();
        }
        
        PhysicalObject realObj = obj.realizedObject;
        // currCapacity -= ItemVolumeFromAbstr(obj);

        if (ModManager.MMF && MMF.cfgKeyItemTracking.Value && AbstractPhysicalObject.UsesAPersistantTracker(obj) && player.room.game.IsStorySession)
        {
            (player.room.game.session as StoryGameSession).AddNewPersistentTracker(obj);
            if (player.room.abstractRoom.NOTRACKERS)
            {
                obj.tracker.lastSeenRegion = player.lastGoodTrackerSpawnRegion;
                obj.tracker.lastSeenRoom = player.lastGoodTrackerSpawnRoom;
                obj.tracker.ChangeDesiredSpawnLocation(player.lastGoodTrackerSpawnCoord);
            }
        }

        hud?.ResetObjects();
        ReloadCapacity();

        Plugin.Log("inventory remove obj:", realObj.GetType().Name);
        UpdateLog();




        if (player.FreeHand() > -1)
        {
            if (ModManager.MMF && (player.grasps[0] != null ^ player.grasps[1] != null) && player.Grabability(obj.realizedObject) == Player.ObjectGrabability.BigOneHand)
            {
                int num3 = 0;
                if (player.FreeHand() == 0)
                {
                    num3 = 1;
                }
                if (player.Grabability(player.grasps[num3].grabbed) != Player.ObjectGrabability.BigOneHand)
                {
                    player.SlugcatGrab(obj.realizedObject, player.FreeHand());
                }
            }
            else
            {
                player.SlugcatGrab(obj.realizedObject, player.FreeHand());
            }
        }

    }


    public void RemoveAllObjects()
    {
        if (player.room == null || Items.Count <= 0) { return; }
        foreach (AbstractPhysicalObject obj in Items)
        {
            
            if (obj.realizedObject != null && obj.realizedObject is Spear)
            {
                // 别删这输出日志 他是代码而不是单纯的输出日志
                Plugin.Log("retrieve spear from back:", itemsOnBack.RemoveSpear(obj.realizedObject as Spear));
            }
            else
            {
                player.room.abstractRoom.AddEntity(obj);
                obj.pos = player.abstractCreature.pos;
                obj.RealizeInRoom();
            }
            
        }
        Items.Clear();
        currCapacity = 0;
        hud?.ResetObjects();
        


        Plugin.Log("removing all inventory objects");
    }


    // 寄，我突然想起来这个事和玩家绑定的，而不是和游戏绑定的，多人游戏下不太好保存
    // 有一个小阴招，就是在游戏存档的那一瞬间之前，把背包里所有东西吐出来，剩下的交给游戏自身机制
    // 算了，先这样凑活一下
    public void Save(RainWorldGame game)
    {

 
    
    }



    public bool CanBePutIntoBag(AbstractPhysicalObject obj)
    {
        if (obj is AbstractSpear)
        {
            return (itemsOnBack.CanAddASpear() != null);
        }
        return currCapacity + ItemVolumeFromAbstr(obj) <= capacity;
    }



    // TODO: 这玩意儿也有bug 点的快了他容易算不出来，最好是用abstractobj计算，每次加东西的时候reset一遍
    // AbstractPhysicalObject.AbstractObjectType

    #pragma warning disable CS0162 // 检测到无法访问的代码
    /*public int ItemVolume(PhysicalObject obj)
    {
        
        if (unlimited) return 0;
        if (obj is Spear) return 5;
        if (player.CanBeSwallowed((PhysicalObject)obj) || obj is IPlayerEdible) return 10;
        if (player.Grabability(obj) <= Player.ObjectGrabability.BigOneHand) return 15;
        if (!player.HeavyCarry(obj)) return 25;
        return 100;
    }*/



    public int ItemVolumeFromAbstr(AbstractPhysicalObject obj)
    {

        if (unlimited) return 0;
        if (obj is AbstractSpear) return 5;
        if (obj.type == AbstractPhysicalObject.AbstractObjectType.Oracle
            || obj.type == AbstractPhysicalObject.AbstractObjectType.Creature
            || obj.type == AbstractPhysicalObject.AbstractObjectType.SeedCob
            || obj.type == AbstractPhysicalObject.AbstractObjectType.CollisionField
            || obj.type == MoreSlugcatsEnums.AbstractObjectType.HRGuard) return 100;
        if (obj.type == MoreSlugcatsEnums.AbstractObjectType.EnergyCell
            || obj.type == MoreSlugcatsEnums.AbstractObjectType.JokeRifle) return 30;
        return 10;
    }
#pragma warning restore CS0162 // 检测到无法访问的代码



    public void ReloadCapacity()
    {
        int result = 0;
        foreach (AbstractPhysicalObject obj in Items) 
        {
            result += ItemVolumeFromAbstr(obj);
        }
        currCapacity = result;
    }
}
















public class InventoryHUD : HudPart
{
    public Inventory owner;
    public Vector2 pos;
    public Vector2 lastPos;
    public float fade;
    public float lastFade;
    public List<InventoryIcon> icons;
    public int showHUDcounter;
    public List<float> rowsWidth;

    public HUDCircle test;

    public InventoryHUD(HUD.HUD hud, FContainer fContainer, Inventory owner) : base(hud)
    {
        this.owner = owner;
        pos = owner.player.mainBodyChunk.pos;
        lastPos = pos;
        fade = 0f;
        lastFade = 0f;
        showHUDcounter = 0;
        icons = new List<InventoryIcon>();
        rowsWidth = new List<float>();


        test = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0)
        {
            rad = 30f,
            thickness = 1f,
            visible = false
        };
        test.sprite.isVisible = false;
        test.pos = owner.player.mainBodyChunk.pos;
    }


    public bool Show
    {
        get
        {
            return (owner != null && owner.IsActive && owner.player.room != null);
        }
    }



    public override void Update()
    {
        base.Update();
        lastPos = pos;
        lastFade = fade;
        Vector2 camPos = new(0, 50);
        if (owner.player.room != null)
        {
            camPos += owner.player.room.game.cameras[0].pos;
        }
        pos = owner.player.mainBodyChunk.pos - camPos;

        if (Show)
        {
            fade = Mathf.Min(1f, fade + 0.1f);
        }
        else
        {
            fade = Mathf.Max(0f, fade - 0.033333335f);
        }


        
        test.Update();
        test.pos = pos;
        test.fade = fade;
        if (Show) test.thickness = Math.Min(3f, test.thickness + 0.333333335f);
        else test.thickness = Math.Max(1f, test.thickness - 0.333333335f);

        
        



    }

    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame) return;
        test.Draw(timeStacker);


        base.Draw(timeStacker);
        if (icons.Count > 0)
        {
            foreach (InventoryIcon icon in icons)
            {
                icon.Draw(fade, DrawPos(timeStacker), timeStacker);
            }
        }
        

        
    }


    // Adapted from HUD.Map.ShelterMarker.RevealSymbols()
    public void ResetObjects()
    {
        ClearSprites();
        foreach (AbstractPhysicalObject obj in owner.Items)
        {
            IconSymbol.IconSymbolData? data = DataFromAbstractPhysical(obj);
            InventoryIcon icon;
            if (data != null)
            {
                icon = new(this, this.hud.fContainers[0], (IconSymbol.IconSymbolData)data)
                {
                    newlyAddCounter = 60,
                    offset = Vector2.zero,
                };
                
            }
            else
            {
                data = new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
                icon = new(this, hud.fContainers[0], (IconSymbol.IconSymbolData)data);
            }
            icons.Add(icon);
        }






        if (icons.Count <= 0) { return; }


        float lineWidth = 0f;
        int lines = 0;
        for (int k = 0; k < icons.Count; k++)
        {
            if (this.rowsWidth.Count <= lines)
            {
                this.rowsWidth.Add(0f);
            }

            icons[k].offset = new Vector2(lineWidth + (icons[k].symbol.graphWidth - this.rowsWidth[lines]) / 2f, (lines + 1) * -30f);
            lineWidth += icons[k].symbol.graphWidth;
            if (lineWidth > 200f || k == icons.Count - 1)
            {
                this.rowsWidth[lines] = lineWidth;
            }
            if (lineWidth > 200f)
            {
                lineWidth = 0f;
                lines++;
            }
            else
            {
                lineWidth += 5f;
            }
        }
    }


    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(lastPos, pos, timeStacker);
    }







    private IconSymbol.IconSymbolData? DataFromAbstractPhysical(AbstractPhysicalObject obj)
    {
        if (obj.destroyOnAbstraction)
        {
            return null;
        }
        if (obj is AbstractCreature)
        {
            if ((obj as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Slugcat || (obj as AbstractCreature).creatureTemplate.quantified)
            {
                return null;
            }
            return CreatureSymbol.SymbolDataFromCreature(obj as AbstractCreature);
        }
        else
        {
            return ItemSymbol.SymbolDataFromItem(obj);
        }
    }




    public override void ClearSprites()
    {
        foreach (InventoryIcon icon in icons)
        {
            icon.RemoveSprites();
        }
        icons.Clear();
        base.ClearSprites();
    }






















    public class InventoryIcon
    {
        private InventoryHUD owner;
        public IconSymbol symbol;

        public IconSymbol.IconSymbolData data;

        public Vector2 offset;
        public int newlyAddCounter;


        public InventoryIcon(InventoryHUD owner, FContainer container, IconSymbol.IconSymbolData data)
        {
            this.owner = owner;
            this.data = data;
            symbol = IconSymbol.CreateIconSymbol(data, container);
            symbol.Show(false);
            

            
        }

        public void Update()
        {
            symbol.Update();
            if (newlyAddCounter > 0)
            {
                newlyAddCounter--;
            }
        }


        public void Draw(float fade, Vector2 drawPos, float timeStacker)
        {
            symbol.Draw(timeStacker, drawPos + offset);

            symbol.symbolSprite.alpha = fade * 0.8f;

        }

        public void RemoveSprites()
        {
            symbol.RemoveSprites();
        }
    }
}
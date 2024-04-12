using System;
using System.Collections.Generic;
using HUD;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using static System.Net.Mime.MediaTypeNames;
using static MonoMod.InlineRT.MonoModRule;
using static Caterators_by_syhnne.CustomSaveData;
using System.Security.Permissions;
using Fisobs.Core;
using System.Text.RegularExpressions;
using System.Linq;

namespace Caterators_by_syhnne.nsh;





#pragma warning disable CS0162 // 检测到无法访问的代码


// TODO: 加一个背包装满的动画效果
// TODO: 修复背上的矛能被香菇吃掉的bug（？
// 我比较好奇猎手有没有遇到过这个问题 呃 应该没有过罢 一般蛞蝓猫应该没有香菇都吃到自己背上来了还能生还的经历 要不就不修了
public class Inventory
{
    /// <summary>
    /// 让nsh可以把任何东西装进背包，包括大型生物的尸体，包括联机队友。如果装了moregrabs我不知道会发生什么。
    /// 还是别给玩家启用这个了，我怕savetostring的时候崩游戏
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

    public bool lastIsActive;

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
            && Input.GetKey(Plugin.instance.configOptions.InventoryKey.Value)
            && player.animation != Player.AnimationIndex.ZeroGPoleGrab);
        }
    }


    public void UpdateLog()
    {
        Plugin.Log("--inventory capacity:", currCapacity, "count:", Items.Count);
        // Plugin.Log("--savetostring:", SaveToString());
    }


    public string SaveToString()
    {
        // 不按他的格式存了，反正这是我自己读取又不是他帮我读取（？
        string str = "";
        if (unlimited) { return str; }
        foreach (AbstractPhysicalObject obj in Items)
        {
            str += obj.ToString();
            // 这是分隔符（。
            str += "<!>";
        }
        str += "";
        return str;
    }


    
    public List<AbstractPhysicalObject> LoadFromString(World world, string str, bool keepSpears)
    {

        List<AbstractPhysicalObject> result = new();
        string[] data = Regex.Split(str, "<!>");

        foreach (string d in data)
        {
            if (d.Count() > 0)
            {
                result.Add(SaveState.AbstractPhysicalObjectFromString(world, d));
            }
        }
        if (keepSpears)
        {
            if (result.Count() != Items.Count) { throw new Exception("items.count?"); }
            List<AbstractPhysicalObject> result2 = new();

            for (int i = 0; i < Items.Count; i++) 
            { 
                if (Items[i] is AbstractSpear)
                {
                    result2.Add(Items[i]);
                }
                else
                {
                    result2.Add(result[i]);
                }
            }
            return result2;
        }
        return result;
    }




    public void Update(bool eu)
    {

        itemsOnBack?.Update(eu);

        // 和plugin实例存储对照检查
        // 这真是个烂方法，除了我没人想得出来那种
        // 他烂就烂在我该什么时候读这个数据。。我想不好。。
        if (!unlimited && player.room != null && IsActive != lastIsActive && Plugin.instance.nshInventoryList[player.abstractCreature.ID.number] != null)
        {
            Items = LoadFromString(player.room.world, Plugin.instance.nshInventoryList[player.abstractCreature.ID.number], true);
            Plugin.Log("inventory empty check for player", player.abstractCreature.ID.number, Items.Count);
            UpdateLog();
            ReloadCapacity();
            hud?.ResetObjects();
            Plugin.instance.nshInventoryList[player.abstractCreature.ID.number] = null;
            Plugin.Log("null?", Plugin.instance.nshInventoryList[player.abstractCreature.ID.number] == null);
        }

        if (IsActive && inputY == 1 && lastInputY != 1)
        {
            for (int i = 0; i < 2; i++)
            {
                if (player.grasps[i] != null)
                {
                    if (CanBePutIntoBag(player.grasps[i].grabbed.abstractPhysicalObject))
                    {
                        AddObjectToTop(player.grasps[i].grabbed);
                        break;
                    }
                    else
                    {
                        hud.refuseCounter = 50;
                    }
                    

                    
                }
            }
        }
        if (IsActive && inputY == -1 && lastInputY != -1)
        {
            RemoveAndRealizeObjectFromTop(eu);
        }
        

        lastInputY = inputY;
        lastIsActive = IsActive;
    }





    public void AddObjectToTop(PhysicalObject obj)
    {
        ReloadCapacity();
        if (obj == null || obj.abstractPhysicalObject == null)
        { 
            throw new ArgumentNullException("null obj:" + nameof(obj));
        }
        if (player.room == null || obj.room == null || player.room != obj.room) return;

        obj.AllGraspsLetGoOfThisObject(true);
        Items.Add(obj.abstractPhysicalObject);
        // currCapacity += ItemVolume(obj);

        
        if ((obj is Spear) && itemsOnBack.AddItem(obj))
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




    public void RemoveAndRealizeObjectFromTop(bool eu)
    {
        if (player.room == null || Items.Count <= 0) {  return; }
        ReloadCapacity();
        AbstractPhysicalObject obj = Items[Items.Count - 1];

        if (!RemoveSpecificObj(obj)) { Plugin.Log("inventory error: null object"); return; }
        
        
        if (obj.realizedObject != null && (obj.realizedObject is Spear))
        {
            if (player.graphicsModule != null && player.FreeHand() > -1)
            {
                (obj.realizedObject).firstChunk.MoveFromOutsideMyUpdate(eu, (player.graphicsModule as PlayerGraphics).hands[player.FreeHand()].pos);
            }
            // 别删这输出日志 他是代码而不是单纯的输出日志
            Plugin.Log("retrieve item from back:", itemsOnBack.RemoveItem(obj.realizedObject));
        }
        else
        {
            
            obj.pos = player.abstractCreature.pos;
            obj.Spawn();
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




    public bool RemoveSpecificObj(AbstractPhysicalObject obj)
    {
        if (Items.Contains(obj))
        {
            Items.Remove(obj);
            hud?.ResetObjects();
            ReloadCapacity();
            Plugin.Log("inventory remove obj:", obj.type.ToString());
            UpdateLog();
            return true;
        }
        return false;
    }





    // 爆装备（不是
    public void RemoveAndRealizeAllObjects()
    {
        if (player.room == null || Items.Count <= 0) { return; }
        foreach (AbstractPhysicalObject obj in Items)
        {
            
            if (obj.realizedObject != null && (obj.realizedObject is Spear))
            {
                // 别删这输出日志 他是代码而不是单纯的输出日志
                Plugin.Log("retrieve item from back:", itemsOnBack.RemoveItem(obj.realizedObject));
            }
            else
            {
                obj.pos = player.abstractCreature.pos;
                obj.Spawn();
            }
            
        }
        Items.Clear();
        currCapacity = 0;
        hud?.ResetObjects();
        


        Plugin.Log("removing all inventory objects");
    }


    // 全部移除，但不realize
    public string CycleEndSave()
    {
        string result = SaveToString();
        foreach (AbstractPhysicalObject obj in Items)
        {
            if (obj.realizedObject != null && (obj.realizedObject is Spear))
            {
                // 别删这输出日志 他是代码而不是单纯的输出日志
                Plugin.Log("retrieve item from back:", itemsOnBack.RemoveItem(obj.realizedObject));
                obj.realizedObject.AllGraspsLetGoOfThisObject(true);
                player.room.RemoveObject(obj.realizedObject);
                obj.realizedObject = null;
                player.room.abstractRoom.RemoveEntity(obj);
                
            }
            else
            {
                obj.pos = player.abstractCreature.pos;
            }
        }
        ReloadCapacity();
        hud?.ResetObjects();
        return result;
    }





    public bool CanBePutIntoBag(AbstractPhysicalObject obj)
    {
        bool spear = true;
        if (obj is AbstractSpear)
        {
            spear = (itemsOnBack.CanAddASpear() != null);
        }
        return spear && (currCapacity + ItemVolumeFromAbstr(obj) <= capacity);
    }



    // TODO: 这玩意儿也有bug 点的快了他容易算不出来，最好是用abstractobj计算，每次加东西的时候reset一遍
    // AbstractPhysicalObject.AbstractObjectType


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



    /*public int ItemVolume(PhysicalObject obj)
    {
        
        if (unlimited) return 0;
        if (obj is Spear) return 5;
        if (player.CanBeSwallowed((PhysicalObject)obj) || obj is IPlayerEdible) return 10;
        if (player.Grabability(obj) <= Player.ObjectGrabability.BigOneHand) return 15;
        if (!player.HeavyCarry(obj)) return 25;
        return 100;
    }*/



    public void ReloadCapacity()
    {
        itemsOnBack.lanternCount = 0;
        int result = 0;
        foreach (AbstractPhysicalObject obj in Items) 
        {
            result += ItemVolumeFromAbstr(obj);
            if (obj.type == AbstractPhysicalObject.AbstractObjectType.Lantern) 
            { 
                itemsOnBack.lanternCount++; 
            }
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
    public int refuseCounter;
    public List<float> rowsWidth;

    public HUDCircle test;

    public InventoryHUD(HUD.HUD hud, FContainer fContainer, Inventory owner) : base(hud)
    {
        this.owner = owner;
        pos = owner.player.mainBodyChunk.pos;
        lastPos = pos;
        fade = 0f;
        lastFade = 0f;
        refuseCounter = 0;
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

        if (refuseCounter > 0)
        {
            refuseCounter--;
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
                icon.Draw(refuseCounter, fade, DrawPos(timeStacker), timeStacker);
                
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
            if (Plugin.DevMode)
            {
                string s = icon.symbol.spriteName.ToString();
                Plugin.Log("iconspritename:", s);
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


        public void Draw(int refuseCounter, float fade, Vector2 drawPos, float timeStacker)
        {
            symbol.Draw(timeStacker, drawPos + offset);

            if (refuseCounter == 0)
            {
                symbol.symbolSprite.alpha = fade * 0.8f;
            }
            else
            {
                symbol.symbolSprite.alpha = fade * 0.8f + Mathf.Sin(refuseCounter) * 0.1f;
            }
            
        }

        public void RemoveSprites()
        {
            symbol.RemoveSprites();
        }
    }
}
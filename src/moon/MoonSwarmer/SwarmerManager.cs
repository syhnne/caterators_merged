using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using UnityEngine;
using RWCustom;
using HUD;
using Caterators_by_syhnne.nsh;
using static Caterators_by_syhnne.nsh.InventoryHUD;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Schema;

namespace Caterators_by_syhnne.moon.MoonSwarmer;




public class SwarmerManager
{
    public Player player;
    public static int maxSwarmer = 5;
    public List<MoonSwarmer> swarmers;
    public _public.DeathPreventer deathPreventer;
    public SwarmerHUD hud;

    // 以下大概是根据剩余的神经元数量，分为三个档位（？
    public bool alive;
    public bool weakMode;
    public bool agility;

    public int callBackCounter = 0;

    /// <summary>
    /// 剩余的神经元数量是否多于1
    /// </summary>
    public bool CanConsumeSwarmer
    {
        get
        {
            int count = 0;
            foreach (MoonSwarmer s in swarmers)
            {
                if (s.State.alive)
                {
                    count++;
                }
            }
            return count > 1;
        }
    }

    public MoonSwarmer? LastAliveSwarmer
    {
        get
        {
            for (int i = swarmers.Count - 1; i >= 0; i--)
            {
                if (swarmers[i].State.alive)
                {
                    return swarmers[i];
                }
            }
            return null;
        }
    }










    public SwarmerManager(Player player)
    {
        swarmers = new();
        this.player = player;
        alive = true;
        weakMode = false;
        SwarmersUpdate();
    }


    public void Update()
    {
        if (player.dead)
        {
            alive = false;  
        }

        if (weakMode)
        {
            if (player.aerobicLevel > 0.95f)
            {
                player.exhausted = true;
            }
            else if (player.aerobicLevel < 0.4f)
            {
                player.exhausted = false;
            }
            if (player.exhausted)
            {
                player.Blink(3);
                player.slowMovementStun = Math.Max(player.slowMovementStun, (int)Custom.LerpMap(player.aerobicLevel, 0.7f, 0.4f, 6f, 0f));
                if (player.aerobicLevel > 0.9f && Random.value < 0.05f)
                {
                    player.Stun(7);
                }
                if (player.aerobicLevel > 0.9f && Random.value < 0.1f)
                {
                    player.standing = false;
                }
                if (!player.lungsExhausted || !(player.animation != Player.AnimationIndex.SurfaceSwim))
                {
                    player.swimCycle += 0.05f;
                }
            }
            else
            {
                player.slowMovementStun = Math.Max(player.slowMovementStun, (int)Custom.LerpMap(player.aerobicLevel, 1f, 0.4f, 2f, 0f, 2f));
            }
        }


        /*if (player.room != null && player.room.abstractRoom.shelter)
        {
            CallBack();

        }*/
    }


    /// <summary>
    /// callback(x) 重开（v)
    /// </summary>
    public void CallBack()
    {
        callBackCounter = 0;
        // 防止玩家在此时损失神经元（？
        deathPreventer?.SetInvuln();
        foreach (var swarmer in swarmers)
        {
            callBackCounter++;
            swarmer.Kill(false);
        }
        swarmers = null;
    }




    public void Respawn()
    {
        deathPreventer?.DisableInvuln();
        if (callBackCounter == 0)
        {
            Plugin.Log("Warning: respawn count = 0");
            return;
        }
        SpawnSwarmer(callBackCounter);
    }



    public void SwarmersUpdate()
    {
        switch (swarmers.Count())
        {
            case 0:
                if (player.stillInStartShelter) break; // 不加这句的下场就是，一点开游戏就看到已经死了（（（
                if (deathPreventer != null) { deathPreventer.dontRevive = true; } // 防止deathpreventer再消耗神经元复活玩家（。
                break;
            case 1:
            case 2: 
                weakMode = true; agility = false;
                break;
            case 3: 
            case 4: 
                weakMode = false; agility = false;
                break;
            default:
                weakMode = false; agility = true;
                break;
        }
        hud?.UpdateIcons();
        Plugin.Log("has swarmers:", swarmers.Count(), "weak:", weakMode, "agility:", agility);
    }




    public void SpawnSwarmer(int number = 1)
    {
        Plugin.Log("spawn MoonSwarmer");
        for (int i = 0; i < number; i++)
        {
            AbstractCreature abstr = new AbstractCreature(player.room.world, StaticWorld.GetCreatureTemplate(MoonSwarmerCritob.MoonSwarmer), null, player.abstractCreature.pos, player.room.game.GetNewID());
            player.room.abstractRoom.AddEntity(abstr);
            abstr.RealizeInRoom();
            // (abstr.realizedCreature as MoonSwarmer).stickToPlayer = new Player.AbstractOnBackStick(player.abstractCreature, abstr);
            abstr.realizedObject.firstChunk.pos = player.firstChunk.pos;
            (abstr.realizedCreature as MoonSwarmer).AI.manager = this;
            swarmers.Add(abstr.realizedCreature as MoonSwarmer);
            
        }
        SwarmersUpdate();
    }



    public bool KillSwarmer(MoonSwarmer s, bool explode)
    {
        bool result = false;
        if (s != null)
        {
            s.isActive = false;
            s.killTag = null;
            s.Kill(explode);
            swarmers.Remove(s);
            Plugin.Log("killed, moon has swarmer:", swarmers.Count);
            result = true;
        }
        SwarmersUpdate();
        return result;
    }


    public void CycleEndSave()
    {
        CallBack();
        if (player.room == null)
        {
            Plugin.Log("Warning: Null player.room, neurons not saved");
            return;
        }
        if (callBackCounter == 0)
        {
            Plugin.Log("Warning: callback count = 0");
            callBackCounter = swarmers.Count;
        }
        player.room.game.GetDeathPersistent().MoonHasSwarmers = callBackCounter;
       
    }




    public void Destroy()
    {

    }


}









public class SwarmerHUD : HudPart
{
    public SwarmerManager owner;
    public Vector2 pos;
    public Vector2 lastPos;
    public float fade;
    public float lastFade;
    public List<FSprite> icons;
    public FSprite lineSprite;

    public int showCount;
    public int showCountDelay;
    public int visibleCounter;
    public float downInCorner;

    private static float xSpacing = 20f;

    public FContainer fContainer
    {
        get
        {
            return this.hud.fContainers[1];
        }
    }

    public SwarmerHUD(HUD.HUD hud, FContainer fContainer, SwarmerManager owner) : base(hud)
    {
        this.owner = owner;
        this.pos = new Vector2(Mathf.Max(50f, hud.rainWorld.options.SafeScreenOffset.x + 5.5f), 25f + Mathf.Max(25f, hud.rainWorld.options.SafeScreenOffset.y + 17.25f));
        this.lastPos = this.pos;
        fade = 0f;
        lastFade = 0f;
        showCount = 0;
        showCountDelay = 0; 
        visibleCounter = 0;

        icons = new List<FSprite>();
        for (int i = 0; i < SwarmerManager.maxSwarmer; i++)
        {
            icons.Add(new FSprite("Symbol_Neuron", false));
        }
        foreach (FSprite icon in icons)
        {
            icon.isVisible = true;
            fContainer.AddChild(icon);
        }

        lineSprite = new FSprite("pixel", true);
        lineSprite.isVisible = true;
        lineSprite.scaleX = 2f;
        lineSprite.scaleY = 32f;
        fContainer.AddChild(this.lineSprite);

    }



    public void UpdateIcons()
    {
        foreach (var icon in icons)
        {
            icon.RemoveFromContainer();
        }
        icons.Clear();

        lineSprite.isVisible = owner.swarmers != null && owner.swarmers.Count >= SwarmerManager.maxSwarmer;


        for (int i = 0; i < (owner.swarmers != null? owner.swarmers.Count : owner.callBackCounter); i++)
        {
            icons.Add(new FSprite("Symbol_Neuron", false) { color = Color.white });
        }

        if (icons.Count < SwarmerManager.maxSwarmer)
        {
            for (int i = (owner.swarmers != null ? owner.swarmers.Count : owner.callBackCounter) - 1; i < SwarmerManager.maxSwarmer - 1; i++)
            {
                icons.Add(new FSprite("Symbol_Neuron", false) { color = new Color(0.3f, 0.3f, 0.3f) });
            }
        }

        foreach (var icon in icons)
        {
            this.fContainer.AddChild(icon);
        }

    }



    public override void Update()
    {
        base.Update();
        this.lastFade = this.fade;
        this.lastPos = this.pos;

        if (this.hud.owner.RevealMap || this.hud.showKarmaFoodRain)
        {
            float num = Mathf.Max((this.visibleCounter > 0) ? 1f : 0f, 1f);
            if (this.hud.showKarmaFoodRain)
            {
                num = 1f;
            }
            if (this.fade < num)
            {
                this.fade = Mathf.Min(num, this.fade + 0.1f);
            }
            else
            {
                this.fade = Mathf.Max(num, this.fade - 0.1f);
            }
            UpdateShowCount();
        }
        else
        {
            if (this.visibleCounter > 0)
            {
                if (!ModManager.MMF || !this.hud.HideGeneralHud)
                {
                    this.visibleCounter--;
                }
                this.fade = Mathf.Min(1f, this.fade + 0.1f);
            }
            else
            {
                this.fade = Mathf.Max(0f, this.fade - 0.0125f);
            }
            if (this.fade == 1f)
            {
                this.UpdateShowCount();
            }
            else if (this.fade == 0f)
            {
                this.showCountDelay = 15;
            }
        }

        /*if (this.downInCorner > 0f && this.hud.karmaMeter.AnyVisibility && hud.foodMeter.visibleCounter > 0)
        {
            this.downInCorner = Mathf.Max(0f, this.downInCorner - 0.0625f);
        }
        else if (this.fade > 0f && this.hud.karmaMeter.fade == 0f && !this.hud.karmaMeter.AnyVisibility && hud.foodMeter.visibleCounter == 0)
        {
            this.downInCorner = Mathf.Min(1f, this.downInCorner + 0.0625f);
        }*/

        // 好好好 方便极了（
        pos = hud.foodMeter.pos;
        pos.y += 34f;
    }



    // 懒癌发作了，进行一个复制粘贴
    private void UpdateShowCount()
    {
        bool flag = true;
        if (ModManager.MMF && this.showCount < this.icons.Count)
        {
            flag = false;
        }
        if (flag)
        {
            if (this.showCountDelay != 0)
            {
                this.showCountDelay--;
                return;
            }
            this.showCountDelay = 10;
            this.showCount++;
        }
    }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame) return;


        base.Draw(timeStacker);

        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].alpha = fade;
            icons[i].SetPosition(DrawPos(timeStacker, i));
        }
        lineSprite.alpha = fade;
        Vector2 linePos = DrawPos(timeStacker, SwarmerManager.maxSwarmer);
        linePos.x -= 0.5f * xSpacing; 
        lineSprite.SetPosition(linePos);

    }


    public Vector2 DrawPos(float timeStacker, int index = 0)
    {
        Vector2 res = Vector2.Lerp(lastPos, pos, timeStacker);
        res.x += index * xSpacing;
        return res;
    }


    public override void ClearSprites()
    {
        base.ClearSprites();
        foreach (var icon in icons)
        {
            icon.RemoveFromContainer();
        }
        icons.Clear();
        icons = null;
        lineSprite.RemoveFromContainer();
        lineSprite = null;
    }

}
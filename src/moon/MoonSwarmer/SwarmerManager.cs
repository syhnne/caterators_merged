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
using Fisobs.Core;

namespace Caterators_by_syhnne.moon.MoonSwarmer;



// TODO: 每次钻管道的时候存储一下管道所处的房间和编号。如果神经元不在其中任何一个房间里，且两个管道不在同一个房间，那就全都传送到倒数第二个管道处
public class SwarmerManager
{
    // : 坏了 这个地方应该用弱引用 但我前面写了那么多东西全都不是弱引用（（
    // 摆烂了，不改了，看起来对游戏性能没什么影响
    public WeakReference<Player> owner;
    public Player? player
    {
        get
        {
            if (owner.TryGetTarget(out Player player)) { return player; }
            return null;
        }
    }
    public int lastPlayerRoom;
    public int playerRoom;
    public static int maxSwarmer = 5;
    public List<AbstractCreature> swarmers;
    public _public.DeathPreventer deathPreventer;
    public SwarmerHUD hud;
    public bool saved = false;
    public int destCounter;

    // 以下大概是根据剩余的神经元数量，分为三个档位（？
    public bool alive;
    public bool weakMode;
    public bool agility;

    // 没事了 这个好像不好使
    public bool needTeleportOnNextShortcut = false;
    public int? callBackSwarmers;

    public bool tryingToTeleport = false;
    public float tpDistance = 80f;
    private bool tryingToCallBack = false;
    private int teleportRetryCounter = 0;
    private int callBackRetryCounter = 0;

    public int meditateTick = 0;

    public int hasSwarmers
    {
        get { return callBackSwarmers != null ? (int)callBackSwarmers : swarmers.Count; }
    }

    /// <summary>
    /// 剩余的神经元数量是否多于1
    /// </summary>
    public bool CanConsumeSwarmer
    {
        get
        {
            int count = 0;
            foreach (AbstractCreature s in swarmers)
            {
                if (s.realizedCreature != null && s.realizedCreature.State.alive)
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
            if (swarmers == null) return null;
            for (int i = swarmers.Count - 1; i >= 0; i--)
            {
                if (swarmers[i].realizedCreature != null && swarmers[i].realizedCreature.State.alive)
                {
                    return swarmers[i].realizedCreature as MoonSwarmer;
                }
            }
            return null;
        }
    }

    public AbstractCreature? FurthestSwarmer
    {
        get
        {
            if (swarmers == null || player.room == null) return null;
            float dist = 0f;
            int index = 0;
            for (int i = 0; i < swarmers.Count; i++)
            {
                float d = Custom.BetweenRoomsDistance(player.room.world, swarmers[i].pos, player.abstractCreature.pos);
                if (d > dist)
                {
                    dist = d;
                    index = i; 
                }
            }
            return swarmers[index];
        }
    }










    public SwarmerManager(Player player)
    {
        swarmers = new();
        this.owner = new WeakReference<Player>(player);
        alive = true;
        weakMode = false;
        SwarmersUpdate();
    }


    public void Update(bool eu)
    {
        if (player == null) { Plugin.Log("swarmermanager update: null player!!"); return; }

        if (player.dead)
        {
            alive = false;  
        }

        if (tryingToCallBack)
        {
            tryingToCallBack = !(CallBack() || callBackRetryCounter > 3);
            Plugin.Log("trying to callback for", callBackRetryCounter, "times");
            callBackRetryCounter++;
        }
        if (tryingToTeleport)
        {
            tryingToTeleport = !TryTeleportAllSwarmers();
            teleportRetryCounter++;
            if (teleportRetryCounter > 10)
            {
                Plugin.Log("retried for 10 times, trying to callback");
                tryingToCallBack = true;
                tryingToTeleport = false;
            }
        }
        else
        {
            teleportRetryCounter = 0;
        }

        if (destCounter < 20) destCounter++;
        else destCounter = 0;

        if (weakMode)
        {
            // 我想在虚弱模式下砍点移速，但不会砍，算了，算你们走运（背手离去
            // player.dynamicRunSpeed[0] *= 0.9f;
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

        // 受不了了，不知道是不是因为我把player改成了弱引用，隔壁ai那里死活找不到玩家在哪。还是这样统一管理吧。
        // 以后可以直接参考原版神经元的代码，能直接在房间里搜索到玩家才会跟着玩家走
        // 至于tp有没有问题，我想大约是没有了罢
        foreach (AbstractCreature sw in swarmers)
        {
            if (sw.realizedCreature != null && (sw.realizedCreature as MoonSwarmer).notInSameRoom)
            {
                sw.realizedCreature.Update(eu);
            }
            if (destCounter == 0)
            {
                if (sw.realizedCreature != null && player != null)
                {
                    (sw.realizedCreature as MoonSwarmer).AI?.SetDestination(player.abstractCreature.pos);
                }
                else if (sw.realizedCreature != null)
                {
                    (sw.realizedCreature as MoonSwarmer).AI?.SetDestination(sw.pos);
                }
            }
        }

        bool idle = player != null && player.room != null && callBackSwarmers == null && swarmers != null && player.onBack == null && player.touchedNoInputCounter > 160;
        foreach (AbstractCreature sw in swarmers)
        {
            if (!(sw.realizedCreature != null && (sw.realizedCreature as MoonSwarmer).inSameRoom))
            {
                // Plugin.Log("swarmers idle set to false because not in same room");
                idle = false; break;
            }
        }
        /*WorldCoordinate co = player.abstractCreature.pos;
        if (player.room.GetTile(co).Solid) idle = false;
        co.y--;
        if (player.room.GetTile(co).Solid) idle = false;*/
        if (!player.room.aimap.IsFreeSpace(player.abstractCreature.pos.Tile, 4)) idle = false;
        if (player.onBack != null || player.room == null || player.room.abstractRoom.shelter || player.room.abstractRoom.gate) { idle = false; }
        IdleAtPlayerPos(idle? HoverAnimation.Circle : HoverAnimation.None);


        /*if (player.room != null && player.room.abstractRoom.shelter)
        {
            CallBack();

        }*/
    }


    /// <summary>
    /// callback(x) 重开（v)
    /// 用来在避难所和业力门门口回收所有的神经元，避免业力门把你的神经元关外面
    /// 不要在这里updateswarmers，经测试这会使神经元数量变成-1并引发deathpreventer的自动防御
    /// </summary>
    public bool CallBack()
    {
        if (player == null) { return false; }
        if (swarmers.Count == 0 && callBackSwarmers != null) return true;
        try
        {
            callBackSwarmers ??= 0;
            deathPreventer.forceRevive = true;
            Plugin.Log("CallBack() before:", swarmers.Count);

            for (int i = swarmers.Count - 1; i >= 0; i--)
            {
                if (swarmers[i].realizedCreature != null)
                {
                    (swarmers[i].realizedCreature as MoonSwarmer).Kill(false);
                }
                swarmers[i].slatedForDeletion = true;
                swarmers.RemoveAt(i);
                callBackSwarmers++;
            }
            Plugin.Log("CallBack(): after:", callBackSwarmers);
            return true;
        }
        catch (Exception ex)
        {
            Plugin.LogException(ex);
            return false;
        }
    }




    public bool Respawn()
    {
        if (player == null) return false;
        deathPreventer.forceRevive = false;
        if (callBackSwarmers == null || callBackSwarmers == 0)
        {
            Plugin.Log("Respawn(): null callBackSwarmers");
            return false;
        }
        SpawnSwarmer((int)callBackSwarmers);
        callBackSwarmers = null;
        SwarmersUpdate();
        return true;
    }



    public void SwarmersUpdate()
    {
        if (player == null) { Plugin.Log("SwarmersUpdate(): null player"); return; }
        switch (hasSwarmers) 
        { 
            case 0:
                if (player.stillInStartShelter) break; // 不加这句的下场就是，一点开游戏就看到已经死了（（（
                weakMode = true; agility = false;
                if (deathPreventer != null) { deathPreventer.dontRevive = true; } // 防止deathpreventer再消耗神经元复活玩家（。
                player.Die();
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
        Plugin.Log("has swarmers:", hasSwarmers, "weak:", weakMode, "agility:", agility);
    }



    /// <summary>
    /// 一般来说不要直接调用这个，最好给callBackSwarmers赋值然后使用Respawn()
    /// </summary>
    /// <param name="number"></param>
    public void SpawnSwarmer(int number = 1, Vector2? spawnPos = null)
    {
        if (player == null) return;
        Plugin.Log("spawn MoonSwarmer:", number);
        for (int i = 0; i < number; i++)
        {
            AbstractCreature abstr = new AbstractCreature(player.room.world, StaticWorld.GetCreatureTemplate(MoonSwarmerCritob.MoonSwarmer), null, player.abstractCreature.pos, player.room.game.GetNewID());
            player.room.abstractRoom.AddEntity(abstr);
            abstr.RealizeInRoom();
            // (abstr.realizedCreature as MoonSwarmer).stickToPlayer = new Player.AbstractOnBackStick(player.abstractCreature, abstr);
            abstr.realizedObject.firstChunk.pos = spawnPos != null? (Vector2)spawnPos : player.firstChunk.pos;
            (abstr.realizedCreature as MoonSwarmer).manager = this;
            (abstr.realizedCreature as MoonSwarmer).AI.manager = this;
            swarmers.Add(abstr);
            
        }
        SwarmersUpdate();
    }


    // 目前这东西是一个有缝衔接，因为那个颜色衔接他不知道为啥压根不干活
    // 不TODO了，我放弃了
    public void ConvertNSHSwarmer(nsh.ReviveSwarmerModules.ReviveSwarmer swarmer)
    {
        if (player == null || swarmer.room == null) { Plugin.Log("ConvertNSHSwarmer(): null room or null player"); return; }

        Plugin.Log("converting NSHswarmer");
        if (callBackSwarmers != null)
        {
            swarmer.Use(false);
            callBackSwarmers++;
            return;
        }
        Vector2 vel = swarmer.firstChunk.vel;
        Vector2 pos = swarmer.firstChunk.pos;

        swarmer.Use(false);
        SpawnSwarmer(1, pos);
        swarmers[swarmers.Count - 1].realizedCreature.firstChunk.vel = vel;
        (swarmers[swarmers.Count - 1].realizedCreature as MoonSwarmer).SpawnColorLerp();
    }



    public bool KillSwarmer(MoonSwarmer s, bool explode)
    {
        bool result = false;
        if (s != null)
        {
            s.isActive = false;
            s.killTag = null;
            s.Kill(explode);
            swarmers.Remove(s.abstractCreature);
            Plugin.Log("killed, moon has swarmer:", hasSwarmers);
            result = true;
        }
        SwarmersUpdate();
        return result;
    }



    public int? CycleEndSave()
    {
        if (saved)
        {
            return null;
        }
        if (player == null)
        {
            return maxSwarmer;
        }
        if (callBackSwarmers  != null)
        {
            return (int)callBackSwarmers;
        }
        for (; ; )
        {
            if (CallBack()) break;
        }
        if (callBackSwarmers == null || callBackSwarmers == 0)
        {
            Plugin.Log("CycleEndSave(): null callBackSwarmers");
            callBackSwarmers = swarmers.Count;
        }
        int result = (int)callBackSwarmers;
        saved = true;
        callBackSwarmers = null;
        return result;
    }



    public void Player_SpitOutOfShortCut(bool stillInStartShelter)
    {
        if (player == null || player.room == null) return;
        lastPlayerRoom = playerRoom;
        playerRoom = player.room.abstractRoom.index;
        if (callBackSwarmers != null)
        {
            Respawn();
        }
        else if (player.room.abstractRoom.gate || player.room.abstractRoom.shelter)
        {
            /*for (; ; )
            {
                if (CallBack()) break;
            }*/
            tryingToCallBack = true;
            return;
        }
        // 去掉这个传送会不会让他们完全跟不上
        // 会，奶奶的，他们几个不会互相谦让，会卡在管道口，而且跟随玩家的速度极慢，还是得想办法传送
        // 有一个小阴招：砍一点玩家的移速，让它们更容易跟上（（
        else if (lastPlayerRoom != playerRoom)
        {
            tpDistance = 80f;
            tryingToTeleport = true;
        }
        /*else if (needTeleportOnNextShortcut)
        {
            tryingToTeleport = true;
            needTeleportOnNextShortcut = false;
        }*/

        // 草 既然如此 不如把远到一定程度的全都tp过来
        // 我估摸着这个距离上限也就在120左右 再远的就要被abstractize了
        /*AbstractCreature s = FurthestSwarmer;
        if (s != null) TryTeleportSwarmer(s);*/

        foreach (AbstractCreature sw in swarmers)
        {
            if (sw.realizedCreature != null && player != null)
            {
                (sw.realizedCreature as MoonSwarmer).AI?.SetDestination(player.abstractCreature.pos);
            }
            else if (sw.realizedCreature != null)
            {
                (sw.realizedCreature as MoonSwarmer).AI?.SetDestination(sw.pos);
            }

        }



    }



    public enum HoverAnimation
    {
        None,
        Circle,

    }


    public void IdleAtPlayerPos(HoverAnimation ani)
    {
        if (callBackSwarmers != null || swarmers == null) { return; }
        for (int i = 0; i < swarmers.Count; i++)
        {
            if (swarmers[i].realizedCreature == null || (swarmers[i].realizedCreature as MoonSwarmer).notInSameRoom) return;
        }
        meditateTick++;
        for (int i = 0; i < swarmers.Count; i++)
        {
            switch (ani)
            {
                case HoverAnimation.Circle:
                    if (swarmers[i].realizedCreature == null) continue;
                    float num = 20f;
                    float num2 = (float)this.meditateTick * 0.035f;
                    if (i % 2 == 0)
                    {
                        num *= Mathf.Sin(num2);
                    }
                    else
                    {
                        num *= Mathf.Cos(num2);
                    }
                    float num3 = ((float)i * (6.2831855f / (float)swarmers.Count) + (float)this.meditateTick * 0.0035f);
                    num3 %= 6.2831855f;
                    float num4 = 90f + num;
                    Vector2 vector = new(Mathf.Cos(num3) * num4 + player.firstChunk.pos.x, -Mathf.Sin(num3) * num4 + player.firstChunk.pos.y);
                    Vector2 newPos = new(swarmers[i].realizedCreature.firstChunk.pos.x + (vector.x - swarmers[i].realizedCreature.firstChunk.pos.x) * 0.05f, swarmers[i].realizedCreature.firstChunk.pos.y + (vector.y - swarmers[i].realizedCreature.firstChunk.pos.y) * 0.05f);
                    swarmers[i].realizedCreature.firstChunk.vel = Vector2.zero;
                    (swarmers[i].realizedCreature as MoonSwarmer).forceHoverPos = newPos;
                    break;
                case HoverAnimation.None:
                    (swarmers[i].realizedCreature as MoonSwarmer).forceHoverPos = null;
                    break;
            }
        }
    }
			



    public bool TryTeleportAllSwarmers()
    {
        if (player == null || swarmers == null || swarmers.Count == 0 || callBackSwarmers != null) return false;
        Plugin.Log("trying to teleport swarmers for", teleportRetryCounter, "times:");

        bool succeed = true;
        for (int i = 0; i < swarmers.Count; i++)
        {
            if (player.room != null && Custom.BetweenRoomsDistance(player.room.world, swarmers[i].pos, player.abstractCreature.pos) < tpDistance)
            {
                Plugin.Log("skipping", swarmers[i].ID.number);
                continue;
            }
            if (!TryTeleportSwarmer(swarmers[i]))
            {
                Plugin.Log("!! failed to teleport swarmer", i);
                succeed = false;
            }
        }

        // player.room.abstractRoom.realizedRoom.aimap.NewWorld(player.room.abstractRoom.index);
        return succeed;
    }



    // 算了 这个不好 他会让所有神经元全卡在管道里
    /*public void ForceAllSwarmersIntoShortcut(IntVector2 entrancePos)
    {
        if (callBackSwarmers != null)
        {
            Plugin.Log("callbackswarmers not forced into shortcut");
            return;
        }
        if (player.room == null) return;
        bool enteringShortcut = !player.room.shortcutData(entrancePos).ToNode || player.room.abstractRoom.nodes[player.room.shortcutData(entrancePos).destNode].type == AbstractRoomNode.Type.Exit;
        bool enteringDen = player.room.shortcutData(entrancePos).ToNode && player.room.abstractRoom.nodes[player.room.shortcutData(entrancePos).destNode].type == AbstractRoomNode.Type.Den;
        if (enteringShortcut && enteringDen) 
        {
            Plugin.Log("player entering den, not forced into shortcut");
            return; 
        }
        Plugin.Log("forcing all swarmers into shortcut:", entrancePos.ToString());
        foreach(var swarmer in swarmers)
        {
            if (swarmer.realizedCreature == null) continue;
            (swarmer.realizedCreature as MoonSwarmer).ForceIntoShortcut(entrancePos);
        }
    }*/



    public bool TryTeleportSwarmer(AbstractCreature swarmer)
    {
        if (player == null || player.room == null) return false;
        try
        {
            
            if (swarmer.realizedCreature != null)
            {
                if ((swarmer.realizedCreature as MoonSwarmer).justTeleported > 0)
                {
                    Plugin.Log("swarmer", swarmer.ID.number, "just teleported");
                    return true;
                }
                if (swarmer.realizedCreature.room != null && swarmer.realizedCreature.room == player.room)
                {
                    Plugin.Log("teleporting", swarmer.ID.number, "in player room");
                    swarmer.realizedCreature.firstChunk.HardSetPosition(player.mainBodyChunk.pos);
                }
                else if (swarmer.realizedCreature.room != null)
                {
                    Plugin.Log("teleporting", swarmer.ID.number, "from", swarmer.Room.name, " to player room:", player.room.abstractRoom.name);
                    swarmer.realizedCreature.room?.RemoveObject(swarmer.realizedCreature);
                    swarmer.destroyOnAbstraction = false;
                    swarmer.Abstractize(player.abstractCreature.pos);
                    swarmer.RealizeInRoom();
                }
                else 
                {
                    Plugin.Log("realized swarmer", swarmer.ID.number, "in null room, retry");
                    return false; 
                }
            }
            else
            {
                Plugin.Log("teleporting abstract", swarmer.ID.number, "from", swarmer.Room.name, " to player room:", player.room.abstractRoom.name);
                swarmer.Move(player.abstractCreature.pos);
                swarmer.RealizeInRoom();
            }
            (swarmer.realizedCreature as MoonSwarmer).manager = this;
            (swarmer.realizedCreature as MoonSwarmer).justTeleported = 60;
            // (swarmer.realizedCreature as MoonSwarmer).AI?.SwitchBehavior(MoonSwarmerAI.Behavior.FollowPlayer);
            (swarmer.realizedCreature as MoonSwarmer).AI?.SetDestination(player.abstractCreature.pos);
            swarmer.realizedCreature.firstChunk.pos = player.mainBodyChunk.pos;
            swarmer.realizedCreature.firstChunk.vel = player.mainBodyChunk.vel;
            return true;
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
            Plugin.Log("failed to teleport", swarmer.ID.number);
            return false;
        }
    }



    public void LogAllSwarmersData()
    {
        Plugin.Log();
        Plugin.Log(" ~ swarmers ~ playerPos: ", player.abstractCreature.pos);
        if (player == null)
        {
            Plugin.Log(" ~ DISCONNECTED");
        }
        if (callBackSwarmers != null)
        {
            Plugin.Log(" ~ not realized, has swarmers:" + callBackSwarmers.ToString());
        }
        else
        {
            foreach (var swarmer in swarmers)
            {
                string msg = player.abstractCreature.ID.number + " ~ " + swarmer.ID.number + " - pos: " + swarmer.pos.ToString();
                if ((swarmer.realizedCreature as MoonSwarmer).AI != null && (swarmer.realizedCreature as MoonSwarmer).AI.pathFinder != null)
                {
                    msg += " | ai destination:" + (swarmer.realizedCreature as MoonSwarmer).AI.pathFinder.destination;
                }
                if ((swarmer.realizedCreature as MoonSwarmer).debugConnectionEnd != null)
                {
                    msg += " | debugConnectionEnd: " + (swarmer.realizedCreature as MoonSwarmer).debugConnectionEnd.ToString();
                }
                Plugin.Log(msg);
            }
        }
        Plugin.Log();
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

        lineSprite.isVisible = owner.hasSwarmers > SwarmerManager.maxSwarmer;

        
        for (int i = 0; i < owner.hasSwarmers; i++)
        {
            icons.Add(new FSprite("Symbol_Neuron", false) { color = Color.white });
        }

        if (icons.Count < SwarmerManager.maxSwarmer)
        {
            for (int i = owner.hasSwarmers - 1; i < SwarmerManager.maxSwarmer - 1; i++)
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












public class DebugHUD : HudPart
{
    public SwarmerManager owner;
    public MoonSwarmer? swarmer;
    public Vector2 dest;
    public Vector2 lastDest;
    public Vector2 end;
    public Vector2 lastEnd;
    public float fade;
    public float lastFade;
    /// <summary>
    /// destination
    /// </summary>
    public FSprite destSprite;
    /// <summary>
    /// connectionEnd
    /// </summary>
    public FSprite endSprite;

    // blueish green / blue
    public static Color32 destColor = new Color32(98, 255, 223, 255);
    // orange / red
    public static Color32 endColor = new Color32(255, 125, 29, 255);

    public DebugHUD(HUD.HUD hud, FContainer fContainer, SwarmerManager owner) : base(hud)
    {
        this.owner = owner;
        
        destSprite = new FSprite("Futile_White", true)
        {
            scale = 0.5f,
            color = destColor,
        };
        
        endSprite = new FSprite("Futile_White", true)
        {
            scale = 0.5f,
            color = endColor,
        };
        fContainer.AddChild(destSprite);
        fContainer.AddChild(endSprite);
    }


    private bool Show
    {
        get
        {
            return owner != null && swarmer != null && swarmer.room.game.devToolsActive
                && swarmer != null && swarmer.room != null && swarmer.AI != null && swarmer.AI.pathFinder != null && swarmer.AI.pathFinder.destination != null;
                
        }
    }


    public override void Update()
    {

        base.Update();
        /*if (Input.GetKeyDown(KeyCode.Backspace))
        {
            visible = !visible;
        }*/

        lastDest = dest;
        lastEnd = end;
        lastFade = fade;
        Vector2 camPos = Vector2.zero;
        if (owner != null && (owner.LastAliveSwarmer == null || (owner.LastAliveSwarmer != null && owner.LastAliveSwarmer != swarmer)))
        {
            swarmer = owner.LastAliveSwarmer;
        }

        if (swarmer == null || swarmer.room == null) return; ////////////

        if (owner.playerRoom != swarmer.room.abstractRoom.index)
        {
            destSprite.color = Color.blue;
            endSprite.color = Color.red;
        }
        else
        {
            destSprite.color = destColor; 
            endSprite.color = endColor;
        }

        if ( swarmer.room != null)
        {
            camPos = swarmer.room.game.cameras[0].pos;
        }
        dest = swarmer.debugDest - camPos;
        end = swarmer.debugConnectionEnd - camPos;

        destSprite.isVisible = true;
        endSprite.isVisible = true;
        if (Show)
        {
            fade = Mathf.Min(1f, fade + 0.033333335f);
        }
        else
        {
            fade = Mathf.Max(0f, fade - 0.1f);
        }




    }




    public Vector2 DrawPos(Vector2 pos, Vector2 lastPos, float timeStacker)
    {
        return Vector2.Lerp(lastPos, pos, timeStacker);
    }

    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame) return;
        destSprite.alpha = fade;
        endSprite.alpha = fade;
        destSprite.SetPosition(DrawPos(dest, lastDest, timeStacker));
        endSprite.SetPosition(DrawPos(end, lastEnd, timeStacker));
    }


    public override void ClearSprites()
    {
        base.ClearSprites();
        destSprite.RemoveFromContainer();
        destSprite = null;
        endSprite.RemoveFromContainer();
        endSprite = null;
    }



}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using Noise;

namespace Caterators_by_syhnne.moon.MoonSwarmer;




/*这波是把fisobs用到极致了
    回头再写吧（疲劳
    准备继承Creature类（乐，真成散装机猫了）不这样的话它钻不了管道
    感觉除了替死以外要是还有别的功能会好一点
    另外需要写一个nsh的复活神经元和这个之间的无缝衔接 也就是说moon活着的时候也能被复活 参考SLOracleBehavior.ConvertingSSSwarmer()
    所以说nsh复活队友的时候最好拿着神经元 不然无缝转换就是有缝转换了
    呃啊 那咋整啊 弄一个圣猫同款大狙吗 什么暴力奶妈（？？

    奶奶的。。这玩意儿真的难写。。*/

// TODO: 让这玩意儿学会下台阶，他现在动不动就卡在平台上

public class MoonSwarmer : Creature
{
    // 好好好 这个manager被引用了14次 我特么测了三天才发现自己从来没给它赋过值
    public SwarmerManager manager = null;
    public MoonSwarmerAI AI = null;
    // 但是这东西会把玩家绑架在管道里 算了 还是老老实实写tp吧
    // public Player.AbstractOnBackStick stickToPlayer;
    public bool isActive;

    public Player? player
    {
        get { return manager?.player; }
    }

    public float rotation;
    public float lastRotation;
    public Vector2 direction;
    public Vector2 lastDirection;
    public Vector2 lazyDirection;
    public Vector2 lastLazyDirection;
    public float revolveSpeed;
    public bool lastVisible;
    public Vector2 drift;

    public float moveSpeed;

    public bool callBack = false;

    public int justTeleported = 0;
    public int cantSeeCounter = 0;
    /*{
        get
        {
            return manager != null && manager.player.room != null && manager.player.room.abstractRoom.shelterIndex != -1;
        }
    }*/
    public int notInSameRoomCounter = 0;
    public bool notInSameRoom => room != null && reachable && manager.player.room != null && room.abstractRoom.index != manager.player.room.abstractRoom.index;
    public bool reachable => manager != null && manager.player != null;
    public float affectedByGravity = 1f;
    public MovementConnection currentConnection;

    public MoonSwarmer(AbstractCreature abstr) : base(abstr, abstr.world)
    {
        isActive = true;
        graphicsModule = new MoonSwarmerGraphics(this, false);

        this.collisionLayer = 1;
        base.bodyChunks = new BodyChunk[1];
        base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), 3f, 0.2f);
        base.bodyChunks[0].loudness = 0.01f;
        bodyChunks[0].mass = 0.1f;
        this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
        base.airFriction = 0.999f;
        base.gravity = 0.9f;
        this.bounce = 0.2f;
        this.surfaceFriction = 0.4f;
        base.waterFriction = 0.94f;
        base.buoyancy = 1.1f;
        this.rotation = 0.25f;
        this.lastRotation = this.rotation;
        moveSpeed = 1f;

        affectedByGravity = 0f;
    }

    


    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room != null && firstChunk != null)
        {
            ChangeCollisionLayer(grabbedBy.Count == 0 ? 2 : 1);
            firstChunk.collideWithTerrain = grabbedBy.Count == 0;
            firstChunk.collideWithSlopes = grabbedBy.Count == 0;




            firstChunk.vel.y = firstChunk.vel.y - this.room.gravity * this.affectedByGravity;
            this.lastDirection = this.direction;
            this.lastLazyDirection = this.lazyDirection;
            this.lazyDirection = Vector3.Slerp(this.lazyDirection, this.direction, 0.06f);
            this.lastRotation = this.rotation;
            this.rotation += this.revolveSpeed;
            if (this.room.gravity * this.affectedByGravity > 0.5f)
            {
                if (base.firstChunk.ContactPoint.y < 0)
                {
                    this.direction = Vector3.Slerp(this.direction, new Vector2(Mathf.Sign(this.direction.x), 0f), 0.4f);
                    this.revolveSpeed *= 0.8f;
                }
                else if (this.grabbedBy.Count > 0)
                {
                    this.direction = Custom.PerpendicularVector(base.firstChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos) * (float)((this.grabbedBy[0].graspUsed == 0) ? -1 : 1);
                }
                else
                {
                    this.direction = Vector3.Slerp(this.direction, Custom.DirVec(base.firstChunk.lastLastPos, base.firstChunk.pos), 0.4f);
                }
                this.revolveSpeed *= 0.5f;
                this.rotation = Mathf.Lerp(this.rotation, Mathf.Floor(this.rotation) + 0.25f, Mathf.InverseLerp(0.5f, 1f, this.room.gravity * this.affectedByGravity) * 0.1f);
            }
            this.direction = new Vector2(0f, 1f);
            this.lazyDirection = this.direction;
            this.revolveSpeed += Mathf.Lerp(-1f, 1f, Random.value) * 1f / 120f;
            this.revolveSpeed = Mathf.Clamp(this.revolveSpeed, -0.025f, 0.025f) * 0.99f;

        }


        if (notInSameRoom)
        {
            notInSameRoomCounter++;
        }
        else
        {
            notInSameRoomCounter = 0;
        }

        if (justTeleported > 0)
        {
            justTeleported--;
        }

        AI?.Update();

        // 可以被b键拖拽
        // 所以他为什么那么喜欢卡在管道里。。
        if (room != null && room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
        {
            base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + this.room.game.cameras[0].pos) * 14f;
        }


        try
        {
            
            affectedByGravity = 0f;

            if (player.abstractCreature.pos.room != abstractCreature.pos.room || (room != null && player.room != null && !room.VisualContact(firstChunk.pos, player.mainBodyChunk.pos)))
            {
                cantSeeCounter++;
            }
            else
            {
                cantSeeCounter = 0;
            }


            // 如果和玩家在一个房间且能看见玩家，就直接移动
            // 否则按照pathfinder的路线行进
            // 其实这个移动逻辑差极了，全仰仗每个房间门口tp一次才能运行的
            if (!dead)
            {
                if (reachable && room != null && player.room != null && !notInSameRoom
                    // && Custom.DistLess(firstChunk.pos, player.mainBodyChunk.pos, 700f)
                    // && AI.VisualContact(player.abstractCreature.pos, 3f)
                    && AI != null && AI.pathFinder != null
                    && AI.pathFinder.CoordinateCost(player.abstractCreature.pos).legality < PathCost.Legality.Unwanted
                    && cantSeeCounter < 30
                    )
                {
                    HoverAtPlayerPos();
                }
                else
                {
                    MoveToDest();
                }
            }


        }
        catch (Exception ex)
        {
            Plugin.LogException(ex);
        }



    }

    public void MoveToDest()
    {
        try
        {

            if (grabbedBy.Count == 0 && AI.pathFinder != null && AI.pathFinder.destination != null && AI.pathFinder.DoneMappingAccessibility)
            {
                MovementConnection connection = (AI.pathFinder as StandardPather).FollowPath(abstractCreature.pos, true);
                currentConnection = connection;
                if (connection != null && connection.type == MovementConnection.MovementType.ShortCut)
                {
                    if ((room.GetTile(connection.StartTile).Terrain == Room.Tile.TerrainType.ShortcutEntrance))
                    {
                        enteringShortCut = new IntVector2?(connection.StartTile);
                    }
                }
                else if (connection != null)
                {
                    TryMoveTowards(room.MiddleOfTile(connection.DestTile), room.MiddleOfTile(AI.pathFinder.destination));
                }
            }
            else if (grabbedBy.Count == 0)
            {
                firstChunk.vel = Vector2.zero;
            }
        }
        catch (Exception ex)
        {
            Plugin.LogException(ex);
        }

    }

    public void TryMoveTowards(Vector2 connectionEnd, Vector2 dest)
    {
        // Plugin.Log("swarmer movetowards:", dest.x, dest.y);

        // 如果距离小于3f就不移动
        if (Custom.DistLess(firstChunk.pos, dest, 10f))
        {
            return;
        }

        if (AI.pathFinder.destination == null || Vector2.Distance(firstChunk.pos, dest) <= 35f)
        {
            bodyChunks[0].vel = Vector2.Lerp(bodyChunks[0].vel, Vector2.zero, 0.05f);
        }
        else
        {
            Vector2 dir1 = Custom.DirVec(firstChunk.pos, connectionEnd);
            // Vector2 dir2 = Custom.DirVec(firstChunk.pos, dest);
            float vel = Mathf.Lerp(moveSpeed, 5f, Custom.Dist(firstChunk.pos, connectionEnd) * 0.05f);
            bodyChunks[0].vel = dir1 * vel;
            bodyChunks[0].vel = Vector2.Lerp(bodyChunks[0].vel, dir1 * vel, 0.02f);
        }

    }





    public void HoverAtPlayerPos()
    {
        Vector2 atPlayerTop = Vector2.zero;
        if (player != null && player.room != null && !player.room.GetTile(player.firstChunk.pos + new Vector2(0f, 70f)).Solid)
        {
            atPlayerTop += new Vector2(0f, 70f);
        }

        Vector2 vector = (player.mainBodyChunk.pos - base.firstChunk.pos) / 100f;
        this.drift = (this.drift + Custom.RNV() * Random.value * 0.22f + vector * 0.03f).normalized;
        base.firstChunk.vel += this.drift * 0.05f;
        base.firstChunk.vel += vector * 0.025f;
        base.firstChunk.vel += vector.normalized * 0.05f;
        base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, player.firstChunk.pos + atPlayerTop) * Mathf.InverseLerp(25f, 550f, Vector2.Distance(base.firstChunk.pos, player.firstChunk.pos + atPlayerTop));
        base.firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 0.2f, 3f, 1f, 0.9f);

        firstChunk.vel += Vector2.up;
    }



    public void ForceIntoShortcut(IntVector2 entrancePos)
    {
        Plugin.Log("swarmer", abstractCreature.ID.number, "forced into shortcut:", entrancePos.ToString());
        SuckedIntoShortCut(entrancePos, true);
    }



    public void SpawnColorLerp()
    {
        base.RemoveGraphicsModule();
        Plugin.Log("swarmer color lerp:");
        graphicsModule = new MoonSwarmerGraphics(this, true);
        graphicsModule.Reset();
    }




    public override void RemoveGraphicsModule()
    {
        base.RemoveGraphicsModule();
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new MoonSwarmerGraphics(this, false);
        graphicsModule.Reset();
    }


    public override Color ShortCutColor()
    {
        return reachable? manager.player.ShortCutColor() : Color.white;
    }




    public override void Die()
    {
        // 懒得给o+8键写ilhook，于是想了这个招，只要我不执行这个函数不就好了吗（
        if (!reachable) return;
        if (manager.callBackSwarmers != null)
        {
            manager.callBackSwarmers++;
        }
        else
        {
            manager.callBackSwarmers = 1;
        }
    }




    public bool Kill(bool explode)
    {
        // if (abstractCreature == null) return false;
        if (explode && room != null)
        {
            room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 200f, 3f, 4, Color.white));
            for (int l = 0; l < 8; l++)
            {
                Vector2 vector2 = Custom.RNV();
                room.AddObject(new Spark(firstChunk.pos + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
            }
            if (room.Darkness(firstChunk.pos) > 0f)
            {
                room.AddObject(new LightSource(firstChunk.pos, false, Color.white, this));
            }
        }

        isActive = false;
        base.Die();
        // manager?.swarmers.Remove(this);

        /*stickToPlayer?.Deactivate();
        stickToPlayer = null;*/
        Room r = room;
        if (r != null)
        {
            r.RemoveObject(this);
            r.abstractRoom.RemoveEntity(abstractPhysicalObject);
        }
        Destroy();
        return true;
    }


    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
    }

    public override bool CanBeGrabbed(Creature grabber)
    {
        return grabber is Player;
    }

    public override void Stun(int st)
    {
    }

    public override void Blind(int blnd)
    {
    }

    public override void Destroy()
    {
        
        base.Destroy();
        
    }



}
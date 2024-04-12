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
public class MoonSwarmer : Creature
{

    public SwarmerManager manager;
    public MoonSwarmerAI AI = null;
    public bool isActive;

    public float rotation;
    public float lastRotation;
    public Vector2 direction;
    public Vector2 lastDirection;
    public Vector2 lazyDirection;
    public Vector2 lastLazyDirection;
    public float revolveSpeed;
    public bool lastVisible;


    public float affectedByGravity = 1f;


    public MoonSwarmer(AbstractCreature abstr) : base(abstr, abstr.world)
    {
        isActive = true;
        graphicsModule = new MoonSwarmerGraphics(this);

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

        affectedByGravity = 0f;
    }

    


    public override void Update(bool eu)
    {
        base.Update(eu);
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



        
        try
        {
            if (isActive && State.alive && graphicsModule == null)
            {
                graphicsModule = new MoonSwarmerGraphics(this);
            }
            AI?.Update();
            // 可以被b键拖拽
            // 所以他为什么那么喜欢卡在管道里。。
            if (room != null && room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            {
                base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + this.room.game.cameras[0].pos) * 14f;
            }

            if (manager != null)
            {
                affectedByGravity = Mathf.Lerp(affectedByGravity, (manager.player.dead || dead) ? 1f : 0f, 0.1f);
                Plugin.Log("swarmer affectedbyG", affectedByGravity);
            }

            

            if (room != null && room.gravity * affectedByGravity > 0.5f)
            {
                return;
            }

            if (!dead)
            {
                Act();
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex);
        }
        

        
    }

    public void Act()
    {
        if (grabbedBy != null && grabbedBy.Count == 0 &&  AI.pathFinder != null && AI.pathFinder.destination != null)
        {
            MovementConnection connection = (AI.pathFinder as StandardPather).FollowPath(abstractCreature.pos, true);
            if (connection != null && connection.type == MovementConnection.MovementType.ShortCut)
            {
                if ((room.GetTile(connection.StartTile).Terrain == Room.Tile.TerrainType.ShortcutEntrance))
                {
                    enteringShortCut = new IntVector2?(connection.StartTile);
                }
            }
            else if (connection != null)
            {
                MoveTowards(room.MiddleOfTile(connection.DestTile));
            }
        }
    }

    public void MoveTowards(Vector2 dest)
    {
        // Plugin.Log("swarmer movetowards:", dest.x, dest.y);
        Vector2 dir = Custom.DirVec(firstChunk.pos, dest);
        float vel = 4f;
        bodyChunks[0].vel = dir * vel;
    }




    public override Color ShortCutColor()
    {
        return Color.white;
    }

    public override void Die()
    {
        isActive = false;
        base.Die();
        
        manager?.swarmers.Remove(this);
        // AllGraspsLetGoOfThisObject(true);
        Room r = room;
        if (r != null)
        {
            Explode();
            
            r.RemoveObject(this);
            r.abstractRoom.RemoveEntity(abstractPhysicalObject);
        }
        Destroy();
    }

    // TODO: 有没有一种可能，如果玩家趁着神经元在钻管道的时候去世，他就会卡bug
    private void Explode()
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
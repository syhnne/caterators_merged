using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caterators_by_syhnne.fp.Daddy;
using RWCustom;
using UnityEngine;

namespace Caterators_by_syhnne.fp;

// 现在是2024年8月1日晚上17点51分，我在此隆重宣布：写完这玩意儿我就开摆
// 他妈的，两个月前说这个东西只是想口嗨来着，怎么就写起来了
// 他妈的。我给自己立了什么见鬼的flag。这可不是两三下就能完事的东西啊。这比月姐那个神经元还恐怖。


// 现在是2024年8月16日 21:44:56
// 我们的香菇终于初具猫形了
// 受不了了，一出门就被蜥蜴群殴，我要先改生物捕食关系

public class DaddyModule : IDrawable
{
    public WeakReference<Player> owner;
    public Player player { get {
            if (owner.TryGetTarget(out Player player)) { return player; }
            return null;
    }}
    public CustomDaddyTentacle[] tentacles;

    public int numberOfSprites;
    public int startSprite;

    public Vector2 playerInput = Vector2.zero;

    // eat
    private List<DaddyLongLegs.EatObject> eatObjects;
    public int digestingCounter;



    public bool NeededForLocomotion => (player.grasps[0] != null && player.HeavyCarry(player.grasps[0].grabbed)) || (player.grasps[1] != null && player.HeavyCarry(player.grasps[1].grabbed));
    public int TentaclesOnTerrain
    {
        get
        {
            int n = 0;
            foreach (var t in tentacles)
            {
                if (t.atGrabDest) n++;
            }
            return n;
        }
    }
    public int TentaclesOnSameDirection
    {
        get
        {
            int n = 0;
            foreach (var t in tentacles)
            {
                if (t.atGrabDest && Mathf.Abs(Custom.VecToDeg(t.direction) - Custom.VecToDeg(playerInput)) < 120f) n++;
            }
            return n;
        }
    }
    public float PlayerExtraMass
    {
        get
        {
            if (!NeededForLocomotion) return 0f;
            float f = 0.2f * TentaclesOnTerrain;
            Plugin.Log("player extra mass:", f);
            return f;
            
        }
    }

    

    public readonly Control controlOfPlayer = Control.NoTentacle;
    // 大概就是玩家在多大程度上能控制触手的行为
    public enum Control
    {
        NoTentacle, // 没启用
        Throw, // 可以通过投掷键让触手不再抓握生物，最高等级的控制
        Locomotion, // 玩家需要移动时可以借助触手提高移动能力
        Stun, // 玩家会经常晕倒，但拥有了新的进食方法
        CantWalk, // 失去双腿（
        NoControl // 只能控制玩家方向，失去正常蛞蝓猫的大部分能力

    }





    public DaddyModule(Player player, int cycle)
    {
        // 测试用，之后这个会根据目前的雨循环变化
        controlOfPlayer = Control.NoControl;

        this.owner = new(player);
        this.eatObjects = new List<DaddyLongLegs.EatObject>();

        if (controlOfPlayer == Control.NoTentacle) return; // 注意这句话，不要把关键事项放这个后面
        tentacles = new CustomDaddyTentacle[]
        {
            new(this, player.bodyChunks[1], 120f),
            new(this, player.bodyChunks[1], 90f),
            new(this, player.bodyChunks[1], 80f),
            new(this, player.bodyChunks[0], 100f),
        };
        foreach (var t in tentacles)
        {
            numberOfSprites += t.graphics.numberOfSprites;
        }

        
    }





    public void Update(bool eu)
    {
        if (controlOfPlayer == Control.NoTentacle) return;
        bool neededForLocomotion = NeededForLocomotion;

        foreach (var t in tentacles)
        {
            // 加个数量判定
            t.neededForLocomotion = t.canGrabTerrain && (neededForLocomotion || controlOfPlayer >= Control.CantWalk);

            t.Update();
            t.limp = !player.Consious;
            t.playerInput = playerInput;
        }



        switch (controlOfPlayer)
        {
            case Control.Throw:
                break;
            case Control.Locomotion:
                break;
            case Control.Stun:
                Eat(eu);
                break;
            case Control.CantWalk:
            case Control.NoControl:
                Eat(eu);

                // 好吧这个似乎不管用，我觉得下一步是劫持玩家的input让这个数只输入到这里，让玩家变成一只真正的香菇
                // 不行，这样进不了管道，而且真的巨tm搞笑。。。卧槽。。
                foreach (BodyChunk bodyChunk in player.bodyChunks)
                {
                    bodyChunk.vel.y += (player.gravity - player.buoyancy * bodyChunk.submersion) * 0.1f * TentaclesOnTerrain;
                    // 卧槽 笑崩溃了
                    // 特么的太超模了 直接飞檐走壁啊
                    // Plugin.Log("tentacles on same dir:", TentaclesOnSameDirection);
                    bodyChunk.vel += playerInput * 0.18f * TentaclesOnSameDirection;
                }
                break;
        }
    }

    public void NewRoom(Room room)
    {
        foreach (var t in tentacles)
        {
            t.NewRoom(room);
        }
            
    }

    public void LeaveRoom(Room oldRoom)
    {
        foreach(var t in tentacles)
        {
            t.LeaveRoom(oldRoom);
        }
    }


    public void Eat(bool eu)
    {
        // Vector2 middleOfBody = this.MiddleOfBody;
        for (int i = this.eatObjects.Count - 1; i >= 0; i--)
        {
            var obj = this.eatObjects[i];
            if (obj.progression > 1f)
            {
                if (obj.chunk.owner is Creature)
                {
                    this.digestingCounter = (int)Custom.LerpMap((obj.chunk.owner as Creature).Template.bodySize, 0.2f, 5f, 50f, 1100f);
                    Player player = obj.chunk.owner as Player;
                    player?.PermaDie();
                }
                obj.chunk.owner.Destroy();
                this.eatObjects.RemoveAt(i);
            }
            else
            {
                if (obj.chunk.owner.collisionLayer != 0)
                {
                    obj.chunk.owner.ChangeCollisionLayer(0);
                }
                if (ModManager.MMF && obj.chunk.owner is Creature)
                {
                    (obj.chunk.owner as Creature).enteringShortCut = null;
                }
                float progression = obj.progression;
                obj.progression += 0.0125f;
                if (progression <= 0.5f && obj.progression > 0.5f)
                {
                    if (obj.chunk.owner is Creature)
                    {
                        (obj.chunk.owner as Creature).Die();
                    }
                    for (int j = 0; j < obj.chunk.owner.bodyChunkConnections.Length; j++)
                    {
                        obj.chunk.owner.bodyChunkConnections[j].type = PhysicalObject.BodyChunkConnection.Type.Pull;
                    }
                }
                float num = obj.distance * (1f - obj.progression);
                obj.chunk.vel *= 0f;
                obj.chunk.MoveFromOutsideMyUpdate(eu, player.mainBodyChunk.pos + Custom.DirVec(player.mainBodyChunk.pos, obj.chunk.pos) * num);
                for (int k = 0; k < obj.chunk.owner.bodyChunks.Length; k++)
                {
                    obj.chunk.owner.bodyChunks[k].vel *= 1f - obj.progression;
                    obj.chunk.owner.bodyChunks[k].MoveFromOutsideMyUpdate(eu, Vector2.Lerp(obj.chunk.owner.bodyChunks[k].pos, player.mainBodyChunk.pos + Custom.DirVec(player.mainBodyChunk.pos, obj.chunk.owner.bodyChunks[k].pos) * num, obj.progression));
                }
                if (obj.chunk.owner.graphicsModule != null && obj.chunk.owner.graphicsModule.bodyParts != null)
                {
                    for (int l = 0; l < obj.chunk.owner.graphicsModule.bodyParts.Length; l++)
                    {
                        obj.chunk.owner.graphicsModule.bodyParts[l].vel *= 1f - obj.progression;
                        obj.chunk.owner.graphicsModule.bodyParts[l].pos = Vector2.Lerp(obj.chunk.owner.graphicsModule.bodyParts[l].pos, player.mainBodyChunk.pos, obj.progression);
                    }
                }
            }
        }
    }


    public bool CheckDaddyConsumption(PhysicalObject otherObject)
    {
        bool result = false;
        if (otherObject != null)
        {
            if (otherObject is DaddyLongLegs)
            {
                result = false;
            }
            else
            {
                result = (otherObject.TotalMass < 1.6f * player.TotalMass);
            }
        }
        return result;
    }



    public void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (player.room == null) return;
        if (CheckDaddyConsumption(otherObject))
        {
            bool grabbing = false;
            if (this.digestingCounter > 0)
            {
                return;
            }
            int num = 0;
            while (num < this.tentacles.Length && !grabbing)
            {
                if (this.tentacles[num].grabChunk != null && this.tentacles[num].grabChunk.owner == otherObject)
                {
                    grabbing = true;
                }
                num++;
            }
            int num2 = 0;
            while (num2 < this.eatObjects.Count && grabbing)
            {
                if (this.eatObjects[num2].chunk.owner == otherObject)
                {
                    grabbing = false;
                }
                num2++;
            }
            // 如果有抓住生物后吃掉 的输入条件，写在下面这一行
            if (grabbing)
            {
                if (player.graphicsModule != null)
                {
                    if (otherObject is IDrawable)
                    {
                        player.graphicsModule.AddObjectToInternalContainer(otherObject as IDrawable, 0);
                    }
                    else if (otherObject.graphicsModule != null)
                    {
                        player.graphicsModule.AddObjectToInternalContainer(otherObject.graphicsModule, 0);
                    }
                }
                this.eatObjects.Add(new DaddyLongLegs.EatObject(otherObject.bodyChunks[otherChunk], Vector2.Distance(player.bodyChunks[myChunk].pos, otherObject.bodyChunks[otherChunk].pos)));
                this.player.room.PlaySound(SoundID.Bro_Digestion_Init, player.bodyChunks[myChunk].pos);
            }
        }
    }



    #region graphics

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        int sprite = startSprite;
        foreach (var t in tentacles)
        {
            t.graphics.startSprite = sprite;
            sprite += t.graphics.numberOfSprites;
            t.graphics.InitiateSprites(sLeaser, rCam);
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        foreach (var t in tentacles)
        {
            t.graphics.DrawSprite(sLeaser, rCam, timeStacker, camPos);
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        foreach(var t in tentacles)
        {
            t.graphics.ApplyPalette(sLeaser, rCam, palette);
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
    }

    #endregion

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;

namespace Caterators_by_syhnne.fp.Daddy;








// 他妈的，太难绷了，猫死了之后尸体要是被捡走了，这一截触手会留在原地
// 但这不是重点……这得以后再修……（强忍
// 呃啊 而且玩家是可以被复活的。。要是复活了怎么整。。（瘫

// TODO: 需要写一个玩家钻管道时收回触手的代码，不然看着像瞬移
// TODO: 修复触手撞墙时抽搐的bug
// 好了我觉得这东西已经没问题了，剩下一切表现不如意的地方都是玩家那块导致的

public class CustomDaddyTentacle : Tentacle
{
    public DaddyModule daddy;
    
    public float tipRad = 1f;
    public float rootRad = 5f;
    public TentacleRopeGraphics graphics;
    public Tentacle.TentacleChunk firstChunk => tChunks[0];
    public Tentacle.TentacleChunk lastChunk => tChunks[tChunks.Count() - 1];
    public Vector2 direction => Tip.pos - connectedChunk.pos;

    // DLL features
    public BodyChunk grabChunk;
    public AbstractCreature huntCreature;
    public int stun = 0;
    public Task task = Task.Idle;
    public float sticky;
    public bool neededForLocomotion;
    public Vector2 huntDirection;
    public Vector2 lastSeenPos;
    public float length;

    private int lastSwitchTaskFrame = 0;
    public Vector2 idealGrabPos;
    public Vector2 tentacleDir;
    public int foundNoGrabPos;
    public IntVector2 secondaryGrabPos;
    private int secondaryGrabBackTrackCounter;
    public Vector2 preliminaryGrabDest;
    public int[] chunksStickSounds;
    public float chunksGripping;
    public bool atGrabDest;
    private bool lastBackTrack;
    public Vector2 playerInput = Vector2.zero;

    public bool canGrabTerrain;

    // physics
    private IntVector2[] _cachedRays1 = new IntVector2[200];

    public CustomDaddyTentacle(DaddyModule owner, BodyChunk connectedChunk, float length) : base(owner.player, connectedChunk, length)
    {
        canGrabTerrain = length > 75f;
        this.length = length;
        daddy = owner;
        limp = false;
        neededForLocomotion = true;
        tProps = new Tentacle.TentacleProps(true, true, false, 0.5f, 0.1f, 0f, 0f, 0f, 3.2f, 10f, 0.25f, 5f, 15, 60, 12, 20);
        scratchPath = new();
        tChunks = new Tentacle.TentacleChunk[Mathf.RoundToInt(length) / 20];
        for (int i = 0; i < tChunks.Length; i++)
        {
            tChunks[i] = new Tentacle.TentacleChunk(this, i, (float)(i + 1) / (float)tChunks.Length, Mathf.Lerp(this.rootRad, this.tipRad, (float)i / (float)(tChunks.Length - 1)))
            {
                collideWithTerrain = true,
            };
        }
        stretchAndSqueeze = 0.1f;
        this.chunksStickSounds = new int[this.tChunks.Length];

        this.graphics = new(this);
        this.graphics.Reset(this.firstChunk.pos);
    }

    





    public override void Update()
    {
        int exceptionStage = 0;
        base.Update();
        graphics.Update();
        if (this.grabChunk != null && (this.daddy.player.room == null || this.daddy.player.room != this.grabChunk.owner.room))
        {
            this.stun = 10;
            ReleaseGrasp(false);
        }
        if (this.daddy.player.dead)
        {
            neededForLocomotion = true;
            ReleaseGrasp(false);
            this.limp = true;
        }
        if (this.stun > 0)
        {
            this.stun--;
            this.grabChunk = null;
        }
        if (daddy.player != null && !daddy.player.IsGrabbingAnything() && daddy.player.input[0].thrw)
        {
            ReleaseGrasp(true);
        }
        if (grabChunk != null && grabChunk.owner != null && grabChunk.owner is Creature && daddy.player != null && !WantToGrabThisCreature(grabChunk.owner as Creature))
        {
            ReleaseGrasp(true);
        }

        exceptionStage = 1;

        if (this.grabChunk != null && task == Task.Grabbing)
        {
            // Plugin.Log("grabbing");
            float chunkCenterDist = Vector2.Distance(base.Tip.pos, this.grabChunk.pos);
            float totalRad = (base.Tip.rad + this.grabChunk.rad) / 4f;
            Vector2 a = Custom.DirVec(base.Tip.pos, this.grabChunk.pos);
            float grabSpeed = this.grabChunk.mass / (this.grabChunk.mass + 0.01f);
            float d = (connectedChunk.owner.TotalMass / (connectedChunk.owner.TotalMass + grabChunk.owner.TotalMass)) * 2f;
            base.Tip.pos += a * (chunkCenterDist - totalRad) * grabSpeed * d;
            base.Tip.vel += a * (chunkCenterDist - totalRad) * grabSpeed * d;
            this.grabChunk.pos -= a * (chunkCenterDist - totalRad) * (1f - grabSpeed) * d;
            this.grabChunk.vel -= a * (chunkCenterDist - totalRad) * (1f - grabSpeed) * d;
            // Plugin.Log(d);
            // firstChunk.pos += a * (chunkCenterDist - totalRad) * grabSpeed * (1f - d);
            // firstChunk.vel += a * (chunkCenterDist - totalRad) * grabSpeed * (1f - d);

            // 触手对玩家也有相应作用力
            if (grabChunk.owner is not IPlayerEdible)
            {
                connectedChunk.pos += a * (chunkCenterDist - totalRad) * (1f - grabSpeed) * (2.3f - d);
                connectedChunk.vel += a * (chunkCenterDist - totalRad) * (1f - grabSpeed) * (2.3f - d);
            }
        }
        

        exceptionStage = 2;
        this.limp = (!this.daddy.player.Consious || this.stun > 0);
        for (int i = 0; i < this.tChunks.Length; i++)
        {
            this.tChunks[i].vel *= 0.9f;
            if (this.limp)
            {
                Tentacle.TentacleChunk tentacleChunk = this.tChunks[i];
                tentacleChunk.vel.y -= 0.5f;
            }
            if (this.stun > 0 && !this.daddy.player.dead)
            {
                this.tChunks[i].vel += Custom.RNV() * 10f;
            }
        }
        if (this.limp)
        {
            for (int j = 0; j < this.tChunks.Length; j++)
            {
                Tentacle.TentacleChunk tentacleChunk2 = this.tChunks[j];
                tentacleChunk2.vel.y -= 0.7f;
            }
            return;
        }
        this.atGrabDest = false;
        if (canGrabTerrain)
        {
            if (this.backtrackFrom > -1)
            {
                this.secondaryGrabBackTrackCounter++;
                if (!this.lastBackTrack)
                {
                    this.secondaryGrabBackTrackCounter += 20;
                }
            }
            this.lastBackTrack = (this.backtrackFrom > -1);
        }

        exceptionStage = 3;

        this.chunksGripping = 0f;
        if (!this.neededForLocomotion)
        {
            if (this.task != Task.Grabbing)
            {
                this.LookForCreaturesToHunt();
            }
        }
        else if (this.task != Task.Locomotion)
        {
            this.SwitchTask(Task.Locomotion);
        }


        if (this.task == Task.Hunt && (this.huntCreature == null || this.huntCreature.slatedForDeletion))
        {
            this.SwitchTask(Task.Idle);
        }
        else if (this.task != Task.Hunt && this.huntCreature != null)
        {
            this.huntCreature = null;
        }
        if (this.task == Task.Grabbing && (this.grabChunk == null || this.grabChunk.owner.room != this.room || (ModManager.MMF && !this.daddy.player.Consious)))
        {
            this.SwitchTask(Task.Idle);
        }
        else if (this.task != Task.Grabbing && this.grabChunk != null)
        {
            this.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, this.grabChunk.pos);
            this.grabChunk = null;
        }

        // 离玩家太远会松手。我懒得写其他判断逻辑，留给下一帧来考虑吧（
        // 啊？不要告诉我你的degtovec是角度制
        // 我超，还真是角度制
        if (room != null && atGrabDest && grabDest != null && (!Custom.DistLess(connectedChunk.pos, room.MiddleOfTile((IntVector2)grabDest), length + 5f) || Mathf.Abs(Custom.VecToDeg(room.MiddleOfTile((IntVector2)grabDest) - connectedChunk.pos) - Custom.VecToDeg(playerInput)) > 100f))
        {
            SwitchTask(Task.Idle);
        }

        exceptionStage = 4;
        /*if (this.task == DaddyTentacle.Task.Idle)
        {
            this.Climb(ref this.scratchPath);
        }
        else if (this.task == DaddyTentacle.Task.Hunt)
        {
            this.Hunt(ref this.scratchPath);
        }
        else if (this.task == DaddyTentacle.Task.ExamineSound)
        {
            this.ExamineSound(ref this.scratchPath);
        }*/
        switch (task)
        {
            case Task.Locomotion:
                Climb(ref this.scratchPath);
                break;
            case Task.Idle:
                break;
            case Task.Hunt:
                Hunt(ref this.scratchPath);
                break;
            case Task.Grabbing:
                base.MoveGrabDest(connectedChunk.pos + Custom.DirVec(connectedChunk.pos, this.grabChunk.pos) * 20f, ref this.scratchPath);
                Vector2 p = connectedChunk.pos;
                bool flag2 = this.room.VisualContact(this.grabChunk.pos, connectedChunk.pos);
                for (int l = this.tChunks.Length - 1; l >= 0; l--)
                {
                    Vector2 p2 = base.FloatBase;
                    if (l > 0)
                    {
                        p2 = this.tChunks[l - 1].pos;
                        if (!flag2 && !this.room.VisualContact(this.grabChunk.pos, this.tChunks[l - 1].pos))
                        {
                            p = this.tChunks[l].pos;
                            flag2 = true;
                        }
                    }
                    this.tChunks[l].vel += Custom.DirVec(this.tChunks[l].pos, p2) * 1.2f;
                    if (this.tChunks[l].phase > -1f || (grabChunk.owner is not IPlayerEdible && this.room.GetTile(this.tChunks[l].pos).Solid))
                    {
                        //  || this.room.GetTile(this.tChunks[l].pos).Solid
                        // 懂了，主要是注释这句话导致的松手，不然它基本上是不会松开的
                        Plugin.Log("too far release");
                        ReleaseGrasp(true);
                        break;
                    }
                }
                if (this.task == Task.Grabbing)
                {
                    // 死去的数学突然开始攻击我
                    Vector2 vec = Vector3.Slerp(Custom.DirVec(this.grabChunk.pos, p), Custom.DirVec(base.Tip.pos, this.tChunks[this.tChunks.Length - 2].pos), 0.5f) * Custom.LerpMap((float)this.grabPath.Count, 3f, 18f, 0.65f, 0.25f) * 0.45f / Mathf.Max(0.5f * (this.grabChunk.mass - 1f) + 1f, grabChunk.mass);
                    this.grabChunk.vel += vec;
                }
                break;
        }
        exceptionStage = 5;
        Touch();
    }


    #region Hunt

    // 不是，这么一个没几句话的小破函数到底是哪个位置能给我卡这么多bug
    public void ReleaseGrasp(bool playSound)
    {
        if (grabChunk == null) return;
        Plugin.Log("tentacle release grasp:");
        grabChunk ??= null;
        if (playSound && room != null) room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, grabChunk.pos);
        SwitchTask(Task.Idle);
    }


    // 好小子，每个香菇的每只触手在每一帧都要跑一遍这个三重循环？
    // 属于是解除了关于【我的代码会不会拖慢游戏运行速度】的担心

    // 奶奶的，手都摸上了，你倒是抓啊
    private void Touch()
    {
        if (daddy.player.room == null) return;

        bool flag = false;
        var creatures = daddy.player.room.abstractRoom.creatures;

        for (int i = 0; i < creatures.Count; i++)
        {
            if (creatures[i].realizedCreature == null || creatures[i].realizedCreature.inShortcut || creatures[i].realizedCreature == this.daddy.player || creatures[i].tentacleImmune) continue;

            Creature realizedCreature = creatures[i].realizedCreature;
            for (int j = 0; j < this.tChunks.Length; j++)
            {
                int k = 0;
                while (k < realizedCreature.bodyChunks.Length)
                {
                    // i - 当前生物 j - 触手体块 k - 生物体块

                    // 意思就是边缘有接触
                    // 给他留点误差
                    // 不留了，容易bug
                    if (Custom.DistLess(this.tChunks[j].pos, realizedCreature.bodyChunks[k].pos, this.tChunks[j].rad + realizedCreature.bodyChunks[k].rad))
                    {
                        if (realizedCreature.abstractCreature.creatureTemplate.AI && realizedCreature.abstractCreature.abstractAI.RealAI != null && realizedCreature.abstractCreature.abstractAI.RealAI.tracker != null)
                        {
                            realizedCreature.abstractCreature.abstractAI.RealAI.tracker.SeeCreature(this.daddy.player.abstractCreature);
                        }
                        this.CollideWithCreature(j, realizedCreature.bodyChunks[k]);
                        if (!neededForLocomotion && task != Task.Grabbing && realizedCreature.newToRoomInvinsibility < 1 && this.grabChunk == null && j == this.tChunks.Length - 1 && (realizedCreature.State.meatLeft > 0 || realizedCreature is IPlayerEdible))
                        {
                            flag = true;
                            if (realizedCreature is not IPlayerEdible && Vector2.Distance(this.tChunks[j].vel, realizedCreature.bodyChunks[k].vel) >= Mathf.Lerp(1f, 8f, this.sticky))
                            {
                                break;
                            }
                            bool flag3 = WantToGrabThisCreature(realizedCreature);
                            for (int l = 0; l < tChunks.Length; l++)
                            {
                                if (this.tChunks[l].phase > -1f || this.room.GetTile(this.tChunks[l].pos).Solid)
                                {
                                    flag3 = false;
                                }
                            }
                            if (flag3)
                            {
                                Plugin.Log("tentacle grab creature:", realizedCreature);
                                this.grabChunk = realizedCreature.bodyChunks[k];
                                this.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Creature, this.tChunks[j].pos, 1f, 1f);
                                this.SwitchTask(Task.Grabbing);
                                return;
                            }
                            break;
                        }
                        else
                        {
                            if (neededForLocomotion || task == Task.Hunt || task == Task.Grabbing || this.IsCreatureCaughtEnough(realizedCreature.abstractCreature))
                            {
                                break;
                            }
                            bool flag4 = false;
                            int cnt = 0;
                            while (cnt < this.daddy.tentacles.Length && !flag4)
                            {
                                if (this.daddy.tentacles[cnt].huntCreature == realizedCreature.abstractCreature)
                                {
                                    flag4 = true;
                                }
                                cnt++;
                            }
                            if (!flag4)
                            {
                                this.huntCreature = realizedCreature.abstractCreature;
                                this.SwitchTask(Task.Hunt);
                                break;
                            }
                            break;
                        }
                    }
                    else
                    {
                        k++;
                    }
                }
            }
        }
        if (flag)
        {
            this.sticky = Mathf.Min(1f, this.sticky + 0.033333335f);
            return;
        }
        this.sticky = Mathf.Max(0f, this.sticky - 0.016666668f);
    }



    // （搞定，最后还是使用了地毯式搜索……）修这里的bug，目前不知道原因，但我真懒得地毯式搜查了，累得慌
    private void LookForCreaturesToHunt()
    {
        if (this.neededForLocomotion || daddy.player == null || daddy.player.room == null || room == null)
        {
            return;
        }
        AbstractCreature targetCreature = null;
        /*huntDirection = Vector2.zero;
        if (this.huntDirection == Vector2.zero)
        {
            this.huntDirection = Custom.RNV() * 80f;
        }
        if (this.daddy.player.input[0].AnyDirectionalInput)
        {
            this.huntDirection = new Vector2((float)daddy.player.input[0].x, (float)daddy.player.input[0].y) * 80f;
        }*/

        try
        {
            if (room == null) return;
            Creature crit = null;
            float num = float.MaxValue;
            // float current = Custom.VecToDeg(this.huntDirection);
            var creatures = room.abstractRoom.creatures;
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].realizedCreature != null && WantToGrabThisCreature(creatures[i].realizedCreature))
                {
                    // float degree = Custom.AimFromOneVectorToAnother(this.connectedChunk.pos, creatures[i].realizedCreature.mainBodyChunk.pos);
                    float distance = Custom.Dist(this.connectedChunk.pos, creatures[i].realizedCreature.mainBodyChunk.pos);
                    // Mathf.Abs(Mathf.DeltaAngle(current, degree)) < 22.5f && 
                    if (distance < num)
                    {
                        num = distance;
                        crit = creatures[i].realizedCreature;
                    }
                }
            }
            if (crit != null)
            {
                targetCreature = crit.abstractCreature;
            }
            else
            {
                return;
            }
            for (int j = 0; j < this.daddy.tentacles.Length; j++)
            {
                if (this.daddy.tentacles[j].huntCreature == targetCreature)
                {
                    return;
                }
            }
            /*if (this.IsCreatureCaughtEnough(targetCreature))
            {
                return;
            }*/
            if (targetCreature.pos.room != this.daddy.player.abstractCreature.pos.room)
            {
                return;
            }
            if (Vector2.Distance(this.room.MiddleOfTile(targetCreature.pos), base.FloatBase) > this.idealLength + 10f)
            {
                return;
            }
            this.huntCreature = targetCreature;
            this.SwitchTask(Task.Hunt);
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
        }
    }



    public bool WantToGrabThisCreature(Creature crit)
    {
        if (crit is Player) { /*Plugin.Log("crit is player")*/; return false; } 
        if (daddy.player != null)
        {
            if (daddy.player.FoodInStomach >= daddy.player.MaxFoodInStomach) { /*Plugin.Log("enough food");*/ return false; }
            if (crit.IsGrabbedBy(daddy.player)) { /*Plugin.Log("is grabbed");*/ return false; }
            if (crit is not IPlayerEdible && crit.State.meatLeft <= 0) { /*Plugin.Log("crit.State.meatLeft <= 0");*/ return false; }
            if (crit is IPlayerEdible)
            {
                foreach (var t in daddy.tentacles)
                {
                    if (t.grabChunk != null && t.grabChunk.owner == crit) { /*Plugin.Log("is grabbed by other tentacles");*/ return false; }
                }
            }
            // TODO: 给秃鹫面具加个额外判定，因为那玩意扔不出去）？
            if (!daddy.player.IsGrabbingAnything() && daddy.player.input[0].thrw) { /*Plugin.Log("throw");*/ return false; }
        }
        return (crit is IPlayerEdible || crit.State.meatLeft > 0);
    }



    private void CollideWithCreature(int tChunk, BodyChunk creatureChunk)
    {
        if (this.backtrackFrom > -1 && this.backtrackFrom <= tChunk)
        {
            return;
        }
        float num = Vector2.Distance(this.tChunks[tChunk].pos, creatureChunk.pos);
        float num2 = (this.tChunks[tChunk].rad + creatureChunk.rad) / 4f;
        Vector2 a = Custom.DirVec(this.tChunks[tChunk].pos, creatureChunk.pos);
        float num3 = creatureChunk.mass / (creatureChunk.mass + 0.01f);
        float d = 0.8f;
        this.tChunks[tChunk].pos += a * (num - num2) * num3 * d;
        this.tChunks[tChunk].vel += a * (num - num2) * num3 * d;
        creatureChunk.pos -= a * (num - num2) * (1f - num3) * d;
        creatureChunk.vel -= a * (num - num2) * (1f - num3) * d;
    }



    private bool IsCreatureCaughtEnough(AbstractCreature crit)
    {
        int num = 0;
        try
        {
            for (int i = 0; i < this.daddy.tentacles.Length; i++)
            {
                if (this.daddy.tentacles[i].grabChunk != null && this.daddy.tentacles[i].grabChunk.owner is Creature && (this.daddy.tentacles[i].grabChunk.owner as Creature).abstractCreature == crit)
                {
                    num++;
                }
            }
            // 呃，好吧，抓住一个bodysize = 1的生物需要三只触手，但拢共就只有两只。。
            return (float)num >= crit.creatureTemplate.bodySize * 1.5f;
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
            return false;
        }
    }




    private void Hunt(ref List<IntVector2> path)
    {
        if (this.huntCreature.pos.room != this.daddy.player.abstractCreature.pos.room)
        {
            this.SwitchTask(Task.Idle);
            return;
        }
        else if (huntCreature.realizedCreature != null && (!room.VisualContact(huntCreature.realizedCreature.mainBodyChunk.pos, connectedChunk.pos) || !Custom.DistLess(huntCreature.realizedCreature.DangerPos, connectedChunk.pos, length + 100f)))
        {
            this.SwitchTask(Task.Idle);
            return;
        }
        else if (huntCreature.realizedCreature != null)
        {
            lastSeenPos = huntCreature.realizedCreature.mainBodyChunk.pos;
            if (!WantToGrabThisCreature(huntCreature.realizedCreature))
            {
                this.SwitchTask(Task.Idle);
                return;
            }
        }
        base.MoveGrabDest(lastSeenPos, ref path);

        if ((float)this.grabPath.Count * 20f > this.idealLength || this.neededForLocomotion)
        {
            float chunkCenterDist = float.MaxValue;
            int distance = -1;
            for (int k = 0; k < this.daddy.tentacles.Length; k++)
            {
                if (this.daddy.tentacles[k].task == Task.Idle && !this.daddy.tentacles[k].neededForLocomotion && (this.daddy.tentacles[k].idealLength > this.idealLength || this.neededForLocomotion) && !this.daddy.tentacles[k].atGrabDest && Mathf.Abs(this.daddy.tentacles[k].idealLength - (float)this.grabPath.Count * 20f) < chunkCenterDist)
                {
                    chunkCenterDist = Mathf.Abs(this.daddy.tentacles[k].idealLength - (float)this.grabPath.Count * 20f);
                    distance = k;
                }
            }
            if (distance > -1)
            {
                this.daddy.tentacles[distance].huntCreature = this.huntCreature;
                this.daddy.tentacles[distance].task = Task.Hunt;
                this.huntCreature = null;
                this.UpdateClimbGrabPos(ref path);
                return;
            }
        }
        /*if (Vector2.Distance(this.room.MiddleOfTile(this.huntCreature.BestGuessForPosition()), base.FloatBase) > this.idealLength * 1.5f)
        {
            this.huntCreature = null;
            this.UpdateClimbGrabPos(ref path);
            return;
        }*/
        for (int l = 0; l < this.tChunks.Length; l++)
        {
            if (this.backtrackFrom == -1 || this.backtrackFrom > l)
            {
                if (base.grabDest != null && this.room.VisualContact(this.tChunks[l].pos, this.floatGrabDest.Value))
                {
                    this.tChunks[l].vel += Vector2.ClampMagnitude(this.floatGrabDest.Value - this.tChunks[l].pos, 20f) * (1f / daddy.player.TotalMass) / 28f;
                }
                else
                {
                    this.tChunks[l].vel += Vector2.ClampMagnitude(this.room.MiddleOfTile(this.segments[this.tChunks[l].currentSegment]) - this.tChunks[l].pos, 20f) * (1f / daddy.player.TotalMass) / 28f ;
                }
            }
        }
    }

    #endregion



    #region Climb


    private void Climb(ref List<IntVector2> path)
    {
        if (daddy.player == null) { return; }
        Vector2 moveDir = daddy.player.touchedNoInputCounter > 0? new Vector2(connectedChunk.vel.x, connectedChunk.vel.y) : playerInput;
        // 如果玩家在地面上，触手会倾向于往高处抓
        // 这主要是为了视觉效果（
        if (daddy.player.IsTileSolid(1, 0, -1))
        {
            moveDir.y += 20f;
        }

        // 注意到safariControlled状态下stuckCounter一直为0，所以直接写0f（
        // 等会，这特么是个定值。。算了有空再把结果写上去

        // 啊？tentacledir是啥
        // 懂了，那个是根据蓝香菇总的触手数量来定的，大概意思就是触手在核心周围均匀分布，互相成的角度相同
        // 这个报错是不是vs的一个bug，当我有没声明的变量的时候他只报那个错，等我改完了他又告诉我vector3的函数不能用
        // 只好自己写一个凑活凑活了。。一会逝逝就知道效果是否一样
        this.idealGrabPos = base.FloatBase + sCustom.SlerpVec(this.tentacleDir, moveDir, 0.8f) * this.idealLength * 0.7f;
        // 这个foundnograbpos是个计时器，给他加上就好了
        Vector2 vector = base.FloatBase + sCustom.SlerpVec(Vector3.Slerp(this.tentacleDir, moveDir, 0.8f), Custom.RNV(), Mathf.InverseLerp(20f, 200f, (float)this.foundNoGrabPos)) * this.idealLength * Custom.LerpMap((float)Math.Max(this.foundNoGrabPos, 0f), 20f, 200f, 0.7f, 1.2f);
        // 不知道这是啥写法，不敢动（
        int i;
        for (i = SharedPhysics.RayTracedTilesArray(base.FloatBase, vector, this._cachedRays1); i >= this._cachedRays1.Length; i = SharedPhysics.RayTracedTilesArray(base.FloatBase, vector, this._cachedRays1))
        {
            Plugin.Log(string.Format("DaddyTentacle Climb ray tracing limit exceeded, extending cache to {0} and trying again!", this._cachedRays1.Length + 100));
            Array.Resize<IntVector2>(ref this._cachedRays1, this._cachedRays1.Length + 100);
        }
        bool foundGrabPos = false;
        for (int j = 0; j < i - 1; j++)
        {
            if (this.room.GetTile(this._cachedRays1[j + 1]).IsSolid())
            {
                this.ConsiderGrabPos(Custom.RestrictInRect(vector, this.room.TileRect(this._cachedRays1[j]).Shrink(1f)), this.idealGrabPos);
                foundGrabPos = true;
                break;
            }
            if (this.room.GetTile(this._cachedRays1[j]).horizontalBeam || this.room.GetTile(this._cachedRays1[j]).verticalBeam)
            {
                this.ConsiderGrabPos(this.room.MiddleOfTile(this._cachedRays1[j]), this.idealGrabPos);
                foundGrabPos = true;
            }
        }
        if (foundGrabPos)
        {
            this.foundNoGrabPos = 0;
        }
        else
        {
            this.foundNoGrabPos++;
        }
        bool flag2 = this.secondaryGrabBackTrackCounter < 200 && this.SecondaryGrabPosScore(this.secondaryGrabPos) > 0f;
        for (int k = 0; k < this.tChunks.Length; k++)
        {
            if (this.backtrackFrom == -1 || this.backtrackFrom > k)
            {
                this.StickToTerrain(this.tChunks[k]);
                if (base.grabDest != null)
                {
                    if (!this.atGrabDest && Custom.DistLess(this.tChunks[k].pos, this.floatGrabDest.Value, 20f))
                    {
                        this.atGrabDest = true;
                    }
                    if (this.tChunks[k].currentSegment <= this.grabPath.Count || !flag2)
                    {
                        this.tChunks[k].vel += Vector2.ClampMagnitude(this.floatGrabDest.Value - this.tChunks[k].pos, 20f) / 20f * 1.5f;
                    }
                    else if (k > 1 && this.segments.Count > this.grabPath.Count && flag2)
                    {
                        float num2 = Mathf.InverseLerp((float)this.grabPath.Count, (float)this.segments.Count, (float)this.tChunks[k].currentSegment);
                        Vector2 vector2 = Custom.DirVec(this.tChunks[k - 2].pos, this.tChunks[k].pos) * (1f - num2) * 0.6f;
                        vector2 += Custom.DirVec(this.tChunks[k].pos, this.room.MiddleOfTile(base.grabDest.Value)) * Mathf.Pow(1f - num2, 4f) * 2f;
                        vector2 += Custom.DirVec(this.tChunks[k].pos, this.room.MiddleOfTile(this.secondaryGrabPos)) * Mathf.Pow(num2, 4f) * 2f;
                        vector2 += Custom.DirVec(this.tChunks[k].pos, base.FloatBase) * Mathf.Sin(num2 * 3.1415927f) * 0.3f;
                        this.tChunks[k].vel += vector2.normalized * 1.2f;
                        if (k == this.tChunks.Length - 1)
                        {
                            this.tChunks[k].vel += Vector2.ClampMagnitude(this.room.MiddleOfTile(this.secondaryGrabPos) - this.tChunks[k].pos, 20f) / 20f * 5.2f;
                        }
                    }
                }
            }
        }
        if (base.grabDest != null)
        {
            this.ConsiderSecondaryGrabPos(base.grabDest.Value + new IntVector2(Random.Range(-20, 21), Random.Range(-20, 21)));
        }
        if (base.grabDest == null || !this.atGrabDest)
        {
            this.UpdateClimbGrabPos(ref path);
        }
    }



    public void UpdateClimbGrabPos(ref List<IntVector2> path)
    {
        if (this.huntCreature != null)
        {
            return;
        }
        base.MoveGrabDest(this.preliminaryGrabDest, ref path);
    }



    private float GrabPosScore(Vector2 testPos, Vector2 idealGrabPos)
    {
        float num = 100f / Vector2.Distance(testPos, idealGrabPos);
        if (base.grabDest != null && this.room.GetTilePosition(testPos) == base.grabDest.Value)
        {
            // 这个数大概是决定了它有时不想换位置
            num *= 1.2f;
        }
        for (int i = 0; i < 4; i++)
        {
            if (this.room.GetTile(testPos + Custom.fourDirections[i].ToVector2() * 20f).Solid)
            {
                num *= 2f;
                break;
            }
        }
        return num;
    }



    private void ConsiderGrabPos(Vector2 testPos, Vector2 idealGrabPos)
    {
        if (this.GrabPosScore(testPos, idealGrabPos) > this.GrabPosScore(this.preliminaryGrabDest, idealGrabPos))
        {
            this.preliminaryGrabDest = testPos;
        }
    }



    private float SecondaryGrabPosScore(IntVector2 testPos)
    {
        if (base.grabDest == null)
        {
            return 0f;
        }
        if (testPos.FloatDist(base.BasePos) < 7f)
        {
            return 0f;
        }
        float num = this.idealLength - (float)this.grabPath.Count * 20f;
        if (Vector2.Distance(this.room.MiddleOfTile(testPos), this.floatGrabDest.Value) > num)
        {
            return 0f;
        }
        if (!SharedPhysics.RayTraceTilesForTerrain(this.room, base.grabDest.Value, testPos))
        {
            return 0f;
        }
        float num2 = 0f;
        for (int i = 0; i < 8; i++)
        {
            if (this.room.GetTile(testPos + Custom.eightDirections[i]).Solid)
            {
                num2 += 1f;
            }
        }
        if (this.room.GetTile(testPos).horizontalBeam || this.room.GetTile(testPos).verticalBeam)
        {
            num2 += 1f;
        }
        if (num2 > 0f && testPos == this.secondaryGrabPos)
        {
            num2 += 1f;
        }
        if (num2 == 0f)
        {
            return 0f;
        }
        num2 += testPos.FloatDist(base.BasePos) / 10f;
        return num2 / (1f + Mathf.Abs(num * 0.75f - Vector2.Distance(this.room.MiddleOfTile(testPos), this.floatGrabDest.Value)) + Vector2.Distance(this.room.MiddleOfTile(testPos), this.room.MiddleOfTile(this.segments[this.segments.Count - 1])));
    }



    private void ConsiderSecondaryGrabPos(IntVector2 testPos)
    {
        if (this.room.GetTile(testPos).Solid)
        {
            return;
        }
        if (this.SecondaryGrabPosScore(testPos) > this.SecondaryGrabPosScore(this.secondaryGrabPos))
        {
            this.secondaryGrabBackTrackCounter = 0;
            this.secondaryGrabPos = testPos;
        }
    }




    private void StickToTerrain(Tentacle.TentacleChunk chunk)
    {
        if (this.floatGrabDest != null && !Custom.DistLess(chunk.pos, this.floatGrabDest.Value, 200f))
        {
            return;
        }
        int num = (int)Mathf.Sign(chunk.pos.x - this.room.MiddleOfTile(chunk.pos).x);
        Vector2 vector = new(0f, 0f);
        IntVector2 tilePosition = this.room.GetTilePosition(chunk.pos);
        int i = 0;
        while (i < 8)
        {
            if (this.room.GetTile(tilePosition + new IntVector2(Custom.eightDirectionsDiagonalsLast[i].x * num, Custom.eightDirectionsDiagonalsLast[i].y)).Solid)
            {
                if (Custom.eightDirectionsDiagonalsLast[i].x != 0)
                {
                    vector.x = this.room.MiddleOfTile(chunk.pos).x + (float)(Custom.eightDirectionsDiagonalsLast[i].x * num) * (20f - chunk.rad);
                }
                if (Custom.eightDirectionsDiagonalsLast[i].y != 0)
                {
                    vector.y = this.room.MiddleOfTile(chunk.pos).y + (float)Custom.eightDirectionsDiagonalsLast[i].y * (20f - chunk.rad);
                    break;
                }
                break;
            }
            else
            {
                i++;
            }
        }
        if (vector.x == 0f && this.room.GetTile(chunk.pos).verticalBeam)
        {
            vector.x = this.room.MiddleOfTile(chunk.pos).x;
        }
        if (vector.y == 0f && this.room.GetTile(chunk.pos).horizontalBeam)
        {
            vector.y = this.room.MiddleOfTile(chunk.pos).y;
        }
        if (chunk.tentacleIndex > this.tChunks.Length / 2)
        {
            if (vector.x != 0f || vector.y != 0f)
            {
                if (this.chunksStickSounds[chunk.tentacleIndex] > 10)
                {
                    this.owner.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Terrain, chunk.pos, Mathf.InverseLerp((float)(this.tChunks.Length / 2), (float)(this.tChunks.Length - 1), (float)chunk.tentacleIndex), 1f);
                }
                if (this.chunksStickSounds[chunk.tentacleIndex] > 0)
                {
                    this.chunksStickSounds[chunk.tentacleIndex] = 0;
                }
                else
                {
                    this.chunksStickSounds[chunk.tentacleIndex]--;
                }
            }
            else
            {
                if (this.chunksStickSounds[chunk.tentacleIndex] < -10)
                {
                    this.owner.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Terrain, chunk.pos, Mathf.InverseLerp((float)(this.tChunks.Length / 2), (float)(this.tChunks.Length - 1), (float)chunk.tentacleIndex), 1f);
                }
                if (this.chunksStickSounds[chunk.tentacleIndex] < 0)
                {
                    this.chunksStickSounds[chunk.tentacleIndex] = 0;
                }
                else
                {
                    this.chunksStickSounds[chunk.tentacleIndex]++;
                }
            }
        }
        if (vector.x != 0f)
        {
            chunk.vel.x += (vector.x - chunk.pos.x) * 0.1f;
            chunk.vel.y *= 0.9f;
        }
        if (vector.y != 0f)
        {
            chunk.vel.y += (vector.y - chunk.pos.y) * 0.1f;
            chunk.vel.x *= 0.9f;
        }
        if (vector.x != 0f || vector.y != 0f)
        {
            this.chunksGripping += 1f / (float)this.tChunks.Length;
        }
    }



    // 啊？这是什么？怎么我之前没发现？
    // 好小子原来他自己不调用，在daddylonglegs那里调用 怪不得我没看见
    // 怪不得手感这么差。。我还要自己动脑子想怎么让他在合适的时候松手。。
    public float ReleaseScore()
    {
        float num = float.MaxValue;
        for (int i = this.tChunks.Length / 2; i < this.tChunks.Length; i++)
        {
            if (Custom.DistLess(this.tChunks[i].pos, this.idealGrabPos, num))
            {
                num = Vector2.Distance(this.tChunks[i].pos, this.idealGrabPos);
            }
        }
        return num;
    }

    #endregion





    public override void NewRoom(Room room)
    {
        base.NewRoom(room);
        graphics.Reset(connectedChunk.pos);
    }


    public void LeaveRoom(Room oldRoom)
    {
        ReleaseGrasp(true);
    }


    public override IntVector2 GravityDirection()
    {
        if (UnityEngine.Random.value >= 0.5f)
        {
            return new IntVector2(0, -1);
        }
        return new IntVector2((base.Tip.pos.x < this.connectedChunk.pos.x) ? -1 : 1, -1);

    }


    public float Rad(float f)
    {
        f = Mathf.Max(1f - f, Mathf.Sin(3.1415927f * Mathf.InverseLerp(0.7f, 1f, f)));
        return Mathf.Lerp(this.tipRad, this.rootRad, f);
    }




    public void SwitchTask(Task newTask)
    {
        if (newTask != Task.Grabbing && this.grabChunk != null)
        {
            this.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, this.grabChunk.pos);
            this.grabChunk = null;
        }
        if (task != newTask)
        {
            // Plugin.Log("tentacle switch task:", task, "=>", newTask);
            this.task = newTask;
        }
    }



    public enum Task
    {
        Locomotion,
        Idle,
        Hunt,
        Grabbing,
    }






}

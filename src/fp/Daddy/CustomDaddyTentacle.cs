using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using MoreSlugcats;

namespace Caterators_by_syhnne.fp.Daddy;








// 他妈的，太难绷了，猫死了之后尸体要是被捡走了，这一截触手会留在原地
// 但这不是重点……这得以后再修……（强忍

public class CustomDaddyTentacle : Tentacle
{
    public DaddyModule daddy;
    
    public float tipRad = 1f;
    public float rootRad = 5f;
    public TentacleRopeGraphics graphics;
    public Tentacle.TentacleChunk firstChunk => tChunks[0];
    public Tentacle.TentacleChunk lastChunk => tChunks[tChunks.Count() - 1];

    // DLL features
    public Creature.Grasp[] grasps;
    // public BodyChunk grabChunk;
    public AbstractCreature huntCreature;
    public int stun = 0;
    public Task task = Task.Locomotion;
    public float sticky;
    public bool neededForLocomotion;
    public float awayFromBodyRotation;
    public Vector2 huntDirection;
    public Vector2 lastSeenPos;


    public CustomDaddyTentacle(DaddyModule owner, BodyChunk connectedChunk, float length) : base(owner.player, connectedChunk, length)
    {
        daddy = owner;
        limp = false;
        neededForLocomotion = false;
        tProps = new Tentacle.TentacleProps(false, true, false, 0.5f, 0f, 0f, 0f, 0f, 3.2f, 7f, 0.25f, 5f, 15, 60, 12, 20);
        tChunks = new Tentacle.TentacleChunk[5];
        for (int i = 0; i < tChunks.Length; i++)
        {
            tChunks[i] = new Tentacle.TentacleChunk(this, i, (float)(i + 1) / (float)tChunks.Length, Mathf.Lerp(this.rootRad, this.tipRad, (float)i / (float)(tChunks.Length - 1)))
            {
                collideWithTerrain = true,
            };
        }
        stretchAndSqueeze = 0.1f;
        
        this.graphics = new(this);
        this.graphics.Reset(this.firstChunk.pos);
        this.grasps = new Creature.Grasp[1];
    }

    





    public override void Update()
    {
        base.Update();
        graphics.Update();
        if (this.grabChunk != null && (this.grabChunk.owner.room == null || this.grabChunk.owner.room != this.daddy.player.room))
        {
            this.stun = 10;
            this.grabChunk = null;
        }
        if (this.daddy.player.dead)
        {
            neededForLocomotion = true;
            this.grabChunk = null;
            this.limp = true;
        }
        if (this.stun > 0)
        {
            this.stun--;
            this.grabChunk = null;
        }
        if (this.grabChunk != null)
        {
            float num = Vector2.Distance(base.Tip.pos, this.grabChunk.pos);
            float num2 = (base.Tip.rad + this.grabChunk.rad) / 4f;
            Vector2 a = Custom.DirVec(base.Tip.pos, this.grabChunk.pos);
            float num3 = this.grabChunk.mass / (this.grabChunk.mass + 0.01f);
            float d = 1f;
            base.Tip.pos += a * (num - num2) * num3 * d;
            base.Tip.vel += a * (num - num2) * num3 * d;
            this.grabChunk.pos -= a * (num - num2) * (1f - num3) * d;
            this.grabChunk.vel -= a * (num - num2) * (1f - num3) * d;
            if (this.grabChunk.owner is Player && UnityEngine.Random.value < Mathf.Lerp(0f, 1f / 10f, (this.grabChunk.owner as Player).GraspWiggle))
            {
                this.stun = Math.Max(this.stun, UnityEngine.Random.Range(1, 17));
                this.grabChunk = null;
            }
        }
        this.limp = (!this.daddy.player.Consious || this.stun > 0);
        for (int i = 0; i < this.tChunks.Length; i++)
        {
            this.tChunks[i].vel *= 0.9f;
            if (this.limp)
            {
                Tentacle.TentacleChunk tentacleChunk = this.tChunks[i];
                tentacleChunk.vel.y = tentacleChunk.vel.y - 0.5f;
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
                tentacleChunk2.vel.y = tentacleChunk2.vel.y - 0.7f;
            }
            return;
        }




        this.awayFromBodyRotation = 0f;
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
            this.SwitchTask(Task.Locomotion);
        }
        else if (this.task != Task.Hunt && this.huntCreature != null)
        {
            this.huntCreature = null;
        }

        // 准备改一下这里，抓住别的东西不松手的时候会直接把对方拽进管道
        if (this.task == Task.Grabbing && (this.grabChunk == null || this.grabChunk.owner.room != this.room || (ModManager.MMF && !this.daddy.player.Consious)))
        {
            this.SwitchTask(Task.Locomotion);
        }
        else if (this.task != Task.Grabbing && this.grabChunk != null)
        {
            this.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, this.grabChunk.pos);
            this.grabChunk = null;
        }

        /*if (this.task == DaddyTentacle.Task.Locomotion)
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

        // TODO: 写闲置状态
        switch (task)
        {
            // 真神奇，我没见任何一个地方给scratchPath 赋值，这玩意是个private不存在其他东西给他赋值的可能性，但它还能跑，还不卡bug
            case Task.Locomotion:
                Climb(ref scratchPath);
                break;
            case Task.Hunt:
                Hunt(ref scratchPath);
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
                    if (this.tChunks[l].phase > -1f || this.room.GetTile(this.tChunks[l].pos).Solid)
                    {
                        Plugin.Log("tentacle release grasp:", grabChunk.owner);
                        this.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, this.grabChunk.pos);
                        this.grabChunk = null;
                        this.SwitchTask(Task.Locomotion);
                        break;
                    }
                }
                // 这块建议重写，因为这个代码是给香菇用的，考虑到香菇的质量，其他物体对香菇的作用力可以忽略
                // 但玩家是蛞蝓猫，再怎么变香菇了这个问题也是要考虑的
                // 而且我严重怀疑他有bug
                // 还得额外写一行代码来判断要不要松手，松手条件是离得足够远，或者玩家空手按了投掷键
                // 然后……我就不管了……多少行代码了已经……差不多够了吧……
                if (this.task == Task.Grabbing)
                {
                    Vector2 vec = Vector3.Slerp(Custom.DirVec(this.grabChunk.pos, p), Custom.DirVec(base.Tip.pos, this.tChunks[this.tChunks.Length - 2].pos) , 0.5f) * Custom.LerpMap((float)this.grabPath.Count, 3f, 18f, 0.65f, 0.25f) * 0.45f / this.grabChunk.mass;
                    this.grabChunk.vel += vec;
                }
                break;
        }

        Touch();
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
            // 能抓玩家这个事情有点难绷，主要是会引发别的问题，所以暂且禁用
            // 按理说不能联机的，有空稍微修一修，应该出避难所之前就把队友全吃了
            if (creatures[i].realizedCreature == null || creatures[i].realizedCreature.inShortcut || creatures[i].realizedCreature is Player || creatures[i].tentacleImmune) continue;

            Creature realizedCreature = creatures[i].realizedCreature;
            for (int j = 0; j < this.tChunks.Length; j++)
            {
                int k = 0;
                while (k < realizedCreature.bodyChunks.Length)
                {
                    // i - 当前生物 j - 触手体块 k - 生物体块

                    // 意思就是边缘有接触
                    // 给他留点误差
                    if (Custom.DistLess(this.tChunks[j].pos, realizedCreature.bodyChunks[k].pos, this.tChunks[j].rad + realizedCreature.bodyChunks[k].rad + 2f))
                    {
                        if (realizedCreature.abstractCreature.creatureTemplate.AI && realizedCreature.abstractCreature.abstractAI.RealAI != null && realizedCreature.abstractCreature.abstractAI.RealAI.tracker != null)
                        {
                            realizedCreature.abstractCreature.abstractAI.RealAI.tracker.SeeCreature(this.daddy.player.abstractCreature);
                        }
                        this.CollideWithCreature(j, realizedCreature.bodyChunks[k]);
                        if (!neededForLocomotion && task != Task.Grabbing && realizedCreature.newToRoomInvinsibility < 1 && this.grasps[0] == null && j == this.tChunks.Length - 1 && (this.task == Task.Hunt || !this.IsCreatureCaughtEnough(realizedCreature.abstractCreature)))
                        {
                            flag = true;
                            if (Vector2.Distance(this.tChunks[j].vel, realizedCreature.bodyChunks[k].vel) >= Mathf.Lerp(1f, 8f, this.sticky))
                            {
                                break;
                            }
                            bool flag3 = false;
                            if (realizedCreature.State.meatLeft > 0)
                            {
                                flag3 = true;
                            }
                            int num = 0;
                            while (num < this.tChunks.Length && flag3)
                            {
                                if (this.tChunks[num].phase > -1f || this.room.GetTile(this.tChunks[num].pos).Solid)
                                {
                                    flag3 = false;
                                }
                                num++;
                            }
                            // 呃，数值我瞎填的，这下估计很难抓住东西了罢
                            if (flag3 && Grab(realizedCreature, 0, k, Creature.Grasp.Shareability.CanNotShare, 0.6f, true, false))
                            {
                                Plugin.Log("tentacle grab creature:", realizedCreature);
                                // this.grabChunk = realizedCreature.bodyChunks[k];
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
                            if (realizedCreature.State.meatLeft <= 0)
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




    private void LookForCreaturesToHunt()
    {
        if (this.neededForLocomotion || daddy.player.room == null)
        {
            return;
        }
        AbstractCreature targetCreature = null;
        if (this.huntDirection == Vector2.zero)
        {
            this.huntDirection = Custom.RNV() * 80f;
        }
        if (this.daddy.player.input[0].AnyDirectionalInput)
        {
            this.huntDirection = new Vector2((float)daddy.player.input[0].x, (float)daddy.player.input[0].y) * 80f;
        }
        Creature crit = null;
        float num = float.MaxValue;
        float current = Custom.VecToDeg(this.huntDirection);
        var creatures = daddy.player.room.abstractRoom.creatures;
        for (int i = 0; i < creatures.Count; i++)
        {
            if (creatures[i].realizedCreature != null && creatures[i].realizedCreature is not Player)
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
                if (this.daddy.tentacles[i].grasps[0] != null && this.daddy.tentacles[i].grasps[0].grabbed is Creature && (this.daddy.tentacles[i].grasps[0].grabbed as Creature).abstractCreature == crit)
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

    // 
    private void Hunt(ref List<IntVector2> path)
    {
        if (this.huntCreature.pos.room != this.daddy.player.abstractCreature.pos.room)
        {
            this.SwitchTask(Task.Locomotion);
            return;
        }
        else if (huntCreature.realizedCreature != null && !room.VisualContact(huntCreature.realizedCreature.mainBodyChunk.pos, connectedChunk.pos))
        {
            this.SwitchTask(Task.Locomotion);
            return;
        }
        else if (huntCreature.realizedCreature != null)
        {
            lastSeenPos = huntCreature.realizedCreature.mainBodyChunk.pos;
            
        }
        base.MoveGrabDest(lastSeenPos, ref path);

        /*if ((float)this.grabPath.Count * 20f > this.idealLength || this.neededForLocomotion)
        {
            float num = float.MaxValue;
            int distance = -1;
            for (int k = 0; k < this.daddy.tentacles.Length; k++)
            {
                if (this.daddy.tentacles[k].task == Task.Locomotion && !this.daddy.tentacles[k].neededForLocomotion && (this.daddy.tentacles[k].idealLength > this.idealLength || this.neededForLocomotion) && !this.daddy.tentacles[k].atGrabDest && Mathf.Abs(this.daddy.tentacles[k].idealLength - (float)this.grabPath.Count * 20f) < num)
                {
                    num = Mathf.Abs(this.daddy.tentacles[k].idealLength - (float)this.grabPath.Count * 20f);
                    distance = k;
                }
            }
            if (distance > -1)
            {
                this.daddy.tentacles[distance].huntCreature = this.huntCreature;
                this.daddy.tentacles[distance].task = DaddyTentacle.Task.Hunt;
                this.huntCreature = null;
                this.UpdateClimbGrabPos(ref path);
                return;
            }
        }
        if (Vector2.Distance(this.room.MiddleOfTile(this.huntCreature.BestGuessForPosition()), base.FloatBase) > this.idealLength * 1.5f)
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
                    this.tChunks[l].vel += Vector2.ClampMagnitude(this.floatGrabDest.Value - this.tChunks[l].pos, 20f) / 20f * 1.2f;
                }
                else
                {
                    this.tChunks[l].vel += Vector2.ClampMagnitude(this.room.MiddleOfTile(this.segments[this.tChunks[l].currentSegment]) - this.tChunks[l].pos, 20f) / 20f * 1.2f;
                }
            }
        }
    }


    private void Climb(ref List<IntVector2> path)
    {
        // TODO
    }



    public override void NewRoom(Room room)
    {
        base.NewRoom(room);
        graphics.Reset(connectedChunk.pos);
        if (this.grasps != null)
        {
            foreach (Creature.Grasp grasp in this.grasps)
            {
                if (grasp != null)
                {
                    room.AddObject(grasp.grabbed);
                    for (int j = 0; j < grasp.grabbed.bodyChunks.Length; j++)
                    {
                        grasp.grabbed.bodyChunks[j].pos = this.connectedChunk.pos;
                        grasp.grabbed.bodyChunks[j].lastPos = this.connectedChunk.pos;
                        grasp.grabbed.bodyChunks[j].lastLastPos = this.connectedChunk.pos;
                        grasp.grabbed.bodyChunks[j].setPos = null;
                        grasp.grabbed.bodyChunks[j].vel *= 0f;
                    }
                }
            }
        }
    }



    public void LeaveRoom(Room oldRoom)
    {
        if (this.grasps != null)
        {
            foreach (Creature.Grasp grasp in this.grasps)
            {
                if (grasp != null)
                {
                    if (grasp.grabbed is Creature)
                    {
                        (grasp.grabbed as Creature).FlyAwayFromRoom(true);?
                    }
                    else if (this.room != null)
                    {
                        this.room.RemoveObject(grasp.grabbed);
                    }
                }
            }
        }
    }



    public void LoseAllGrasps()
    {
        if (this.grasps.Count() > 0)
        {
            for (int i = 0; i < this.grasps.Length; i++)
            {
                this.ReleaseGrasp(i);
            }
        }
    }


    public virtual void ReleaseGrasp(int grasp)
    {
        if (grasp - 1 > this.grasps.Length)
        {
            Plugin.Log("tentacle ReleaseGrasp(): index out of range");
            return;
        }
        // 哈哈，坏了菜了，这个grabber必须是一个creature，因为要调用这个creature的grasps
        // 造成的效果就是玩家会松手
        // 好了我要回档了，这些东西留着我哪天吃饱了撑的真准备用Creature.Grasp的时候再写吧
        this.grasps[grasp]?.Release();
    }



    public bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (this.grasps == null || graspUsed < 0 || graspUsed > this.grasps.Length)
        {
            return false;
        }
        if (obj.slatedForDeletetion || (obj is Creature && !(obj as Creature).CanBeGrabbed(daddy.player)))
        {
            return false;
        }
        if (this.grasps[graspUsed] != null && this.grasps[graspUsed].grabbed == obj)
        {
            this.ReleaseGrasp(graspUsed);
            this.grasps[graspUsed] = new Creature.Grasp(this.daddy.player, obj, graspUsed, chunkGrabbed, shareability, dominance, true);
            obj.Grabbed(this.grasps[graspUsed]);
            new AbstractPhysicalObject.CreatureGripStick(this.daddy.player.abstractCreature, obj.abstractPhysicalObject, graspUsed, pacifying || obj.TotalMass < this.daddy.player.TotalMass);
            // TODO: 香菇会影响玩家自身的质量，要改这个totalmass
            return true;
        }
        foreach (Creature.Grasp grasp in obj.grabbedBy)
        {
            if (grasp.grabber == daddy.player || (grasp.ShareabilityConflict(shareability) && ((overrideEquallyDominant && grasp.dominance == dominance) || grasp.dominance > dominance)))
            {
                return false;
            }
        }
        for (int i = obj.grabbedBy.Count - 1; i >= 0; i--)
        {
            if (obj.grabbedBy[i].ShareabilityConflict(shareability))
            {
                obj.grabbedBy[i].Release();
            }
        }
        if (this.grasps[graspUsed] != null)
        {
            this.ReleaseGrasp(graspUsed);
        }
        this.grasps[graspUsed] = new Creature.Grasp(daddy.player, obj, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
        obj.Grabbed(this.grasps[graspUsed]);
        new AbstractPhysicalObject.CreatureGripStick(daddy.player.abstractCreature, obj.abstractPhysicalObject, graspUsed, pacifying || obj.TotalMass < daddy.player.TotalMass);
        return true;
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
        if (newTask != Task.Grabbing && this.grasps[0] != null)
        {
            this.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, this.grasps[0].grabbedChunk.pos);
            grasps[0].Release();
        }
        if (task != newTask)
        {
            Plugin.Log("tentacle switch task:", task, "=>", newTask);
            this.task = newTask;
        }
    }



    public enum Task
    {
        Idle,
        Locomotion,
        Hunt,
        Grabbing,
    }









    public void ReleaseDoorForbiddenCreatures(bool enteringShortcut, bool enteringDen)
    {
        if ((ModManager.MMF && MMF.cfgVanillaExploits.Value) || (!enteringShortcut && !enteringDen))
        {
            return;
        }
        // TODO
    }

    

}

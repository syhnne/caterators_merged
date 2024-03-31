using System;
using UnityEngine;
using RWCustom;
using HUD;




namespace Caterators_by_syhnne;





// 其实我想把这玩意重写一遍
public class GravityController : UpdatableAndDeletable
{
    public Player player;
    private bool unlocked;
    private bool everUsed = false;
    public int gravityControlCounter = 0;
    public int gravityBonus = 10;
    private static readonly int gravityControlSpeed = 20;
    public float amountZeroG;
    public float amountBrokenZeroG;
    public bool enabled = true;
    public bool isAbleToUse = false;
    public bool lastRoomHasEffect = true;
    public bool RoomHasEffect = true;
    public int inputY = 0;

    public GravityController(Player player)
    {
        this.player = player;
        unlocked = (player.room.game.IsStorySession && player.room.game.IsCaterator() && player.room.game.GetStorySession.saveState.deathPersistentSaveData.ascended) || Plugin.DevMode;
        /*if (player.room != null && player.room.abstractRoom.name == "SS_AI" && player.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
        {
            gravityBonus = 0;
        }*/

    }





    public void Update(bool eu, bool isMyStory)
    {
        if (!enabled || player.room.abstractRoom.name == "SS_E08" || !unlocked) return;

        if (player.room.abstractRoom.name == "SS_AI" || player.room.abstractRoom.name == "SS_A03")
        {
        }
        else if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG) != null)
        {
            if (gravityBonus != (int)Mathf.Round((1f - Mathf.InverseLerp(0f, 0.85f, 1f - player.room.gravity)) * 10f)
            && gravityBonus != (int)Mathf.Round(10f * 1f - player.room.gravity))
            {
                Plugin.Log("gravity mismatch IN BROKEN ZEROG AREA");
                Plugin.Log("-- room gravity: ", player.room.gravity);
                gravityBonus = (int)Mathf.Round((1f - Mathf.InverseLerp(0f, 0.85f, 1f - player.room.gravity)) * 10f);
            }
        }
        else if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) != null)
        {
            if (gravityBonus != (int)Mathf.Round((1f - Mathf.InverseLerp(0f, 0.85f, 1f - player.room.gravity)) * 10f))
            {
                Plugin.Log("gravity mismatch IN ZEROG AREA");
                Plugin.Log("-- room gravity: ", player.room.gravity);
                gravityBonus = (int)Mathf.Round((1f - Mathf.InverseLerp(0f, 0.85f, 1f - player.room.gravity)) * 10f);
            }
        }
        else if (gravityBonus != (int)Mathf.Round(10f * player.room.gravity))
        {
            Plugin.Log("gravity mismatch or coop player changing gravity: ");
            Plugin.Log("-- room gravity: ", player.room.gravity);
            gravityBonus = (int)Mathf.Round(10f * player.room.gravity);
        }


        if (player.Consious && !player.dead && player.stun == 0
            && Input.GetKey(Plugin.instance.option.GravityControlKey.Value)
            && player.animation != Player.AnimationIndex.ZeroGPoleGrab)
        {
            player.Blink(5);
            isAbleToUse = true;
            everUsed = true;
        }
        else { isAbleToUse = false; }



        if (isAbleToUse && inputY != 0)
        {
            gravityControlCounter++;
            if (gravityControlCounter >= gravityControlSpeed)
            {
                gravityBonus += inputY;
                if (gravityBonus >= 0)
                {

                    if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) != null || player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG) != null || player.room.abstractRoom.name == "SS_AI")
                    {
                        Plugin.Log("HAS GRAVITY EFFECT");
                        if (gravityBonus <= 10)
                        {
                            if (player.room.abstractRoom.name == "SS_AI") { }
                            else
                            {
                                player.room.gravity = 1f - Mathf.Lerp(0f, 0.85f, 1f - gravityBonus * 0.1f);
                                // 找到并修改zeroG这个效果。原来他能这么用啊。。
                                if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) != null)
                                {
                                    player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG).amount = 0.1f * (10 - gravityBonus);
                                    Plugin.Log("zeroG amount: ", 0.1f * (10 - gravityBonus));
                                }
                                if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG) != null)
                                {
                                    player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG).amount = 0.1f * (10 - gravityBonus);
                                    Plugin.Log("broken zeroG amount: ", 0.1f * (10 - gravityBonus));
                                }
                            }


                        }

                        else { gravityBonus = 10; }

                    }
                    // 由于颜色显示有上限，所以再加个钳制……
                    // 没事了，颜色显示的方案失效了，我在想要不要把这个钳制去了，让闲的没事的人把重力改到100，然后因为下台阶而摔死
                    else if (gravityBonus <= 80)
                    {
                        player.room.gravity = 0.1f * gravityBonus;
                    }
                    else { gravityBonus = 80; }

                }
                else { gravityBonus = 0; }

                Plugin.Log("player gravity control RESULT" + player.room.gravity);
                gravityControlCounter = 0;
            }
        }
    }






    public void NewRoom(bool isMyStory)
    {
        if (player.room == null) return;
        lastRoomHasEffect = RoomHasEffect;
        /*// 非本模组剧情下不能控制五卵石、腐化之地、仰望皓月地区重力（避免玩家发现我的代码有bug……
        if (!isMyStory && (player.room.abstractRoom.name.StartsWith("SS") || player.room.abstractRoom.name.StartsWith("DM") || player.room.abstractRoom.name.StartsWith("RM")))
        {
            RoomHasEffect = true;
            gravityBonus = 0;
            return;
        }*/
        // 防止玩家一开局就看见fp倒着漂浮在空中（好崩溃
        if (player.room.game.StoryCharacter == Enums.FPname && player.room.abstractRoom.name == "SS_AI" && !player.room.game.GetStorySession.saveState.deathPersistentSaveData.ascended && player.stillInStartShelter)
        {
            // 这里不能直接修改重力，只能修改bonus，让SSOracleBehavior_SubBehavior_lowGravity或者unconsciousUpdate来读这个
            RoomHasEffect = true;
            gravityBonus = 10;
        }
        // 防止屋里那个大家伙崩我游戏。。
        else if (player.SlugCatClass == Enums.FPname && player.room.abstractRoom.name == "SS_A03")
        {
            RoomHasEffect = true;
            gravityBonus = 0;
            return;
        }
        // 如果没启用，只同步一下gravityBonus就返回
        else if (!enabled || !unlocked || !everUsed)
        {
            if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) != null || player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG) != null || player.room.abstractRoom.name == "SS_AI")
            {

                RoomHasEffect = true;
                gravityBonus = (int)Mathf.Round((1f - Mathf.InverseLerp(0f, 0.85f, 1f - player.room.gravity)) * 10f);
                // player.room.world.rainCycle.brokenAntiGrav.on = false;
            }
            else
            {
                RoomHasEffect = false;
                gravityBonus = (int)Mathf.Round(10f * player.room.gravity);
            }
            return;
        }
        // 有零重力效果的情况下，记录并且修改效果（记录是为了防止玩家死的时候恢复不了）
        else if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) != null || player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG) != null || player.room.abstractRoom.name == "SS_AI")
        {
            RoomHasEffect = true;
            if (gravityBonus <= 10)
            {
                // 防止从其他地方传送进五卵石内部的时候，由于携带了原本的重力而摔死
                if (!lastRoomHasEffect || player.room.abstractRoom.name == "SS_AI") { return; }
                player.room.gravity = 0.1f * gravityBonus;
                // 找到并修改zeroG这个效果
                bool z = false;
                bool b = false;

                if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) != null)
                {
                    amountZeroG = player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG).amount;
                    player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG).amount = 0.1f * (10 - gravityBonus);
                    Plugin.Log("room effect set - newroom - z, amount:", amountZeroG, " to: ", 0.1f * (10 - gravityBonus));
                    z = true;
                }
                if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG) != null)
                {
                    amountBrokenZeroG = player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG).amount;
                    player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG).amount = 0.1f * (10 - gravityBonus);
                    Plugin.Log("room effect set - newroom - b, amount:", amountBrokenZeroG, " to: ", 0.1f * (10 - gravityBonus));
                    b = true;
                }

                if (!z) amountZeroG = 0f;
                if (!b) amountBrokenZeroG = 0f;
                Plugin.Log("NewRoom ! z: ", amountZeroG, " b: ", amountBrokenZeroG);
            }
            else
            {
                Plugin.Log("gravityBonus out of range, cleared");
                gravityBonus = (int)Mathf.Round(10f * player.room.gravity);
            }
        }
        // 没重力效果的时候直接修改
        else
        {
            RoomHasEffect = false;
            if (lastRoomHasEffect) { return; }
            player.room.gravity = gravityBonus * 0.1f;
        }


    }




    // 其实disable也要调用这个函数，是不是应该给他改个名
    public void Die()
    {
        if (player.room == null || !enabled) return;
        if (player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) != null || player.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG) != null)
        {
            for (int i = 0; i < player.room.roomSettings.effects.Count; i++)
            {
                if (player.room.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.ZeroG)
                {
                    player.room.roomSettings.effects[i].amount = amountZeroG;
                    Plugin.Log("room effect set to original value because player died - ZeroG ", player.room.roomSettings.effects[i].amount);
                    break;
                }
                if (player.room.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.BrokenZeroG)
                {
                    player.room.roomSettings.effects[i].amount = amountBrokenZeroG;
                    Plugin.Log("room effect set to original value because player died - BrokenZeroG ", player.room.roomSettings.effects[i].amount);
                    player.room.world.rainCycle.brokenAntiGrav.on = true;
                    break;
                }
            }
        }
        else
        {
            player.room.gravity = 1f;
            gravityBonus = 10;
            Plugin.Log("gravity set to 1.0 because player died");
        }

    }



    public override void Destroy()
    {
        this.Die();
        base.Destroy();
    }






}









// 这个东西其实不是很好看。首先我不会写那种丝滑的显示效果，我怕我一写他就坏掉。
// 其次，我尝试过通过修改圆圈的颜色来区分不同的重力倍数，但他最后只会给我显示成黑白灰，我不知道为什么。
public class GravityMeter : HudPart
{
    private GravityController owner;
    public Vector2 pos;
    public Vector2 lastPos;
    public float fade;
    public float lastFade;
    public HUDCircle[] circles;
    public HUDCircle[] rows;

    public static Color[] GravityMeterColors = new Color[]
        {
            new Color32(255, 255, 255, 255),
            new Color32(190, 255, 231, 255),
            new Color32(85, 243, 239, 255),
            new Color32(6, 164, 219, 255),
            new Color32(5, 29, 243, 255),
            new Color32(27, 5, 142, 255),
            new Color32(206, 3, 180, 255),
            new Color32(255, 0, 0, 255)
        };


    public GravityMeter(HUD.HUD hud, FContainer fContainer, GravityController owner) : base(hud)
    {
        circles = new HUDCircle[10];
        rows = new HUDCircle[7];
        this.owner = owner;
        pos = owner.player.mainBodyChunk.pos - Vector2.zero;
        lastPos = pos;
        fade = 0f;
        lastFade = 0f;
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
            circles[i].sprite.isVisible = true;
            circles[i].rad = 3f;
            circles[i].thickness = 1f;
            circles[i].visible = true;
        }
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
            rows[i].sprite.isVisible = true;
            rows[i].rad = 40f + (float)i * 4f + i;
            rows[i].thickness = 1f;
            rows[i].visible = true;
            rows[i].pos = owner.player.mainBodyChunk.pos - Vector2.zero;
        }
    }


    private bool Show
    {
        get
        {
            return (owner != null && owner.player.room != null && owner.isAbleToUse);
        }
    }



    public override void Update()
    {

        base.Update();
        lastPos = pos;
        lastFade = fade;
        Vector2 camPos = Vector2.zero;
        if (owner.player.room != null)
        {
            camPos = owner.player.room.game.cameras[0].pos;
        }
        pos = owner.player.mainBodyChunk.pos - camPos;
        int gravityInt = owner.gravityBonus % 10;
        int gravityLevel = owner.gravityBonus / 10;
        // if (gravityLevel >= 7) gravityLevel = 7;
        if (owner.RoomHasEffect && gravityLevel != 0 && gravityInt == 0)
        {
            gravityLevel--;
            gravityInt = 10;
        }

        if (Show)
        {
            fade = Mathf.Min(1f, fade + 0.033333335f);
        }
        else
        {
            fade = Mathf.Max(0f, fade - 0.1f);
        }



        for (int i = 0; i < gravityInt; i++)
        {
            circles[i].thickness = Math.Min(5f, circles[i].thickness + 0.333333335f);
            circles[i].rad = Math.Max(3f, circles[i].rad - 0.1f);
        }
        for (int i = gravityInt; i < circles.Length; i++)
        {
            circles[i].thickness = Math.Max(1f, circles[i].thickness - 0.333333335f);
            circles[i].rad = Math.Min(4f, circles[i].rad + 0.1f);
        }
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].Update();
            circles[i].fade = fade;
            circles[i].pos = pos + new Vector2((float)i * 21.6f, 0f);
            circles[i].visible = true;
            circles[i].pos = pos + Custom.DegToVec((1f - (float)i / (float)circles.Length) * 360f * Custom.SCurve(Mathf.Pow(fade, 1.5f - ((float)i / (float)(circles.Length - 1))), 0.6f)) * (32f);
        }

        for (int i = 0; i < gravityLevel; i++)
        {
            rows[i].visible = true;
        }
        for (int i = gravityLevel; i < rows.Length; i++)
        {
            rows[i].visible = false;
        }
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].Update();
            rows[i].fade = fade;
            rows[i].pos = pos;
        }

    }



    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame) return;
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].Draw(timeStacker);
        }
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].Draw(timeStacker);
        }
    }

    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(lastPos, pos, timeStacker);
    }



    public override void ClearSprites()
    {
        base.ClearSprites();
    }
}

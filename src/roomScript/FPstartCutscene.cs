using Caterators_by_syhnne.fp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using MoreSlugcats;

namespace Caterators_by_syhnne.roomScript;

public class FPstartCutscene : UpdatableAndDeletable
{
    private int timer;
    private new Room room;
    private Player player;


    public FPstartCutscene(Room room)
    {
        timer = 0;
        this.room = room;
        if (room.game != null && room.game.Players != null && room.game.Players[0].realizedCreature != null)
        {
            player = room.game.Players[0].realizedCreature as Player;
        }
    }


    public override void Update(bool eu)
    {
        // 如果玩家离开过演算室，就不会再播放动画了
        if (player == null)
        {
            if (room.game != null && room.game.Players != null && room.game.Players[0].realizedCreature != null)
            {
                player = room.game.Players[0].realizedCreature as Player;
            }
        }
        else if (!player.stillInStartShelter) { return; }




        base.Update(eu);

        if (timer == 10)
        {
            Plugin.Log("START CUTSCENE room effects");
            if (room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) == null)
            {
                room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.DarkenLights, 0f, false));
            }
            if (room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) == null)
            {
                room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Darkness, 0f, false));
            }
            if (room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
            {
                room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 0f, false));
            }

            // 找到fp并给他一个初速度，避免玩家一开局看到难绷的画面（
            Oracle oracle = null;
            for (int j = 0; j < room.physicalObjects.Length; j++)
            {
                for (int k = 0; k < room.physicalObjects[j].Count; k++)
                {
                    if (room.physicalObjects[j][k] is Oracle)
                    {
                        oracle = (room.physicalObjects[j][k] as Oracle);
                        break;
                    }
                }
                if (oracle != null)
                {
                    break;
                }
            }
            if (oracle != null && oracle.ID == Oracle.OracleID.SS)
            {
                oracle.firstChunk.vel += Vector2.right;
            }
        }

        AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
        if (room.game.IsStorySession && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room && room.game.GetStorySession.saveState.cycleNumber == 0)
        {
            Player player = firstAlivePlayer.realizedCreature as Player;

            // 不知道这个会不会有bug，碰见问题先把他注释了
            player.objectInStomach = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null, new WorldCoordinate(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID());
            if (timer <= 110)
            {
                player.SuperHardSetPosition(new Vector2(569.7f, 643.5f));
            }
            if (timer == 110)
            {
                Plugin.Log("START CUTSCENE player enter");
                player.mainBodyChunk.vel = new Vector2(0f, -2f);
                player.Stun(60);
            }
        }
        // 怎么播放这个也没声音啊（恼
        if (timer >= 80 && timer < 110)
        {
            room.PlaySound(SoundID.Player_Tick_Along_In_Shortcut, new Vector2(569.7f, 643.5f));
        }
        if (timer == 110)
        {
            room.PlaySound(SoundID.Player_Exit_Shortcut, new Vector2(569.7f, 643.5f));
        }

        if (timer == 180)
        {
            // 屏幕怎么不晃啊（恼
            // 晃啊！tnnd，为什么不晃！！
            for (int i = 0; i < room.game.cameras.Length; i++)
            {
                if (room.game.cameras[i].room == room && !room.game.cameras[i].AboutToSwitchRoom)
                {
                    room.game.cameras[i].ScreenMovement(null, Vector2.zero, 15f);
                }
            }
        }
        if (this.timer > 180 && this.timer < 260 && this.timer % 16 == 0)
        {
            room.ScreenMovement(null, new Vector2(0f, 0f), 2.5f);
            for (int j = 0; j < 6; j++)
            {
                if (Random.value < 0.5f)
                {
                    room.AddObject(new OraclePanicDisplay.PanicIcon(new Vector2((float)Random.Range(230, 740), (float)Random.Range(100, 620))));
                }
            }
        }

        if (timer == 340)
        {
            room.AddObject(new TestSprite(new Vector2(300, 500), 6, 2f));
        }
        if (timer == 350)
        {
            room.AddObject(new TestSprite(new Vector2(300, 360), 12, 1.5f));
        }
        if (timer == 360)
        {
            room.AddObject(new TestSprite(new Vector2(300, 300), 15, 1.5f));
        }


        if (timer == 580)
        {
            room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Press G and up&down arrow keys to adjust the gravity in room."), 140, 300, true, true);
            Destroy();
            return;
            // Plugin.Log("total time: ", room.game.GetStorySession.saveState.totTime);
        }
        // Plugin.Log("start cutscene timer: ", timer);
        timer++;
    }
}




public class TestSprite : CosmeticSprite
{

    private bool visible = true;
    private int num;
    private float scale;

    public TestSprite(Vector2 position, int num, float scale)
    {
        pos = position;
        this.num = num;
        this.scale = scale;
    }




    public override void Update(bool eu)
    {
        timer++;

        if (timer >= 190)
        {
            Destroy();
        }
        base.Update(eu);
    }






    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[num];
        FSprite[] glyphs =
        {
            new("BigGlyph0", true),
            new("BigGlyph1", true),
            new("BigGlyph2", true),
            new("BigGlyph3", true),
            new("BigGlyph4", true),
            new("BigGlyph5", true),
            new("BigGlyph6", true),
            new("BigGlyph7", true),
            new("BigGlyph8", true),
            new("BigGlyph9", true),
            new("BigGlyph10", true),
            new("BigGlyph11", true),
            new("BigGlyph12", true)
        };
        System.Random r = new();
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            int randint = r.Next(0, glyphs.Length);
            // Plugin.Log("sprites: ", i, " sp: ", randint);
            sLeaser.sprites[i] = glyphs[randint];
            sLeaser.sprites[i].color = new Color(0f, 0f, 0f);
            sLeaser.sprites[i].isVisible = true;
            sLeaser.sprites[i].scale = scale;

        }
        // 世界未解之谜：为什么有的会显示不出来
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
    }






    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (timer > 160 && timer % 8 < 4)
        {
            visible = false;
        }
        else { visible = true; }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].x = pos.x - camPos.x + (20 * scale * i);
            sLeaser.sprites[i].y = pos.y - camPos.y;
            sLeaser.sprites[i].isVisible = visible;

        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }


    public override void Destroy()
    {
        base.Destroy();
    }


    // Token: 0x040041BE RID: 16830
    public int timer;

    // Token: 0x040041BF RID: 16831
    public float circleScale;
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu;
using MoreSlugcats;
using UnityEngine;

namespace Caterators_by_syhnne.roomScript;



// 这应该是正经结局，好了，那么问题来了，fp猫猫是怎么把自己整活的。回头我得想想，现在的内容是只要进了这个房间且掉在地板上（y<400f？）过几秒就触发结局
public class FPaltEndingCutscene : UpdatableAndDeletable
{

    public bool endingTriggered;
    public int endingTriggerTime;
    private Player foundPlayer;
    private bool setController;
    public FadeOut fadeOut;
    private bool doneFinalSave;

    internal FPaltEndingCutscene(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        // 测试的时候用这个粗略判断一下，防止刚一开始就结束了
        if (!room.game.IsStorySession
            || room.game.StoryCharacter != Enums.FPname
            || !room.game.GetStorySession.saveState.miscWorldSaveData.EverMetMoon) return;

        if (!ModManager.CoopAvailable)
        {
            if (foundPlayer == null && room.game.Players.Count > 0 && room.game.Players[0].realizedCreature != null && room.game.Players[0].realizedCreature.room == room)
            {
                foundPlayer = (room.game.Players[0].realizedCreature as Player);
            }
            if (foundPlayer == null || foundPlayer.inShortcut || room.game.Players[0].realizedCreature.room != room)
            {
                return;
            }
        }
        else
        {
            if (foundPlayer == null && room.PlayersInRoom.Count > 0 && room.PlayersInRoom[0] != null && room.PlayersInRoom[0].room == room)
            {
                foundPlayer = room.PlayersInRoom[0];
            }
            if (foundPlayer == null || foundPlayer.inShortcut || foundPlayer.room != room)
            {
                return;
            }
            room.game.cameras[0].EnterCutsceneMode(foundPlayer.abstractCreature, RoomCamera.CameraCutsceneType.Oracle);
        }
        if (foundPlayer.firstChunk.pos.y < 500f && !setController)
        {
            Plugin.Log("Ending cutscene timer:", endingTriggerTime);
            RainWorld.lockGameTimer = true;
            // 应该没必要控制玩家行为。。
            // setController = true;
            // foundPlayer.controller = new EndingController(this);
        }
        if (foundPlayer.firstChunk.pos.y < 500f && !endingTriggered)
        {
            endingTriggerTime++;
            if (endingTriggerTime > 20)
            {
                endingTriggered = true;
                // 这是不是过场动画？
                room.game.manager.sceneSlot = room.game.StoryCharacter;

                if (fadeOut == null)
                {
                    fadeOut = new FadeOut(room, Color.black, 200f, false);
                    room.AddObject(fadeOut);
                }
            }
        }
        if (fadeOut != null && fadeOut.IsDoneFading() && !doneFinalSave)
        {
            Plugin.Log("fpslugcat Alt Ending !!!");
            // 这句话对我来说没用吧
            room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts = 0;
            // 好吧我想到了。在这里挂altending，还是在那个函数里判断吧
            room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
            room.game.GoToRedsGameOver();
            RainWorldGame.BeatGameMode(room.game, false);
            doneFinalSave = true;
        }




    }
}



// public class OE_GourmandEnding : UpdatableAndDeletable 改的这个

internal class SS_PebblesAltEnding
{
    


}
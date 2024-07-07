using Caterators_by_syhnne._public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_by_syhnne.roomScript;

public class MoonStartCutscene : UpdatableAndDeletable
{
    private int timer;
    private new Room room;
    private Player player;


    public MoonStartCutscene(Room room)
    {
        timer = 0;
        this.room = room;
    }


    public override void Update(bool eu)
    {
        if (player == null)
        {
            if (room.game != null && room.game.Players != null && room.game.Players[0].realizedCreature != null)
            {
                player = room.game.Players[0].realizedCreature as Player;
            }
        }
        else if (!player.stillInStartShelter)
        {
            Destroy();
            return;
        }
        if (player == null)
        {
            Plugin.Log("null player! MoonStartCutscene");
            return;
        }


        base.Update(eu);
        timer++;
        


        

        AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
        if (room.game.IsStorySession && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room && room.game.GetStorySession.saveState.cycleNumber == 0)
        {
            Player pl = firstAlivePlayer.realizedCreature as Player;
            if (timer < 70)
            {
                pl.SuperHardSetPosition(new Vector2(1552.6f, 156f));
            }

            if (timer == 100 && Plugin.playerModules.TryGetValue(pl, out var mod))
            {

                if (mod != null && mod.swarmerManager != null)
                {
                    mod.swarmerManager.Respawn();
                    Plugin.Log("spawned moonswarmer SL_AI");
                }

            }
            Plugin.Log("player pos:", pl.firstChunk.pos);

        }

            



        if (timer == 250)
        {
            room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("You have " + moon.MoonSwarmer.SwarmerManager.maxSwarmer + " neurons. Each can withstand one fatal attack for you."), 140, 350, true, true);
            room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Every successfully survived Rain Cycle may restore a neuron fly."), 140, 350, true, true);
            // TODO: 检查语法错误，我急着睡觉所以瞎写
            Destroy();
        }


    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.roomScript;

public class SRSstartTutorial : UpdatableAndDeletable
{
    public SRSstartTutorial(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room.game.IsStorySession && room.game.StoryCharacter == Enums.SRSname && !room.game.GetDeathPersistent().SRSstartTutorial)
        {
            Player player = this.room.game.FirstRealizedPlayer;
            if (ModManager.CoopAvailable)
            {
                player = this.room.game.RealizedPlayerFollowedByCamera;
            }
            if (player != null && player.room == this.room)
            {
                InGameTranslator t = room.game.rainWorld.inGameTranslator;
                room.game.GetDeathPersistent().SRSstartTutorial = true;
                this.room.game.cameras[0].hud.textPrompt.AddMessage(t.Translate("You need to consume a large amount of food in order to maintain body temperature."), 20, 200, true, true);
                // =>你需要大量进食以维持体温
                // 百度翻译写的那玩意还不如我呢 还是我自己翻译吧
                room.game.cameras[0].hud.textPrompt.AddMessage(t.Translate("Hold [up] and [pickup] to create high-temperature needles, stab creatures with them to leech energy."), 20, 200, true, true);
                // =>按住拾取键和上键，从你的体内拔出高温的针头，用它们刺伤生物并吸取营养。
                // 要不我用chatgpt？
                Destroy();
            }
        }

    }

}







public class SRSwaterMessage : UpdatableAndDeletable
{
    public SRSwaterMessage(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room.game.IsStorySession && room.game.StoryCharacter == Enums.SRSname && !room.game.GetDeathPersistent().SRSwaterMessage)
        {
            Player player = this.room.game.FirstRealizedPlayer;
            if (ModManager.CoopAvailable)
            {
                player = this.room.game.RealizedPlayerFollowedByCamera;
            }
            if (player != null && player.room == this.room)
            {
                InGameTranslator t = room.game.rainWorld.inGameTranslator;
                room.game.GetDeathPersistent().SRSwaterMessage = true;
                this.room.game.cameras[0].hud.textPrompt.AddMessage(t.Translate("Exposure to water or cold air can cause your body to lose temperature rapidly."), 20, 200, true, true);
                // => 水体和寒冷环境会让你的身体快速失温
                Destroy();
            }
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.roomScript;

public class NSHinventoryTutorial : UpdatableAndDeletable
{

    public NSHinventoryTutorial(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room.game.IsStorySession && room.game.StoryCharacter == Enums.NSHname && !room.game.GetDeathPersistent().NSHinventoryTutorial)
        {
            Player player = this.room.game.FirstRealizedPlayer;
            if (ModManager.CoopAvailable)
            {
                player = this.room.game.RealizedPlayerFollowedByCamera;
            }
            if (player != null && player.room == this.room)
            {
                InGameTranslator t = room.game.rainWorld.inGameTranslator;
                room.game.GetDeathPersistent().NSHinventoryTutorial = true;
                this.room.game.cameras[0].hud.textPrompt.AddMessage(t.Translate("Hold [D] and [up/down] to store objects in your backpack."), 60, 240, true, true);
                // => 按住D键和上下键，将物品存储在背包里。（好翻译腔啊 但无所谓了 这里是雨世界）
                Destroy();
            }
        }

    }

}




public class NSHneuronTutorial : UpdatableAndDeletable
{

    public NSHneuronTutorial(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room.game.IsStorySession && room.game.StoryCharacter == Enums.NSHname && !room.game.GetDeathPersistent().NSHneuronTutorial)
        {
            Player player = this.room.game.FirstRealizedPlayer;
            if (ModManager.CoopAvailable)
            {
                player = this.room.game.RealizedPlayerFollowedByCamera;
            }
            if (player != null && player.room == this.room)
            {
                InGameTranslator t = room.game.rainWorld.inGameTranslator;
                room.game.GetDeathPersistent().NSHneuronTutorial = true;
                this.room.game.cameras[0].hud.textPrompt.AddMessage(t.Translate("While in the air, press [jump] and [pickup] together to activate the green neuron."), 20, 200, true, true);
                // => 在空中时，同时按下拾取和跳跃键启用绿色的神经元
                this.room.game.cameras[0].hud.textPrompt.AddMessage(t.Translate("After that, grab a slugcat and hold [pickup] to revive them."), 20, 200, true, true);
                // => 然后抓住其他蛞蝓猫来复活他们。
                Destroy();
            }
        }
    }

}

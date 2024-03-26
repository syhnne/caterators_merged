using System;
using Caterators_by_syhnne.srs;


namespace Caterators_by_syhnne;

public class PlayerModule
{
    public readonly WeakReference<Player> playerRef;
    public readonly SlugcatStats.Name playerName;
    public readonly SlugcatStats.Name storyName;
    public readonly bool IsMyStory;
    public readonly bool isCaterator;
    public GravityController gravityController;
    public nsh.Scarf nshScarf;
    public LightSourceModule srsLightSource;


    public PlayerModule(Player player)
    {
        playerRef = new WeakReference<Player>(player);
        playerName = player.slugcatStats.name;
        if (Enums.IsCaterator(playerName))
        {
            isCaterator = true;
        }
        if (player.room.game.session is StoryGameSession)
        {
            storyName = player.room.game.StoryCharacter;
        }
        else { storyName = null; }

        if (playerName == storyName) { IsMyStory = true; }

        if (isCaterator && storyName != null)
        {
            Plugin.Log("gravity controller added for", playerName);
            gravityController = new GravityController(player);
        }


    }

    public void Update(Player player, bool eu)
    {
        if (player.room == null || player.dead) return;
        gravityController?.Update(eu, storyName == playerName);

    }

}

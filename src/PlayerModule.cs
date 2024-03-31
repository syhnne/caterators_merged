using System;



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
    public nsh.Inventory nshInventory;
    public srs.LightSourceModule srsLightSource;


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
            gravityController = new GravityController(player);
        }
        if (playerName == Enums.NSHname)
        {
            nshInventory = new nsh.Inventory(player);
        }
        if (playerName == Enums.SRSname)
        {
            srsLightSource = new srs.LightSourceModule(player);
        }

    }

    public void Update(Player player, bool eu)
    {
        if (player.room == null || player.dead) return;
        nshInventory?.Update(eu);
        gravityController?.Update(eu, storyName == playerName);
        



    }

}

using System;


namespace Caterators_by_syhnne._public;

// 一家人就要整整齐齐，这就是合并代码的好处（强迫症狂喜
public class PlayerModule
{
    public readonly WeakReference<Player> playerRef;
    public readonly SlugcatStats.Name playerName;
    public readonly SlugcatStats.Name storyName;
    public readonly bool IsMyStory;
    public readonly bool isCaterator;

    public _public.GravityController gravityController;
    public nsh.Scarf nshScarf;
    public nsh.Inventory nshInventory;
    public srs.LightSourceModule srsLightSource;
    public _public.DeathPreventer deathPreventer;
    public moon.MoonSwarmer.SwarmerManager swarmerManager;
    public int spearExhaustCounter;
    public fp.PearlReader pearlReader;


    public PlayerModule(Player player, AbstractCreature abstractCreature, World world)
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


        deathPreventer = new DeathPreventer(player);
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
        if (playerName == Enums.Moonname)
        {
            // 最费劲的一集
            swarmerManager = new moon.MoonSwarmer.SwarmerManager(player);
            deathPreventer.swarmerManager = swarmerManager;
            swarmerManager.deathPreventer = deathPreventer;
            // TODO: 算了，拉倒吧，生成神经元这个工作交给开场动画
            swarmerManager.callBackSwarmers = Math.Min(world.game.GetDeathPersistent().MoonHasSwarmers + 1, moon.MoonSwarmer.SwarmerManager.maxSwarmer);
            Plugin.Log("new game! moon has swarmers:", swarmerManager.callBackSwarmers);
        }
        if (playerName == Enums.FPname)
        {
            pearlReader = new fp.PearlReader(player);
        }

    }

    public void Update(Player player, bool eu)
    {
        if (spearExhaustCounter > 0)
        {
            spearExhaustCounter--;
        }
        pearlReader?.Update(eu);
        deathPreventer?.Update();
        swarmerManager?.Update();
        if (srsLightSource != null && srsLightSource.slatedForDeletion) { srsLightSource = null; }
        srsLightSource?.Update();
        if (player.room == null || player.dead) return;
        nshInventory?.Update(eu);
        gravityController?.Update(eu, storyName == playerName);





    }

}

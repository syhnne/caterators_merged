using System;


namespace Caterators_by_syhnne._public;

// 一家人就要整整齐齐，这就是合并代码的好处（强迫症狂喜

// 他妈的。我写的这堆东西，怎么说，他很巧妙，他巧妙地叠加组合在一起，使得工作量指数级地增大了，，
// 举个例子，nsh可以复活队友，但如果他复活的是srs，我得重新生成他的那个光效，如果他复活的是moon，那就更麻烦了，得写个神经元类型转换，要不然moon一活过来发现自己没有神经元又要去世了
// 再比如说，由于这个神经元谁拿了都可以去复活队友，那么如果有人在别的档里拿开发者模式调了一个这玩意儿出来防止自己被fp杀死，他又会有什么反应
// 呃啊。。光是想想就觉得要晕过去了
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
    public _public.PlayerReviver playerReviver;


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
        playerReviver = new PlayerReviver(player);
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
        playerReviver?.Update(eu);
        swarmerManager?.Update();
        if (srsLightSource != null && srsLightSource.slatedForDeletion) { srsLightSource = null; }
        srsLightSource?.Update();
        if (player.room == null || player.dead) return;
        nshInventory?.Update(eu);
        gravityController?.Update(eu, storyName == playerName);





    }

}

using System;
using System.Linq;
using Caterators_by_syhnne.fp;
using RWCustom;
using UnityEngine;


namespace Caterators_by_syhnne._public;

// 一家人就要整整齐齐，这就是合并代码的好处（强迫症狂喜

// 他妈的。我写的这堆东西，怎么说，他很巧妙，他巧妙地叠加组合在一起，使得工作量指数级地增大了，，
// 举个例子，nsh可以复活队友，但如果他复活的是srs，我得重新生成他的那个光效，如果他复活的是moon，那就更麻烦了，得写个神经元类型转换，要不然moon一活过来发现自己没有神经元又要去世了
// 再比如说，由于这个神经元谁拿了都可以去复活队友，那么如果有人在别的档里拿开发者模式调了一个这玩意儿出来防止自己被fp杀死，他又会有什么反应
// 呃啊。。光是想想就觉得要晕过去了

// 绷不住了，我注释都写了复活srs需要重新加光效，结果到头来还是忘了写
public class PlayerModule
{
    public readonly WeakReference<Player> playerRef;
    public readonly SlugcatStats.Name playerName;
    public readonly SlugcatStats.Name storyName;
    public readonly bool IsMyStory;
    public readonly bool isCaterator;

    public GravityController_v2 gravityController;
    public nsh.Scarf nshScarf;
    public nsh.Inventory nshInventory;
    public srs.LightSourceModule srsLightSource;
    public _public.DeathPreventer deathPreventer;
    public moon.MoonSwarmer.SwarmerManager swarmerManager;
    public int spearExhaustCounter;
    public fp.PearlReader pearlReader;
    public fp.DaddyModule daddy;
    public _public.PlayerReviver playerReviver;
    public PlayerGraphics.AxolotlGills gills;
    public bool isMoon = false;


    public PlayerModule(Player player, AbstractCreature abstractCreature, World world)
    {
        // Plugin.Log("quickdeath:", player.Template.quickDeath);

        playerRef = new WeakReference<Player>(player);
        playerName = player.slugcatStats.name;
        if (Enums.IsCaterator(playerName))
        {
            isCaterator = true;
        }
        if (player.room.game.IsStorySession)
        {
            storyName = player.room.game.StoryCharacter;
        }
        else { storyName = null; }

        if (playerName == storyName) { IsMyStory = true; }


        deathPreventer = new DeathPreventer(player);
        playerReviver = new PlayerReviver(player);
        if (playerName == Enums.NSHname)
        {
            nshInventory = new nsh.Inventory(player);
        }
        else if (playerName == Enums.SRSname)
        {
            srsLightSource = new srs.LightSourceModule(player);
        }
        else if (playerName == Enums.Moonname)
        {
            if (!world.game.IsArenaSession)
            {
                // 最费劲的一集
                swarmerManager = new moon.MoonSwarmer.SwarmerManager(player);
                deathPreventer.swarmerManager = swarmerManager;
                swarmerManager.deathPreventer = deathPreventer;
                swarmerManager.callBackSwarmers = world.game.GetDeathPersistent().MoonHasSwarmers;
                Plugin.Log("new game! moon has swarmers:", swarmerManager.callBackSwarmers, "last cycle swarmers:", world.game.GetDeathPersistent().MoonHasSwarmers);
                // gills = new PlayerGraphics.AxolotlGills(player.graphicsModule as PlayerGraphics, 13);
            }
            isMoon = true;
        }
        else if (playerName == Enums.FPname)
        {
            // pearlReader = new fp.PearlReader(player);
            gravityController = new(player);
            if (IsMyStory) daddy = new(player, world.game.GetStorySession.saveState.cycleNumber);
        }
        else if (playerName == Enums.test)
        {
            // 只是方便我测试
            isMoon = true;
        }
    }





    public void Update(Player player, bool eu)
    {


        if (player.SlugCatClass == Enums.Moonname) { isMoon = player.isRivulet; }
        if (spearExhaustCounter > 0) spearExhaustCounter--;
        pearlReader?.Update(eu);
        deathPreventer?.Update();
        playerReviver?.Update(eu);
        swarmerManager?.Update(eu);
        if (srsLightSource != null && srsLightSource.slatedForDeletion) { srsLightSource = null; }
        srsLightSource?.Update();
        daddy?.Update(eu);
        if (player.room == null || player.dead) return; // woc 这句话坑死我了
        nshInventory?.Update(eu);
        gravityController?.Update(eu);






        // 以下是对于玩家机动性做得一些调整，只要给ismoon赋值为true就可以使用
        if (!isMoon) return;
        if (player.animation == Player.AnimationIndex.Flip)
        {
            if (player.input[0].x != 0)
            {
                player.mainBodyChunk.vel.x += 1.5f * player.slugcatStats.runspeedFac / (player.input[0].x * (Mathf.Abs(player.mainBodyChunk.vel.x) + 0.5f));
            }
        }
        if (player.animation == Player.AnimationIndex.StandOnBeam)
        {
            // 加快你的速度，让能后空翻这事显得更合理（？
            if (player.input[0].x != 0)
            {
                player.mainBodyChunk.vel.x += Mathf.Sign(player.mainBodyChunk.vel.x) * 0.5f * player.slugcatStats.runspeedFac;
            }
            /*if (player.initSlideCounter > 10 && player.input[0].x != -player.slideDirection)
            {
                // 数学最有用的一集（。
                player.mainBodyChunk.vel.x += Mathf.Log10(player.initSlideCounter - 9) * player.slideDirection;
            }*/


            // slideCounter 就是mod显示的Turn 急转能量
            if (player.slideCounter > 0)
            {
                player.slideCounter++;
                // 上了杆子之后slidedirection不会变 还得修这个。。
                if (player.slideCounter > 20 || player.input[0].x != -player.slideDirection)
                {
                    player.slideCounter = 0;
                }
                float num = -Mathf.Sin((float)player.slideCounter / 20f * 3.1415927f * 0.5f) + 0.5f;
                player.mainBodyChunk.vel.x += (num * 3.5f * (float)player.slideDirection - (float)player.slideDirection * ((num < 0f) ? 0.8f : 0.5f) * (player.isSlugpup ? 0.25f : 1f));
                player.bodyChunks[1].vel.x += (num * 3.5f * (float)player.slideDirection + (float)player.slideDirection * 0.5f);
            }
            else if (player.input[0].x != 0)
            {
                if (player.input[0].x != player.slideDirection)
                {
                    if (player.initSlideCounter > (player.isRivulet? 10:15) && player.mainBodyChunk.vel.x > 0f == player.slideDirection > 0 && Mathf.Abs(player.mainBodyChunk.vel.x) > 0.8f)
                    {
                        player.slideCounter = 1;
                    }
                    else
                    {
                        player.slideDirection = player.input[0].x;
                    }
                    player.initSlideCounter = 0;
                    return;
                }
                if (player.initSlideCounter < 30)
                {
                    player.initSlideCounter++;
                    return;
                }
            }
            else if (player.initSlideCounter > 0)
            {
                player.initSlideCounter--;
                return;
            }

        }

    }




    public void NewRoom(Room newRoom)
    {
        gravityController?.NewRoom(newRoom);
        daddy?.NewRoom(newRoom);
    }



    public void LeaveRoom(Room oldRoom)
    {
        gravityController?.LeaveRoom(oldRoom);
        daddy?.LeaveRoom(oldRoom);
    }


    public IntVector2 PlayerInput(int x, int y)
    {
        if (gravityController != null && gravityController.KeyPressed)
        {
            gravityController.inputY = y;
            y = 0;
        }
        if (nshInventory != null && nshInventory.IsActive)
        {
            nshInventory.inputY = y;
            y = 0;
        }
        if (daddy != null && daddy.controlOfPlayer >= fp.DaddyModule.Control.CantWalk)
        {
            daddy.playerInput.x = x;
            daddy.playerInput.y = y;
            // 阴间小寄巧：每隔几帧就把input还给玩家一次，防止钻不进管道
            // 算了，这下鬼畜了
            /*if (Plugin.TickCount % 2 == 1)
            {
                x = 0;
                y = 0;
            }*/
        }
        return new(x, y);
    }

}

using MoreSlugcats;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.srs;


// 你说得对，但我是大sb
// 这真的是我学了三个月c#之后写的东西吗？看完感觉小脑萎缩了一下
// 喜报，重开了






public class SRSLightSource : LightSource, IProvideWarmth
{

    public LightSourceModule owner;
    public SRSLightSource(LightSourceModule owner) : base(owner.player.mainBodyChunk.pos, false, Color.Lerp(PlayerGraphicsModule.spearColor, PlayerGraphicsModule.bodyColor, 0.6f), owner.player)
    {
        this.owner = owner;
    }



    public Vector2 Position()
    {
        return owner.player.mainBodyChunk.pos;
    }

    public Room loadedRoom
    {
        get
        {
            return owner.player.room;
        }
    }


    public float warmth
    {
        get
        {
            return (owner.player.Malnourished ? 1 : Mathf.Lerp(3f, 1.5f, owner.player.Hypothermia)) * RainWorldGame.DefaultHeatSourceWarmth;
        }
    }

    public float range
    {
        get
        {
            return owner.LightSourceRad(0);
        }
    }
}







public class LightSourceModule
{
    public Player player;
    public LightSource[] lightSources;
    public string type;
    public int deletionCounter;
    public bool slatedForDeletion;


    




    public LightSourceModule(Player player)
    {
        this.player = player;
        AddModules();
    }



    // 真正添加自发光的代码。写的很乱，请看查找引用
    // warp模组传送之后会显示不出来，钻个管道就能解决，懒得管了（
    public void AddModules()
    {
        if (player.room == null || player.dead) { return; }
        deletionCounter = 0;
        lightSources = new LightSource[2]
        {
            new SRSLightSource(this)
            {
                requireUpKeep = true,
                setRad = new float?(player.Malnourished? 300f : 700f),
                setAlpha = new float?(player.Malnourished? 1f : 3f),
                fadeWithSun = false,
                affectedByPaletteDarkness = 0.1f,
            },
            new LightSource(player.mainBodyChunk.pos, false, Color.Lerp(PlayerGraphicsModule.spearColor, PlayerGraphicsModule.bodyColor, 0.9f), player)
            {
                requireUpKeep = true,
                setRad = new float?(100f),
                setAlpha = new float?(player.Malnourished? 0.1f : 0.2f),
                fadeWithSun = false,
                flat = true,
                affectedByPaletteDarkness = 0.3f,
            },
        };
        foreach (var source in lightSources)
        {
            player.room.AddObject(source);
        }
    }





    public void Update()
    {
        if (deletionCounter > 100)
        {
            slatedForDeletion = true;
            Clear();
            return;
        }

        if (lightSources == null)
        {
            AddModules();
        }
        else
        {
            if (player.dead) deletionCounter++;
            else
            {
                for (int i = 0; i < lightSources.Length; i++)
                {

                    if (lightSources[i] == null) continue;
                    if (lightSources[i].slatedForDeletetion)
                    {
                        lightSources[i] = null;
                        continue;
                    }
                    lightSources[i].stayAlive = true;
                    lightSources[i].setPos = new Vector2?(player.mainBodyChunk.pos);
                    lightSources[i].rad = LightSourceRad(i);
                    // 懂了 下面这句话才是让灯光跟随玩家的关键
                    if (lightSources[i].room != player.room)
                    {
                        lightSources[i].slatedForDeletetion = true;
                        lightSources = null;
                        return;
                    }
                }
            }
        }



        
 
        
        
    }


    public float LightSourceRad(int index)
    {
        if (index == 0)
        {
            return Mathf.Lerp(
                Mathf.Lerp(
                    player.Malnourished? 300f:700f, 
                    0f, 
                    0.25f * Mathf.Clamp(player.Hypothermia, 0f, 4f)), 
                0f, 
                deletionCounter * 0.01f);

        }
        else if (index == 1)
        {
            return Mathf.Lerp(
                Mathf.Lerp(
                    100f,
                    0f,
                    0.25f * Mathf.Clamp(player.Hypothermia, 0f, 4f)),
                0f,
                deletionCounter * 0.01f);
        }
        else { return 0f; }
    }

    public void Clear()
    {
        if (lightSources != null)
        {
            foreach (var source in lightSources)
            {
                if (source == null) continue;
                player.room.RemoveObject(source);
            }
            lightSources = null;
        }

    }





}

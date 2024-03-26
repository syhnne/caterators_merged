using MoreSlugcats;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.srs;


// 理论上这个可以给联机队友保温，但我还没试过冻死是什么感觉
// 啊 是时候改apply palette了（目死
// 妈的 这不起作用啊 咋回事
public class LightSourceModule : UpdatableAndDeletable, IProvideWarmth
{
    public Player player;
    public LightSource[] lightSources;
    public string type;


    public Vector2 Position()
    {
        return player.mainBodyChunk.pos;
    }

    public Room loadedRoom
    {
        get
        {
            return player.room;
        }
    }


    public float warmth
    {
        get
        {
            return (player.Malnourished ? 1 : 3) * RainWorldGame.DefaultHeatSourceWarmth;
        }
    }

    public float range
    {
        get
        {
            return player.Malnourished ? 300f : 700f;
        }
    }




    public LightSourceModule(Player player)
    {
        this.player = player;
        AddLightSource();
    }



    // 真正添加自发光的代码。写的很乱，请看查找引用
    // warp模组传送之后会显示不出来，钻个管道就能解决，懒得管了（
    public void AddLightSource()
    {
        if (player.room == null) { return; }

        lightSources = new LightSource[2]
        {
            new LightSource(player.mainBodyChunk.pos, false, Color.Lerp(PlayerGraphicsModule.spearColor, PlayerGraphicsModule.bodyColor, 0.6f), player)
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
        player.room.AddObject(this);
    }



    public void Update()
    {
        if (player.room == null) return;
        else if (lightSources == null) AddLightSource();
        for (int i = 0; i < lightSources.Length; i++)
        {
            if (lightSources[i] == null) continue;
            lightSources[i].stayAlive = true;
            lightSources[i].setPos = new Vector2?(player.mainBodyChunk.pos);
            if (lightSources[i].slatedForDeletetion)
            {
                lightSources[i] = null;
            }
        }
    }


    public void Clear()
    {
        foreach (var source in lightSources)
        {
            if (source == null) continue;
            player.room.RemoveObject(source);
        }
        lightSources = null;
        player.room.RemoveObject(this);
    }





}

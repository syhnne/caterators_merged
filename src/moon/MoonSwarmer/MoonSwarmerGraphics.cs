using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using System.Runtime.CompilerServices;

namespace Caterators_by_syhnne.moon.MoonSwarmer;

public class MoonSwarmerGraphics : GraphicsModule
{

    public MoonSwarmer swarmer;
    private Color blackColor;
    public float blackMode;

    public int colorLerpCounter = 0;
    public Color mainColor;
    public Color dotColor;
    public static Color dotColor2 = new Color(0.9f, 0.9f, 0.9f);

    public MoonSwarmerGraphics(MoonSwarmer ow, bool spawnColorLerp = false) : base(ow, false) 
    {
        this.swarmer = ow;
        this.owner = ow;
        // 好好好，我放弃，我不写了，凑活凑活吧
        // 现在他是这样的，如果我一上来就给所有的神经元全都写成绿的，那渐变色是能正常运行的，所有的都能
        // 但是加了判定之后，就变成所有的都不能了
        // 明明下面这行输出日志也有，但他就是不好使。不知道为什么。
        // 而且那个update函数似乎没有被调用
        if (spawnColorLerp)
        {
            Plugin.Log("spawnColorLerp");
            mainColor = nsh.ReviveSwarmerModules.NSHswarmerColor;
            dotColor = nsh.ReviveSwarmerModules.NSHswarmerColor;
            colorLerpCounter = 160;
        }
        else
        {
            mainColor = Color.white;
            dotColor = dotColor2;
        }
        /*mainColor = nsh.ReviveSwarmerModules.NSHswarmerColor;
        dotColor = nsh.ReviveSwarmerModules.NSHswarmerColor;
        if (spawnColorLerp )
        {
            colorLerpCounter = 200;
        }*/
    }




    public override void Update()
    {
        base.Update();
        
    }



    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[5];
        sLeaser.sprites[0] = new FSprite("Futile_White", true);
        sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
        sLeaser.sprites[0].scale = 1.5f;
        sLeaser.sprites[0].alpha = 0.2f;
        sLeaser.sprites[1] = new FSprite("JetFishEyeA", true);
        sLeaser.sprites[1].scaleY = 1.2f;
        sLeaser.sprites[1].scaleX = 0.75f;
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[2 + i] = new FSprite("deerEyeA2", true);
            sLeaser.sprites[2 + i].anchorX = 0f;
        }

        sLeaser.sprites[4] = new FSprite("JetFishEyeB", true);
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].color = mainColor;
        }
        sLeaser.sprites[4].color = dotColor;
        this.AddToContainer(sLeaser, rCam, null);
    }


    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            if (i == 0)
            {
                rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
            }
            else
            {
                newContainer.AddChild(sLeaser.sprites[i]);
            }
        }
    }


    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (this.blackMode < 1f)
        {
            this.blackMode = Mathf.Max(0f, this.blackMode - 1f / Mathf.Lerp(200f, 700f, Random.value));
        }
        // 这就是我不懂了，他好像压根就不管我给他设置的颜色，完全不知道为啥，难道我哪里还写了颜色的函数吗
        if (colorLerpCounter > 0)
        {
            colorLerpCounter--;
            mainColor = Color.Lerp(mainColor, Color.white, timeStacker);
            dotColor = Color.Lerp(dotColor, dotColor2, timeStacker);
            Plugin.Log("colorlerp:", colorLerpCounter, mainColor.ToString(), dotColor.ToString());
        }
        else if (colorLerpCounter == 0)
        {
            mainColor = Color.white;
            dotColor = dotColor2;
        }



        Vector2 vector = Vector2.Lerp(swarmer.firstChunk.lastPos, swarmer.firstChunk.pos, timeStacker);
        bool flag = rCam.room.ViewedByAnyCamera(vector, 48f);
        if (flag != swarmer.lastVisible)
        {
            for (int i = 0; i <= 4; i++)
            {
                sLeaser.sprites[i].isVisible = flag;
            }
            swarmer.lastVisible = flag;
        }
        if (!flag)
        {
            return;
        }
        Vector2 vector2 = Vector3.Slerp(swarmer.lastDirection, swarmer.direction, timeStacker);
        Vector2 vector3 = Vector3.Slerp(swarmer.lastLazyDirection, swarmer.lazyDirection, timeStacker);
        Vector3 vector4 = Custom.PerpendicularVector(vector2);
        float num = Mathf.Sin(Mathf.Lerp(swarmer.lastRotation, swarmer.rotation, timeStacker) * 3.1415927f * 2f);
        float num2 = Mathf.Cos(Mathf.Lerp(swarmer.lastRotation, swarmer.rotation, timeStacker) * 3.1415927f * 2f);
        sLeaser.sprites[0].x = vector.x - camPos.x;
        sLeaser.sprites[0].y = vector.y - camPos.y;
        sLeaser.sprites[1].x = vector.x - camPos.x;
        sLeaser.sprites[1].y = vector.y - camPos.y;
        sLeaser.sprites[4].x = vector.x + vector4.x * 2f * num2 * Mathf.Sign(num) - camPos.x;
        sLeaser.sprites[4].y = vector.y + vector4.y * 2f * num2 * Mathf.Sign(num) - camPos.y;
        sLeaser.sprites[1].rotation = Custom.VecToDeg(vector2);
        sLeaser.sprites[4].rotation = Custom.VecToDeg(vector2);
        sLeaser.sprites[4].scaleX = 1f - Mathf.Abs(num2);
        sLeaser.sprites[1].isVisible = true;
        for (int j = 0; j < 2; j++)
        {
            sLeaser.sprites[2 + j].x = vector.x - vector2.x * 4f - camPos.x;
            sLeaser.sprites[2 + j].y = vector.y - vector2.y * 4f - camPos.y;
            sLeaser.sprites[2 + j].rotation = Custom.VecToDeg(vector3) + 90f + ((j == 0) ? -1f : 1f) * Custom.LerpMap(Vector2.Distance(vector2, vector3), 0.06f, 0.7f, 10f, 45f, 2f) * num;
        }
        sLeaser.sprites[2].scaleY = -1f * num;
        sLeaser.sprites[3].scaleY = num;

        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].color = Color.Lerp(mainColor, this.blackColor, this.blackMode);
        }
        sLeaser.sprites[0].alpha = 0.2f * (1f - this.blackMode);
        sLeaser.sprites[4].color = Color.Lerp(dotColor, this.blackColor, this.blackMode * 0.9f);
        // Plugin.Log("3 color", sLeaser.sprites[3].color);

        if (swarmer.slatedForDeletetion || swarmer.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }


    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        /*Color color = new(1f, 1f, 1f);
        sLeaser.sprites[0].color = Color.Lerp(color, new Color(1f, 1f, 1f), 0.8f);
        sLeaser.sprites[1].color = color;*/
        this.blackColor = palette.blackColor;
    }










}

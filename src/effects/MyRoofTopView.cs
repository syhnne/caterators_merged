using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using MoreSlugcats;
using EffExt;

namespace Caterators_by_syhnne.effects;






// 这东西自打写出以来，我只见他运行过一次，就是显示了一些满屏幕乱飘的贴图，然后我退了一下游戏，后来他再也没显示过
public class MyRoofTopView : BackgroundScene
{
    public bool isRL;

    private float floorLevel = 26f;

    public Color atmosphereColor = new Color(0.16078432f, 0.23137255f, 0.31764707f);

    public BackgroundScene.Simple2DBackgroundIllustration daySky;

    public BackgroundScene.Simple2DBackgroundIllustration duskSky;

    public BackgroundScene.Simple2DBackgroundIllustration nightSky;

    private List<RoofTopView.DustWave> dustWaves;

    public EffectExtraData data;

    public MyRoofTopView(Room room, EffectExtraData data) : base(room)
    { 
        this.room = room;
        this.data = data;
        // floorLevel = data.GetFloat("floorLevel");
        isRL = (room.world.region != null && room.world.region.name == "RL") || room.abstractRoom.name.StartsWith("RL");

        if (isRL)
        {
            this.daySky = new BackgroundScene.Simple2DBackgroundIllustration(this, "AtC_Sky", new Vector2(683f, 384f));
            this.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(this, "AtC_DuskSky", new Vector2(683f, 384f));
            this.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(this, "AtC_NightSky", new Vector2(683f, 384f));
            this.AddElement(this.nightSky);
            this.AddElement(this.duskSky);
            this.AddElement(this.daySky);
            this.floorLevel = this.room.world.RoomToWorldPos(new Vector2(0f, 0f), this.room.abstractRoom.index).y - 30992.8f;
            this.floorLevel *= 22f;
            this.floorLevel = -this.floorLevel;
            float num3 = this.room.world.RoomToWorldPos(new Vector2(0f, 0f), this.room.abstractRoom.index).x - 11877f;
            num3 *= 0.01f;
            Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, this.atmosphereColor);
            Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
            Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, this.sceneOrigo);
            this.AddElement(new MyBuilding(this, "city2", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 200f - num3).x, this.floorLevel * 0.2f - 170000f), 420.5f, 2f));
            this.AddElement(new MyBuilding(this, "city1", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, this.floorLevel * 0.25f - 116000f), 340f, 2f));
            this.AddElement(new MyBuilding(this, "city3", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, this.floorLevel * 0.3f - 85000f), 260f, 2f));
            this.AddElement(new MyBuilding(this, "city2", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 40f - num3 * 0.5f).x, this.floorLevel * 0.35f - 42000f), 180f, 2f));
            this.AddElement(new MyBuilding(this, "city1", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 90f - num3 * 0.2f).x, this.floorLevel * 0.4f + 5000f), 100f, 2f));
        }




        /*this.sceneOrigo = base.RoomToWorldPos(room.abstractRoom.size.ToVector2() * 10f);
        room.AddObjectToTop(new RoofTopView.DustpuffSpawner());
        this.daySky = new BackgroundScene.Simple2DBackgroundIllustration(this, "Rf_Sky", new Vector2(683f, 384f));
        this.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(this, "Rf_DuskSky", new Vector2(683f, 384f));
        this.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(this, "Rf_NightSky", new Vector2(683f, 384f));

        if (this.room.dustStorm)
        {
            this.dustWaves = new List<RoofTopView.DustWave>();
            float num = 2500f;
            float num2 = 0f;
            this.dustWaves.Add(new RoofTopView.DustWave(this, "RF_CityA_DM", new Vector2(base.PosFromDrawPosAtNeutralCamPos(Vector2.zero, -num2).x, this.floorLevel / 4f - num * 40f), 370f, 0f));
            this.dustWaves.Add(new RoofTopView.DustWave(this, "RF_CityA_DM", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(600f, 0f), -num2).x, this.floorLevel / 5f - num * 30f), 290f, 0f));
            this.dustWaves.Add(new RoofTopView.DustWave(this, "RF_CityA_DM", new Vector2(base.PosFromDrawPosAtNeutralCamPos(Vector2.zero, -num2).x, this.floorLevel / 6f - num * 20f), 210f, 0f));
            this.dustWaves.Add(new RoofTopView.DustWave(this, "RF_CityA_DM", new Vector2(base.PosFromDrawPosAtNeutralCamPos(Vector2.zero, -num2).x, this.floorLevel / 7f - num * 10f), 130f, 0f));
            RoofTopView.DustWave dustWave = new RoofTopView.DustWave(this, "RF_CityA_DM", new Vector2(base.PosFromDrawPosAtNeutralCamPos(Vector2.zero, -num2).x, this.floorLevel / 8f), 50f, 0f);
            dustWave.isTopmost = true;
            this.dustWaves.Add(dustWave);
            foreach (RoofTopView.DustWave element in this.dustWaves)
            {
                this.AddElement(element);
            }
        }
        if (this.isRL)
        {
            this.daySky = new BackgroundScene.Simple2DBackgroundIllustration(this, "AtC_Sky", new Vector2(683f, 384f));
            this.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(this, "AtC_DuskSky", new Vector2(683f, 384f));
            this.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(this, "AtC_NightSky", new Vector2(683f, 384f));
            this.AddElement(this.nightSky);
            this.AddElement(this.duskSky);
            this.AddElement(this.daySky);
            this.floorLevel = this.room.world.RoomToWorldPos(new Vector2(0f, 0f), this.room.abstractRoom.index).y - 30992.8f;
            this.floorLevel *= 22f;
            this.floorLevel = -this.floorLevel;
            float num3 = this.room.world.RoomToWorldPos(new Vector2(0f, 0f), this.room.abstractRoom.index).x - 11877f;
            num3 *= 0.01f;
            Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, this.atmosphereColor);
            Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
            Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, this.sceneOrigo);
            this.AddElement(new RoofTopView.Building(this, "city2", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 200f - num3).x, this.floorLevel * 0.2f - 170000f), 420.5f, 2f));
            this.AddElement(new RoofTopView.Building(this, "city1", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, this.floorLevel * 0.25f - 116000f), 340f, 2f));
            this.AddElement(new RoofTopView.Building(this, "city3", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, this.floorLevel * 0.3f - 85000f), 260f, 2f));
            this.AddElement(new RoofTopView.Building(this, "city2", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 40f - num3 * 0.5f).x, this.floorLevel * 0.35f - 42000f), 180f, 2f));
            this.AddElement(new RoofTopView.Building(this, "city1", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 90f - num3 * 0.2f).x, this.floorLevel * 0.4f + 5000f), 100f, 2f));
            this.AddElement(new RoofTopView.Floor(this, "floor", new Vector2(0f, this.floorLevel * 0.2f - 90000f), 400.5f, 500.5f));
            return;
        }
        this.AddElement(this.nightSky);
        this.AddElement(this.duskSky);
        this.AddElement(this.daySky);
        Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
        this.AddElement(new RoofTopView.Floor(this, "floor", new Vector2(0f, this.floorLevel), 1f, 12f));
        Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, this.atmosphereColor);
        Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, this.sceneOrigo);
        for (int i = 0; i < 16; i++)
        {
            float num4 = (float)i / 15f;
            this.AddElement(new RoofTopView.Rubble(this, "Rf_Rubble", new Vector2(0f, this.floorLevel), Mathf.Lerp(1.5f, 8f, Mathf.Pow(num4, 1.5f)), i));
        }
        this.AddElement(new RoofTopView.DistantBuilding(this, "Rf_HoleFix", new Vector2(-2676f, 9f), 1f, 0f));

        this.AddElement(new RoofTopView.DistantBuilding(this, "RF_CityA_DM", new Vector2(base.PosFromDrawPosAtNeutralCamPos(Vector2.zero, 8.5f).x, this.floorLevel - 25.5f), 8.5f, 0f));
        this.AddElement(new RoofTopView.DistantBuilding(this, "RF_CityB_DM", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(215f, 0f), 6.5f).x, this.floorLevel - 13f), 6.5f, 0f));
        this.AddElement(new RoofTopView.DistantBuilding(this, "RF_CityC_DM", new Vector2(base.PosFromDrawPosAtNeutralCamPos(new Vector2(100f, 0f), 5f).x, this.floorLevel - 8.5f), 5f, 0f));
        base.LoadGraphic("smoke1", false, false);
        this.AddElement(new RoofTopView.Smoke(this, new Vector2(0f, this.floorLevel + 560f), 7f, 0, 2.5f, 0.1f, 0.8f, false));
        this.AddElement(new RoofTopView.Smoke(this, new Vector2(0f, this.floorLevel), 4.2f, 0, 0.2f, 0.1f, 0f, true));
        this.AddElement(new RoofTopView.Smoke(this, new Vector2(0f, this.floorLevel + 28f), 2f, 0, 0.5f, 0.1f, 0f, true));
        this.AddElement(new RoofTopView.Smoke(this, new Vector2(0f, this.floorLevel + 14f), 1.2f, 0, 0.75f, 0.1f, 0f, true));*/
    }



    public float AtmosphereColorAtDepth(float depth)
    {
        return Mathf.Clamp(depth / 15f, 0f, 1f) * 0.9f;
    }


    public override void Update(bool eu)
    {
        base.Update(eu);
        // floorLevel = data.GetFloat("floorLevel");
        if ((this.room.game.cameras[0].effect_dayNight > 0f && this.room.world.rainCycle.timer >= this.room.world.rainCycle.cycleLength) || (ModManager.Expedition && this.room.game.rainWorld.ExpeditionMode))
        {
            float num = 1320f;
            float num2 = (float)this.room.world.rainCycle.dayNightCounter / num;
            float num3 = ((float)this.room.world.rainCycle.dayNightCounter - num) / num;
            float num4 = ((float)this.room.world.rainCycle.dayNightCounter - num) / (num * 1.25f);
            Color color = new(0.16078432f, 0.23137255f, 0.31764707f);
            Color color2 = new(0.0627451f, 0.38431373f, 0.3019608f);
            Color color3 = new(0.04882353f, 0.0527451f, 0.06843138f);
            Color color4 = new(0.75686276f, 0.46666667f, 0.45882353f);
            Color color5 = new(0.078431375f, 0.14117648f, 0.21176471f);
            if (ModManager.MSC && this.room.game.IsStorySession && this.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                color2 = new(0.78039217f, 0.41568628f, 0.39607844f);
                color4 = new(1f, 0.79f, 0.47f);
            }
            Color? color6 = null;
            Color? color7 = null;
            if (num2 > 0f && num2 < 1f)
            {
                this.daySky.alpha = 1f - num2;
                color6 = new Color?(Color.Lerp(color, color2, num2));
                color7 = new Color?(Color.Lerp(Color.white, color4, num2));
            }
            if (num2 >= 1f)
            {
                this.daySky.alpha = 0f;
                if (num3 > 0f && num3 < 1f)
                {
                    this.duskSky.alpha = 1f - num3;
                    color6 = new Color?(Color.Lerp(color2, color3, num3));
                }
                if (num3 >= 1f)
                {
                    this.duskSky.alpha = 0f;
                    color6 = new Color?(color3);
                }
                if (num4 > 0f && num4 < 1f)
                {
                    color7 = new Color?(Color.Lerp(color4, color5, num4));
                }
                if (num4 >= 1f)
                {
                    color7 = new Color?(color5);
                }
            }
            if (color6 != null)
            {
                this.atmosphereColor = color6.Value;
                Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, this.atmosphereColor);
            }
            if (color7 != null)
            {
                Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, color7.Value);
            }
        }
    }



    public override void Destroy()
    {
        base.Destroy();
    }












    public class MyBuilding : BackgroundScene.BackgroundSceneElement
    {

        private float scale;
        private Vector2 elementSize;
        private string assetName;

        public MyBuilding(MyRoofTopView owner, string assetName, Vector2 pos, float depth, float scale) : base(owner, pos, depth)
        {
            this.depth = depth;
            this.scale = scale;
            this.assetName = assetName;
            owner.LoadGraphic(assetName, false, true);
            this.elementSize = Futile.atlasManager.GetElementWithName(assetName).sourceSize;
        }


        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite(this.assetName, true);
            sLeaser.sprites[0].scale = this.scale;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["House"];
            sLeaser.sprites[0].anchorY = 0f;
            if ((this.scene as MyRoofTopView).isRL)
            {
                sLeaser.sprites[0].anchorY = -0.02f;
                sLeaser.sprites[0].anchorX = 0.5f;
            }
            this.AddToContainer(sLeaser, rCam, null);
        }


        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = base.DrawPos(camPos, rCam.hDisplace);
            sLeaser.sprites[0].x = vector.x;
            sLeaser.sprites[0].y = vector.y;
            sLeaser.sprites[0].color = new Color(this.elementSize.x * this.scale / 4000f, this.elementSize.y * this.scale / 1500f, 1f / this.depth);
            if ((this.scene as MyRoofTopView).isRL)
            {
                sLeaser.sprites[0].scale = 5f;
                sLeaser.sprites[0].color = new Color(this.elementSize.x * this.scale / 4000f, this.elementSize.y * this.scale / 1500f, 1f / (this.depth / 20f));
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }


    }















}

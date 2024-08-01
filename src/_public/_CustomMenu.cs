using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Drawing.Text;
using RWCustom;
using Noise;
using System.Collections.Generic;
using MoreSlugcats;
using BepInEx.Logging;
using Smoke;
using Random = UnityEngine.Random;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using System.Runtime.CompilerServices;
using Menu;
using static Caterators_by_syhnne.srs.OxygenMaskModules;
using SlugBase.SaveData;
using SlugBase;
using SlugBase.Assets;

namespace Caterators_by_syhnne._public;

public class CustomMenu
{

    public readonly static MenuScene.SceneID moonGhostScene = new("Ghost_moon");
    /*public MenuScene.SceneID moonSlugcat1 = new("Slugcat_moon_1");
    public MenuScene.SceneID moonSlugcat2 = new("Slugcat_moon_2");
    public MenuScene.SceneID moonSlugcat3 = new("Slugcat_moon_3");
    public MenuScene.SceneID moonSlugcat4 = new("Slugcat_moon_4");
    public MenuScene.SceneID moonSlugcat5 = new("Slugcat_moon_5");*/


    public static void Apply()
    {
        On.Menu.SlugcatSelectMenu.SlugcatPage.AddAltEndingImage += SlugcatPage_AddAltEndingImage;
        On.Menu.SlideShow.ctor += SlideShow_ctor;
        On.Menu.SlugcatSelectMenu.SlugcatPage.AddImage += SlugcatSelectMenu_SlugcatPage_AddImage;

        fp.CustomMenu.Apply();
    }



    



    private static void SlugcatSelectMenu_SlugcatPage_AddImage(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_AddImage orig, Menu.SlugcatSelectMenu.SlugcatPage self, bool ascended)
    {
        
        if (self.slugcatNumber == Enums.Moonname)
        {
            // 等别人的自定义界面做好了再搬过去
            self.imagePos = new Vector2(683f, 484f);
            self.sceneOffset = default(Vector2);
            self.slugcatDepth = 1f;
            MenuScene.SceneID sceneID = MenuScene.SceneID.Slugcat_White;
            CustomSaveData.SaveMiscProgression misc = self.menu.manager.rainWorld.progression.miscProgressionData.GetMiscProgression();
            if (ascended)
            {
                sceneID = moonGhostScene;
            }
            else if (misc.MoonSwarmers > 0)
            {
                sceneID = moon.CustomMenu.MoonSlugcatScene(misc.MoonSwarmers);
            }
            else
            {
                sceneID = moon.CustomMenu.MoonSlugcatScene(5);
            }
            if (CustomScene.Registry.TryGet(sceneID, out var customScene))
            {
                self.markOffset = customScene.MarkPos ?? self.markOffset;
                self.glowOffset = customScene.GlowPos ?? self.glowOffset;
                self.sceneOffset = customScene.SelectMenuOffset ?? self.sceneOffset;
                self.slugcatDepth = customScene.SlugcatDepth ?? self.slugcatDepth;
            }








            self.sceneOffset.x = self.sceneOffset.x - (1366f - self.menu.manager.rainWorld.options.ScreenSize.x) / 2f;
            self.slugcatImage = new InteractiveMenuScene(self.menu, self, sceneID);
            self.subObjects.Add(self.slugcatImage);
            if (self.HasMark)
            {
                self.markSquare = new FSprite("pixel", true);
                self.markSquare.scale = 14f;
                self.markSquare.color = Color.Lerp(self.effectColor, Color.white, 0.7f);
                self.Container.AddChild(self.markSquare);
                self.markGlow = new FSprite("Futile_White", true);
                self.markGlow.shader = self.menu.manager.rainWorld.Shaders["FlatLight"];
                self.markGlow.color = self.effectColor;
                self.Container.AddChild(self.markGlow);
            }
        }
        else { orig(self, ascended); }
    }



    private static void SlideShow_ctor(On.Menu.SlideShow.orig_ctor orig, SlideShow self, ProcessManager manager, SlideShow.SlideShowID slideShowID)
    {
        orig(self, manager, slideShowID);
        fp.CustomLore.SlideShow_ctor(self, manager, slideShowID);
    }


    private static void SlugcatPage_AddAltEndingImage(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_AddAltEndingImage orig, SlugcatSelectMenu.SlugcatPage self)
    {
        if (self.slugcatNumber == Enums.FPname)
        {
            fp.CustomLore.SlugcatPage_AddAltEndingImage(self);
        }
        else { orig(self); }
    }

}

using Menu.Remix.MixedUI;
using RWCustom;
using System.Xml.Linq;
using UnityEngine;
using System;

namespace Caterators_by_syhnne;

// TODO: 显示啊！tnnd，为什么不显示！！
internal class Options : OptionInterface
{
    
 


    internal readonly int DefaultExplosionCapacity = 10;
    public static Configurable<int> ExplosionCapacity;
    public Configurable<KeyCode> GravityControlKey;
    public Configurable<KeyCode> InventoryKey;
    public Configurable<KeyCode> CraftKey;
    public static Configurable<bool> RetrieveSlugFix;
    public static Configurable<bool> DevMode;

    public Options()
    {
        ExplosionCapacity = config.Bind<int>("ExplosionCapacity", 10, new ConfigurableInfo("(FP)The maximum number of subsequent explosion actions player can perform before dying", new ConfigAcceptableRange<int>(1, 999), "", "Explosion Capacity"));
        GravityControlKey = config.Bind<KeyCode>("GravityControlKey", KeyCode.G);
        CraftKey = config.Bind<KeyCode>("CraftKey", KeyCode.None);
        InventoryKey = config.Bind<KeyCode>("InventoryKey", KeyCode.D);
        RetrieveSlugFix = config.Bind<bool>("RetrieveSlugFix", true);
        DevMode = config.Bind<bool>("DevMode", false);
    }

    public override void Initialize()
    {
        base.Initialize();
        InGameTranslator inGameTranslator = Custom.rainWorld.inGameTranslator;
        float yspacing = 50f;
        float xposLabel = 20f;
        float xposOpt = 200f;
        float xmax = 600f;
        float ymax = 600f;

        Tabs = new OpTab[]
        {
            new OpTab(this, "General"),
            new OpTab(this, "Slugcat Specific")
        };

        string desc = "The key to be pressed when controlling gravity";
        float ypos = 50f;
        Tabs[0].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - ypos, inGameTranslator.Translate("Gravity control key"), false)
            { description = inGameTranslator.Translate(desc) },
            new OpKeyBinder(GravityControlKey, new Vector2(xposOpt, ymax - yspacing - ypos), new Vector2(50f, 10f), true, OpKeyBinder.BindController.AnyController)
            { description = inGameTranslator.Translate(desc) }
        );

        desc = "Developer Mode (unlock all characters, enables gravity control, disables some functions that make the game harder, etc)";
        ypos += 50f;
        Tabs[0].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - ypos, inGameTranslator.Translate("DevMode"), false)
            { description = inGameTranslator.Translate(desc) },
            new OpCheckBox(DevMode, new Vector2(xposOpt, ymax - yspacing - ypos))
            { description = inGameTranslator.Translate(desc) }
        );

        ypos += 120f;
        Tabs[0].AddItems(
            new OpLabelLong(new Vector2(xposLabel, ymax - yspacing - ypos), new Vector2(xmax - 100, 100), "  DevMode keybinds:<LINE>Y - spawn a green swarmer<LINE>U - Pause rain timer<LINE>T - Spawn a swarmer for moon if possible<LINE>H - Log all swarmers position<LINE>J - Teleport all swarmers to player<LINE>")
        );


        desc = "(FP)The maximum number of subsequent explosion actions player can perform before dying";
        ypos = 50f;
        try
        {
            OpUpdown opUpDown = new OpUpdown(ExplosionCapacity, new Vector2(xposOpt, ymax - yspacing - ypos), 60f) { description = inGameTranslator.Translate(desc) };
            opUpDown.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
            opUpDown.SetNextFocusable(UIfocusable.NextDirection.Right, opUpDown);

            Tabs[1].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - ypos, inGameTranslator.Translate("(FP)Explosion capacity"))
            { description = inGameTranslator.Translate(desc) },
            opUpDown
            /*new OpSlider(ExplosionCapacity, new Vector2(xposOpt, ymax - yspacing - ypos), 360, false)
            {
                min = 5,
                max = 20,
                description = inGameTranslator.Translate(desc)
            }*/
            );
        }
        catch (Exception e)
        {
            Plugin.LogException(e);
        }

        desc = "(FP)The key to be pressed when crafting electric spears (if unspecified, hold [pickup] to craft)";
        ypos += 50f;
        Tabs[1].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - ypos, inGameTranslator.Translate("(FP)Crafting key"), false)
            { description = inGameTranslator.Translate(desc) },
            new OpKeyBinder(CraftKey, new Vector2(xposOpt, ymax - yspacing - ypos), new Vector2(50f, 10f), true, OpKeyBinder.BindController.AnyController)
            { description = inGameTranslator.Translate(desc) }
        );

        desc = "(NSH)The key to be pressed when adding/removing objects from inventory";
        ypos += 50f;
        Tabs[1].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - ypos, inGameTranslator.Translate("(NSH)Inventory key"), false)
            { description = inGameTranslator.Translate(desc) },
            new OpKeyBinder(InventoryKey, new Vector2(xposOpt, ymax - yspacing - ypos), new Vector2(50f, 10f), true, OpKeyBinder.BindController.AnyController)
            { description = inGameTranslator.Translate(desc) }
        );

        // 这个我懒得写了 要不直接认为是true吧 我想正常玩家放下背上的猫崽/联机玩家都是用拾取+下键的
        /*desc = "(SRS)Prevent slugcat on back from being retrieved when making a needle (that bugged me all the time so i think you might need it)";
        ypos += 50f;
        Tabs[1].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - ypos, inGameTranslator.Translate("RetrieveSlugFix"), false)
            { description = inGameTranslator.Translate(desc) },
            new OpCheckBox(RetrieveSlugFix, new Vector2(xposOpt, ymax - yspacing - ypos))
            { description = inGameTranslator.Translate(desc) }
        );*/

    }

}
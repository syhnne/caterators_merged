using Menu.Remix.MixedUI;
using RWCustom;
using System.Xml.Linq;
using UnityEngine;

namespace Caterators_by_syhnne;


internal class ConfigOptions : OptionInterface
{
    public static ConfigOptions Instance { get; } = new();
    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
    }


    internal readonly int DefaultExplosionCapacity = 10;
    public static Configurable<int> ExplosionCapacity;
    public Configurable<KeyCode> GravityControlKey;
    public Configurable<KeyCode> CraftKey;
    public ConfigOptions()
    {
        ExplosionCapacity = config.Bind<int>("ExplosionCapacity", 10);
        GravityControlKey = config.Bind<KeyCode>("GravityControlKey", KeyCode.G);
        CraftKey = config.Bind<KeyCode>("CraftKey", KeyCode.None);
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
        Tabs[0].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - 100f, inGameTranslator.Translate("Gravity control key"), false)
            { description = desc },
            new OpKeyBinder(GravityControlKey, new Vector2(xposOpt, ymax - yspacing - 100f), new Vector2(50f, 10f), true, OpKeyBinder.BindController.AnyController)
            { description = desc }
        );

        desc = "Explosion capacity ";
        Tabs[1].AddItems(
            new OpLabel(xposLabel, ymax - yspacing, inGameTranslator.Translate("Explosion capacity"))
            { description = inGameTranslator.Translate(desc) },
            new OpSlider(ExplosionCapacity, new Vector2(xposOpt, ymax - yspacing), 360, false)
            {
                min = 5,
                max = 20,
                defaultValue = DefaultExplosionCapacity.ToString(),
                description = inGameTranslator.Translate(desc)
            }
        );

        desc = "(FP)The key to be pressed when crafting electric spears (if unspecified, hold [pickup] to craft)";
        Tabs[1].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - 100f, inGameTranslator.Translate("Crafting key"), false)
            { description = inGameTranslator.Translate(desc) },
            new OpKeyBinder(CraftKey, new Vector2(xposOpt, ymax - yspacing - 100f), new Vector2(50f, 10f), true, OpKeyBinder.BindController.AnyController)
            { description = inGameTranslator.Translate(desc) }
        );

    }

}
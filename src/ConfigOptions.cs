using Menu.Remix.MixedUI;
using RWCustom;
using System.Xml.Linq;
using UnityEngine;

namespace Caterators_by_syhnne;

// TODO: 显示啊！tnnd，为什么不显示！！
internal class ConfigOptions : OptionInterface
{
    
 


    internal readonly int DefaultExplosionCapacity = 10;
    public static Configurable<int> ExplosionCapacity;
    public Configurable<KeyCode> GravityControlKey;
    public Configurable<KeyCode> InventoryKey;
    public Configurable<KeyCode> CraftKey;
    public static Configurable<bool> RetrieveSlugFix;

    public ConfigOptions()
    {
        ExplosionCapacity = config.Bind<int>("ExplosionCapacity", DefaultExplosionCapacity);
        GravityControlKey = config.Bind<KeyCode>("GravityControlKey", KeyCode.G);
        CraftKey = config.Bind<KeyCode>("CraftKey", KeyCode.None);
        InventoryKey = config.Bind<KeyCode>("InventoryKey", KeyCode.D);
        RetrieveSlugFix = config.Bind<bool>("RetrieveSlugFix", true);
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

        desc = "(FP)Explosion capacity ";
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

        desc = "(NSH)The key to be pressed when adding/removing objects from inventory";
        Tabs[1].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - 150f, inGameTranslator.Translate("Inventory key"), false)
            { description = desc },
            new OpKeyBinder(InventoryKey, new Vector2(xposOpt, ymax - yspacing - 150f), new Vector2(50f, 10f), true, OpKeyBinder.BindController.AnyController)
            { description = desc }
        );

        // 这个我懒得写了 要不直接认为是true吧 我想正常玩家放下背上的猫崽/联机玩家都是用拾取+下键的
        /*desc = "(SRS)Prevent slugcat on back from being retrieved when making a needle (that bugged me all the time so i think you might need it)";
        Tabs[1].AddItems(
            new OpLabel(xposLabel, ymax - yspacing - 200f, inGameTranslator.Translate("RetrieveSlugFix"), false)
            { description = desc },
            new OpCheckBox(RetrieveSlugFix, new Vector2(xposOpt, ymax - yspacing - 200f))
            { description = desc }
        );*/

    }

}
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne;



// 抄的珍珠猫代码
// 呃啊 原来slugbase有这个功能啊（晕厥）我都不会用 看document看不懂 不抄不知道
// 虽然我屡次三番想要抄珍珠猫代码，但今天是我第一次看懂这个代码。。
public static class CustomSaveData
{


    public static void Apply()
    {
        On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk;
        On.SaveState.LoadGame += SaveState_LoadGame;
    }



    private static bool PlayerProgression_SaveToDisk(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
    {


        // 哇 这是什么写法 好神奇 马上给isCaterator安排一个
        var miscProg = self.miscProgressionData?.GetMiscProgression();
        var ssn = self.currentSaveState?.saveStateNumber;
        if ( miscProg != null && ssn != null && saveCurrentState && self.currentSaveState != null && self.currentSaveState.IsCaterator())
        {
            if (ssn == Enums.FPname)
            {
                // TODO: 这是我为了防止在虚空海里按10分钟V键拖拽蛞蝓猫只为找到那个触发结局的亮光在哪而做的紧急避险措施，发布的时候记得改回来
                miscProg.beaten_fp = self.currentSaveState.deathPersistentSaveData.altEnding || self.currentSaveState.deathPersistentSaveData.ascended;
            }
            else if (ssn == Enums.SRSname)
            {
                miscProg.beaten_srs = self.currentSaveState.deathPersistentSaveData.altEnding || self.currentSaveState.deathPersistentSaveData.ascended;
            }
            else if (ssn == Enums.NSHname)
            {
                miscProg.beaten_nsh = self.currentSaveState.deathPersistentSaveData.altEnding || self.currentSaveState.deathPersistentSaveData.ascended;
            }
            else if (ssn == Enums.Moonname)
            {
                miscProg.beaten_moon = self.currentSaveState.deathPersistentSaveData.altEnding || self.currentSaveState.deathPersistentSaveData.ascended;
            }
            Plugin.Log("miscProg:", miscProg.beaten_fp, miscProg.beaten_srs, miscProg.beaten_nsh, miscProg.beaten_moon);
        }

        return orig(self, saveCurrentState, saveMaps, saveMiscProg);
    }



    private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        var miscProg = self.progression.miscProgressionData?.GetMiscProgression();
        return orig(self);
    }

    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);
        var miscProg = self.progression.miscProgressionData.GetMiscProgression();
        miscProg.StoryCharacter = self.saveStateNumber;

    }




    // 我焯 这么方便
    // 大概就是，slugbase有一个和MiscProgression绑定的data列表，你只要随便新建一个类，在这个类里面写满你需要用的数据
    // 然后把这个类作为value，把你自己写的随便一个什么字符串（但最好是modid）作为key，存进这个slugbasedata里面，用的时候取就行了
    // 呃啊。。彻底明白了劝学里面那句“君子生非异也，善假于物也”是什么意思。。
    public class SaveMiscProgression
    {
        public SlugcatStats.Name StoryCharacter {  get; set; }
        public bool IsNewSave {  get; set; }
        public bool beaten_fp { get; set; }
        public bool alt2ending_fp {  get; set; }
        public bool beaten_srs {  get; set; }
        public bool alt2ending_srs { get; set; }
        public bool beaten_nsh { get; set; }
        public bool alt2ending_nsh { get; set; }
        public bool beaten_moon {  get; set; }
        public bool alt2ending_moon { get; set; }

    }

    
    public static SaveMiscProgression GetMiscProgression(this RainWorld rainWorld) => GetMiscProgression(rainWorld.progression.miscProgressionData);
    public static SaveMiscProgression GetMiscProgression(this RainWorldGame game) => GetMiscProgression(game.rainWorld.progression.miscProgressionData);
    public static SaveMiscProgression GetMiscProgression(this PlayerProgression.MiscProgressionData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveMiscProgression save))
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());

        return save;
    }

    public class SaveDeathPersistent
    {
        public int CyclesFromLastEnterSSAI {  get; set; }
        public bool OxygenMaskTaken {  get; set; }
        public bool alt2ending_fp { get; set; }
        public bool alt2ending_srs { get; set; }
        public bool alt2ending_nsh { get; set; }
        public bool alt2ending_moon { get; set; }

        public string[] NSHInventoryStrings { get; set; }

    }


    public static SaveDeathPersistent GetDeathPersistent(this RainWorldGame game) => GetDeathPersistent(game.GetStorySession.saveState.deathPersistentSaveData);
    public static SaveDeathPersistent GetDeathPersistent(this DeathPersistentSaveData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveDeathPersistent save))
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());

        return save;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Caterators_merged;


public class CustomDeathPersistentSaveData
{

    public string TotalHeader = Plugin.MOD_ID;
    public SlugcatStats.Name saveStateNumber;

    // 这样调用起来会方便点吗（
    public int CyclesFromLastEnterSSAI = 0;
    public bool TestData = false;
    public List<string> saveStrings = new List<string>()
    {
        nameof(CyclesFromLastEnterSSAI),

    };


    public CustomDeathPersistentSaveData(SlugcatStats.Name saveStateNumber)
    {
        this.saveStateNumber = saveStateNumber;
    }


    public void ClearData(SlugcatStats.Name newName)
    {
        saveStateNumber = newName;
        CyclesFromLastEnterSSAI = 0;
        TestData = false;
    }




    public string SaveToString(string res)
    {
        Plugin.LogStat("DPSaveData - SaveToString", CyclesFromLastEnterSSAI);
        res += TotalHeader + nameof(CyclesFromLastEnterSSAI) + "<dpB>" + CyclesFromLastEnterSSAI.ToString() + "<dpA>";
        // res += TotalHeader + saveStrings[1] + "<dpB>" + TestData.ToString() + "<dpA>";





        return res;
    }



    // 这确实是个烂方法，好在要存的数据不多，暂且复制粘贴一下罢
    public void FromString(List<string> datas)
    {
        foreach (var d in datas)
        {

            string[] data = Regex.Split(d, "<dpB>");


            if (data[0].Contains(nameof(CyclesFromLastEnterSSAI)))
            {
                CyclesFromLastEnterSSAI = int.Parse(data[1]);
                // Plugin.Log("data:", nameof(CyclesFromLastEnterSSAI), CyclesFromLastEnterSSAI);
            }

        }

    }


}


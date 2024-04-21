using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RWCustom;
using MoreSlugcats;
using Random = UnityEngine.Random;
using UnityEngine;
using HUD;
using CustomRegions;

namespace Caterators_by_syhnne.fp;



// 该来的还是会来的。。是时候使用传说中的弱依赖了
// 啊 不对 我本来就有crs依赖项 那没事了（


/// <summary>
/// 自定义珍珠的话，我就只能把月姐的对话给你端上来了
/// </summary>
public class PearlReader
{

    public Player player;
    public int holdingPearlCounter;
    public const int StartCounter = 90;

    public bool chatlog;
    public int chatlogCounter;
    public List<string> chatlogList;

    public Vector2 pearlHoverPos;
    public DataPearl pearlHeld;
    
    public PearlReader(Player player)
    {
        Plugin.Log("new PearlReader");
        this.player = player;
    }



    public void Update(bool eu)
    {
        try
        {
            if (player.room == null) { return; }
            DataPearl pearl = null;

            // 有且仅有拾取键和上键被按住
            if (player.input[0].x == 0 && player.input[0].y == 1 && player.input[0].pckp && !player.input[0].thrw && !player.input[0].jmp
                && ((player.grasps[0] != null && player.grasps[0].grabbed is DataPearl) || (player.grasps[1] != null && player.grasps[1].grabbed is DataPearl)))
            {
                // Plugin.Log("holdingPearlCounter:", holdingPearlCounter);
                holdingPearlCounter++;
                foreach (var obj in player.grasps)
                {
                    if (obj.grabbed != null && obj.grabbed is DataPearl)
                    {
                        pearl = obj.grabbed as DataPearl;
                        break;
                    }
                }
            }
            else
            {
                holdingPearlCounter = 0;
            }

            if (holdingPearlCounter > StartCounter && pearl != null)
            {
                holdingPearlCounter = 0;
                InitChatlog(pearl);
            }
            ProcessChatlog(eu);


            // Plugin.Log("stun:", player.stun);

        }
        catch (Exception e)
        {
            Plugin.LogException(e);
        }


    }






    public void InitChatlog(DataPearl pearl)
    {
        Plugin.Log("init chatlog:");
        pearlHeld = pearl;
        pearlHoverPos = pearl.firstChunk.pos;
        chatlog = true;
        chatlogList = DataPearlToText(pearl);
        chatlogCounter = 0;
        for (int i = 0; i < player.bodyChunks.Length; i++)
        {
            player.bodyChunks[i].vel = Vector2.zero;
        }
    }





    // 那么问题来了，如果有人去读赞美诗珍珠，我是应该给他把解读文案端上来呢，还是应该放音乐呢
    public void ProcessChatlog(bool eu)
    {
        if (pearlHeld != null && (holdingPearlCounter > 0 || chatlog))
        {
            pearlHeld.firstChunk.vel *= Custom.LerpMap(pearlHeld.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
            pearlHeld.firstChunk.vel += Vector2.ClampMagnitude((pearlHoverPos + new Vector2(0, 20f * Mathf.Sin(0.2f * chatlogCounter))) - pearlHeld.firstChunk.pos, 100f) / 100f * 0.4f;
            pearlHeld.gravity = 0f;
            // pearlHeld.firstChunk.MoveFromOutsideMyUpdate(eu, pearlHoverPos);
        }

        if (chatlog && player.room != null)
        {
            player.mushroomCounter = 25;
            player.Stun(25);
            if (ModManager.CoopAvailable)
            {
                foreach (AbstractCreature abstractCreature in player.abstractCreature.world.game.AlivePlayers)
                {
                    if (abstractCreature != player.abstractCreature)
                    {
                        Creature realizedCreature = abstractCreature.realizedCreature;
                        realizedCreature?.Stun(20);
                    }
                }
            }
            chatlogCounter++;
            // 淦 这数还不能瞎改 急
            if (chatlogCounter == 60 && player.room.game.cameras[0].hud.chatLog == null)
            {
                Plugin.Log("init chatlog hud, lines in chatlog:", chatlogList.Count);
                string s = "CHATLOG:";
                foreach (string str in chatlogList)
                {
                    s += str + " ";
                }
                Plugin.Log(s);
                player.room.game.cameras[0].hud.InitChatLog(chatlogList.ToArray());
            }
            else if (player.room.game.cameras[0].hud.chatLog == null && this.chatlogCounter >= 60)
            {
                chatlog = false;
                pearlHeld.gravity = 0.9f;
                pearlHeld = null;
            }

        }


    }





    // public static string[] getChatlog(ChatlogData.ChatlogID id) 返回的是一个string[]，我只要把下面那个list组装一下就行了


    // LoadConversationFromFile()需要重写
    // 好 我明白为啥没人做读珍珠了（恼
    // 赢了，成功了（喜）下一步是做crs的适配
    // 最好摇个人帮我测试一下全游戏所有的珍珠 我懒得自己测（

    public List<string> DataPearlToText(DataPearl pearl)
    {

        List<string> result = new List<string>()
        {
            "[Error: pearl text not found]",
        };

        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc)
        {
            return LoadConversationFromFile(38, pearl.abstractPhysicalObject.ID.RandomSeed);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc2)
        {
            return LoadConversationFromFile(38, pearl.abstractPhysicalObject.ID.RandomSeed);
        }
        if (pearl is PebblesPearl)
        {
            return LoadConversationFromFile(40, pearl.abstractPhysicalObject.ID.RandomSeed);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.CC)
        {
            
            return LoadConversationFromFile(7);
        }
        // public int GetARandomChatLog(bool whichPearl)
        /*if (!ModManager.MSC && id == Conversation.ID.Moon_Pearl_SI_west)
        {
            LoadConversationFromFile(this.GetARandomChatLog(false));
            return;
        }
        if (!ModManager.MSC && id == Conversation.ID.Moon_Pearl_SI_top)
        {
            LoadConversationFromFile(this.GetARandomChatLog(true));
            return;
        }*/
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.LF_west)
        {
            return LoadConversationFromFile(10);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.LF_bottom)
        {
            return LoadConversationFromFile(11);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.HI)
        {
            return LoadConversationFromFile(12);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SH)
        {
            return LoadConversationFromFile(13);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.DS)
        {
            return LoadConversationFromFile(14);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SB_filtration)
        {
            return LoadConversationFromFile(15);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.GW)
        {
            return LoadConversationFromFile(16);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SL_bridge)
        {
            return LoadConversationFromFile(17);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SL_moon)
        {
            return LoadConversationFromFile(18);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SU)
        {
            return LoadConversationFromFile(41);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.UW)
        {
            return LoadConversationFromFile(42);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SB_ravine)
        {
            return LoadConversationFromFile(43);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SL_chimney)
        {
            return LoadConversationFromFile(54);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Red_stomach)
        {
            return LoadConversationFromFile(51);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SI_west)
        {
            return LoadConversationFromFile(20);
        }
        if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.SI_top)
        {
            return LoadConversationFromFile(21);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.SI_chat3)
        {
            return LoadConversationFromFile(22);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.SI_chat4)
        {
            return LoadConversationFromFile(23);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.SI_chat5)
        {
            return LoadConversationFromFile(24);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.SU_filt)
        {
            return LoadConversationFromFile(101);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.DM)
        {
            return LoadConversationFromFile(102);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.LC)
        {
            return LoadConversationFromFile(103);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.OE)
        {
            return LoadConversationFromFile(104);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.MS)
        {
            return LoadConversationFromFile(105);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.RM)
        {
            return LoadConversationFromFile(106);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.Rivulet_stomach)
        {
            return LoadConversationFromFile(119);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.LC_second)
        {
            return LoadConversationFromFile(121);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.VS)
        {
            return LoadConversationFromFile(128);
        }
        if (pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.BroadcastMisc)
        {
            return LoadConversationFromFile(132, pearl.abstractPhysicalObject.ID.RandomSeed);
        }
        
        // 淦 那东西是internal 啊啊啊啊啊啊啊啊啊啊啊
        // 摆了，我得让crs的作者给我添加一个友元程序集，好麻烦，等我社恐治好了再说吧（
        /*foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomRegions.Mod.Structs.CustomPearl> pair in CustomRegions.Collectables.PearlData.CustomDataPearlsList)
        { }*/
        return result;
    }



    // 看上去很容易卡bug的样子。。
    // 好家伙，竟然一遍过了。。
    public List<string> LoadConversationFromFile(int fileName, int miscPearlRandomSeed = -1)
    {
        try
        {
            List<string> texts = new List<string>();

            InGameTranslator translator = player.room.game.rainWorld.inGameTranslator;
            InGameTranslator.LanguageID languageID = player.room.game.rainWorld.inGameTranslator.currentLanguage;
            string filePath;
            for (; ; )
            {
                filePath = AssetManager.ResolveFilePath(translator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + ".txt");
                // 这应该是不同角色的特殊对话文本 我就不管了 反正文本迟早得重写
                /*if (player.room.game.IsStorySession)
                {
                    filePath = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                        translator.SpecificTextFolderDirectory(languageID),
                        Path.DirectorySeparatorChar.ToString(),
                        fileName.ToString(),
                        "-",
                        player.room.game.StoryCharacter.value,
                        ".txt"
                    }));
                }*/
                if (File.Exists(filePath))
                {


                    string origText = File.ReadAllText(filePath, Encoding.UTF8);
                    if (origText[0] != '0')
                    {
                        origText = Custom.xorEncrypt(origText, 54 + fileName + (int)translator.currentLanguage * 7);
                    }
                    string[] array = Regex.Split(origText, "\r\n");



                    try
                    {
                        if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                        {

                            // 白色珍珠那种随机选一行
                            // 暂不考虑，这得单独写一个随机的规则，跟珍珠的apo.id有关

                            if (miscPearlRandomSeed == -1)
                            {
                                for (int j = 1; j < array.Length; j++)
                                {
                                    string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
                                    if (array3.Length == 3)
                                    {
                                        int num;
                                        int num2;
                                        if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out num2))
                                        {
                                            texts.Add(array3[1]);
                                        }
                                        else
                                        {
                                            texts.Add(array3[2]);
                                        }
                                    }
                                    else if (array3.Length == 2)
                                    {
                                        if (array3[0] == "SPECEVENT")
                                        {
                                            texts.Add("<SPECEVENT>" + array3[1]);
                                        }
                                        else if (array3[0] == "PEBBLESWAIT")
                                        {
                                            texts.Add("PEBBLESWAIT" + array3[2]);
                                        }
                                    }
                                    else if (array3.Length == 1 && array3[0].Length > 0)
                                    {
                                        texts.Add(array3[0]);
                                    }
                                }
                            }
                            else
                            {
                                List<string> list = new();
                                for (int i = 1; i < array.Length; i++)
                                {
                                    string[] array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
                                    if (array2.Length == 3)
                                    {
                                        list.Add(array2[2]);
                                    }
                                    else if (array2.Length == 1 && array2[0].Length > 0)
                                    {
                                        list.Add(array2[0]);
                                    }
                                }
                                if (list.Count > 0)
                                {
                                    Random.State state = Random.state;
                                    Random.InitState(miscPearlRandomSeed);
                                    string item = list[Random.Range(0, list.Count)];
                                    Random.state = state;
                                    texts.Add(item);
                                }
                            }

                            return texts;
                        }
                    }
                    catch
                    {
                        Custom.LogWarning("TEXT ERROR");
                        texts.Add("<TEXT ERROR>");
                    }
                }
            }




        }
        catch (Exception e)
        {
            Plugin.LogException(e);
            return new List<string>()
            {
                "LoadConversationFromFile error!",
            };
        }
    }




}












// 太狼狈了 每次一做hud就得复制粘贴代码
public class PearlReaderHUD : HudPart
{
    public PearlReader owner;
    public FSprite circle;
    public Vector2 pos;
    public Vector2 lastPos;
    public float fade;
    public float lastFade;

    public PearlReaderHUD(HUD.HUD hud, FContainer fContainer, PearlReader owner) : base(hud)
    {
        this.owner = owner;
        circle = new FSprite("Futile_White", true);
        circle.shader = hud.rainWorld.Shaders["HoldButtonCircle"];
        circle.color = Color.white;
        pos = owner.player.mainBodyChunk.pos;
        lastPos = pos;
        fade = 0f;
        lastFade = 0f;
        circle.scale = 3f;
        fContainer.AddChild(circle);
    }



    private bool Show
    {
        get
        {
            return owner != null && owner.player.room != null && owner.holdingPearlCounter > 0;
        }
    }



    public override void Update()
    {
        base.Update();
        lastPos = pos;
        lastFade = fade;
        Vector2 camPos = Vector2.zero;
        if (owner.player.room != null)
        {
            camPos = owner.player.room.game.cameras[0].pos;
        }
        pos = owner.player.mainBodyChunk.pos - camPos;

        circle.isVisible = Show;
        fade = ((float)(PearlReader.StartCounter - owner.holdingPearlCounter) / PearlReader.StartCounter);


    }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame) return;
        circle.alpha = fade;
        circle.SetPosition(DrawPos(timeStacker));
    }

    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(lastPos, pos, timeStacker);
    }



    public override void ClearSprites()
    {
        base.ClearSprites();
        circle.RemoveFromContainer();
        circle = null;
    }


}
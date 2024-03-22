using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_merged.fp;

public class SLOracleHooks
{


    public static void MoonFirstPostMarkConversation(SLOracleBehaviorHasMark.MoonConversation self)
    {
        switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
        {
            // 不会有雨鹿绞尽脑汁绕过我加的食性限制还要吃神经元罢（
            // 应当是不会的罢 所以我不写了（
            case 0:
                break;
            case 1:
                self.events.Add(new Conversation.TextEvent(self, 40, "...", 10));
                return;
            case 2:
                self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Get... get away... white.... thing."), 10));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Please... thiss all I have left."), 10));
                return;
            case 3:
                self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("You!"), 10));
                self.events.Add(new Conversation.TextEvent(self, 60, self.Translate("...you ate... me. Please go away. I won't speak... to you.<LINE>I... CAN'T speak to you... because... you ate...me..."), 0));
                return;
            case 4:
                // 哈？这两个文件是啥啊
                /*Plugin.LogAllConversations(self, 35);
                Plugin.LogAllConversations(self, 37);*/
                self.LoadEventsFromFile(35);
                self.LoadEventsFromFile(37);
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I'm still angry at you, but it is good to have someone to talk to after all self time.<LINE>The scavengers aren't exactly good listeners. They do bring me things though, occasionally..."), 0));
                return;
            case 5:

                // 会编程真是方便啊。jpg
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Hello <PlayerName>."), 0));
                /*self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I think I know what you're coming here for."), 0));
                for (int i = 1; i <= 57; i++)
                {
                    Plugin.LogAllConversations(self, i);
                }
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Here are all the conversations in game."), 0));*/
                return;
            default:
                return;

        }
    }





}

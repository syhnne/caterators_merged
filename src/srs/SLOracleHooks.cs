using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_merged.srs;

internal class SLOracleHooks
{

    public static void MoonFirstPostMarkConversation(SLOracleBehaviorHasMark.MoonConversation self)
    {
        switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
        {
            case 0:
                break;
            case 1:
                self.events.Add(new Conversation.TextEvent(self, 40, "...", 10));
                return;
            case 2:
                self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Get... get away..."), 10));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Please... thiss all I have left."), 10));
                return;
            case 3:
                self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("You!"), 10));
                self.events.Add(new Conversation.TextEvent(self, 60, self.Translate("...you ate... me. Please go away. I won't speak... to you.<LINE>I... CAN'T speak to you... because... you ate...me..."), 0));
                return;
            case 4:
                self.LoadEventsFromFile(35);
                self.LoadEventsFromFile(37);
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I'm still angry at you, but it is good to have someone to talk to after all self time.<LINE>The scavengers aren't exactly good listeners. They do bring me things though, occasionally..."), 0));
                return;
            case 5:

                // 芜，我明白了，去抄一下魔方节点那里的对话代码
                // 呃，总之，，我准备使用cutscene mode 并且从pickup candidates里去掉sloracleswarmer防止有雨鹿尝试抓月姐的神经元（主要是为我减小写对话的工作量）
                // 但是并不能减少啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊，，还是得写（抓头发）
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Hello <PlayerName>."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("(The conversation is a work in progress, please wait for future updates)"), 0));
                return;
            default:
                return;

        }
    }
}

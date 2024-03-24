using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_merged;

public class SLOracleHooks
{

    public static void Apply()
    {
        On.SLOracleBehaviorHasMark.InitateConversation += SLOracleBehaviorHasMark_InitateConversation;
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
    }




    private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        SlugcatStats.Name ssn = self.myBehavior.oracle.room.game.StoryCharacter;
        if (self.myBehavior.oracle.room.game.IsStorySession && Enums.IsCaterator(ssn))
        {
            Plugin.Log("moon conversation:", self.id.ToString(), self.State.neuronsLeft.ToString(), "savestatenumber:",  ssn);
        }


        if (ssn == Enums.FPname && self.id == Conversation.ID.MoonFirstPostMarkConversation)
        {
            fp.SLOracleHooks.MoonFirstPostMarkConversation(self);
        }
        else if (ssn == Enums.SRSname && self.id == Conversation.ID.MoonFirstPostMarkConversation)
        {
            srs.SLOracleHooks.MoonFirstPostMarkConversation(self);
        }
        else { orig(self); }
    }



    private static void SLOracleBehaviorHasMark_InitateConversation(On.SLOracleBehaviorHasMark.orig_InitateConversation orig, SLOracleBehaviorHasMark self)
    {
        if (self.oracle.room.game.IsStorySession && Enums.IsCaterator(self.oracle.room.game.StoryCharacter))
        {
            if (!self.State.SpeakingTerms)
            {
                self.dialogBox.NewMessage("...", 10);
                return;
            }
            int swarmers = 0;
            for (int i = 0; i < self.player.grasps.Length; i++)
            {
                if (self.player.grasps[i] != null && self.player.grasps[i].grabbed is SSOracleSwarmer)
                {
                    swarmers++;
                }
            }
            if (self.State.playerEncountersWithMark <= 0)
            {
                if (self.State.playerEncounters < 0)
                {
                    self.State.playerEncounters = 0;
                }



                self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(Conversation.ID.MoonFirstPostMarkConversation, self, SLOracleBehaviorHasMark.MiscItemType.NA);
                return;
            }
            else
            {
                if (swarmers > 0)
                {
                    self.PlayerHoldingSSNeuronsGreeting();
                    return;
                }
                if (self.State.playerEncountersWithMark != 1)
                {
                    self.ThirdAndUpGreeting();
                    return;
                }
                self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(Conversation.ID.MoonSecondPostMarkConversation, self, SLOracleBehaviorHasMark.MiscItemType.NA);
                return;
            }



        }
        else { orig(self); }
    }


}

using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using System.Xml.Schema;

namespace Caterators_by_syhnne._public;

public class SSOracleHooks
{
    public static SSOracleBehavior.Action ActionID = new("fpslugcat_Action", false);
    public static SSOracleBehavior.Action ReviveSwarmerAction = new("reviveSwarmerAction", false);
    public static SSOracleBehavior.SubBehavior.SubBehavID SubBehavID = new("fpslugcat_SubBehavior", false);
    public static Conversation.ID ConversationID = new("fpslugcat_Conversation", false);

    public static void Apply()
    {
        On.SSOracleBehavior.ctor += SSOracleBehavior_ctor;
        On.SSOracleBehavior.UnconciousUpdate += SSOracleBehavior_UnconciousUpdate;
        On.SSOracleBehavior.SeePlayer += SSOracleBehavior_SeePlayer;
        On.SSOracleBehavior.NewAction += SSOracleBehavior_NewAction;

        new Hook(
            typeof(Oracle).GetProperty(nameof(Oracle.Consious), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            Oracle_Consious
            );
        On.Oracle.SetUpMarbles += Oracle_SetUpMarbles;
        fp.SSOracleHooks.Apply();
    }





    // 婴儿般的睡眠。jpg
    private delegate bool orig_Consious(Oracle self);
    private static bool Oracle_Consious(orig_Consious orig, Oracle self)
    {
        var result = orig(self);
        if (self.room.game.IsStorySession && self.ID == Oracle.OracleID.SS && self.room.game.GetStorySession.saveState.IsCaterator() && (self.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding != true || self.room.game.StoryCharacter != Enums.FPname))
        {
            result = false;
        }
        else if (self.room.game.IsStorySession && self.ID == Oracle.OracleID.SL && self.room.game.StoryCharacter == Enums.Moonname)
        {
            result = false;
        }
        return result;
    }




    private static void Oracle_SetUpMarbles(On.Oracle.orig_SetUpMarbles orig, Oracle self)
    {
        if (self.room.game.IsStorySession && self.room.game.IsCaterator() && self.room.game.StoryCharacter != Enums.FPname)
        {
            return;
        }
        orig(self);
    }



    private static void SSOracleBehavior_ctor(On.SSOracleBehavior.orig_ctor orig, SSOracleBehavior self, Oracle oracle)
    {
        orig(self, oracle);
        if (oracle.room.game.IsCaterator())
        {
            self.movementBehavior = SSOracleBehavior.MovementBehavior.Meditate;
        }
        

    }



    private static void SSOracleBehavior_UnconciousUpdate(On.SSOracleBehavior.orig_UnconciousUpdate orig, SSOracleBehavior self)
    {
        if (self.oracle.ID == Oracle.OracleID.SS && self.oracle.room.game.IsCaterator())
        {
            self.FindPlayer();
            self.unconciousTick += 1f;
            self.oracle.setGravity(0.9f);
            if (self.oracle.room.game.StoryCharacter == Enums.FPname)
            {
                if (self.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) != null)
                {
                    self.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0f, 1f, 0.2f + Mathf.Sin(self.unconciousTick * 0.15f));
                }
                if (self.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) != null)
                {
                    self.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0f, 0.4f, 0.2f + Mathf.Sin(self.unconciousTick * 0.15f));
                }
                if (self.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) != null)
                {
                    self.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0f, 0.3f, 0.2f + Mathf.Sin(self.unconciousTick * 0.15f));
                }
            }
            else if (self.player == null)
            {
                self.oracle.room.gravity = 1f;
                return;
            }
            if (self.player != null && Plugin.playerModules.TryGetValue(self.player, out var module) && module.isCaterator && module.gravityController != null && module.gravityController.enabled)
            {
                self.oracle.room.gravity = module.gravityController.gravityBonus * 0.1f;
            }



        }
        else { orig(self); }
    }





    // 让fp无视你的大部分行为
    private static void SSOracleBehavior_SeePlayer(On.SSOracleBehavior.orig_SeePlayer orig, SSOracleBehavior self)
    {
        Plugin.Log("SSOracleBehavior_SeePlayer", self.oracle.room.game.StoryCharacter);
        if (self.oracle.ID == Oracle.OracleID.SS && self.oracle.room.game.IsStorySession && self.oracle.room.game.IsCaterator())
        {
            self.NewAction(ActionID);
            self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad = 0;
        }
        else { orig(self); }
        Plugin.Log("ssaithrowouts:", self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts);
    }






    private static void SSOracleBehavior_NewAction(On.SSOracleBehavior.orig_NewAction orig, SSOracleBehavior self, SSOracleBehavior.Action nextAction)
    {
        Plugin.Log("SSOracleBehavior next action:", nextAction);
        if (nextAction == ActionID)
        {
            if (self.currSubBehavior.ID == SubBehavID) return;


            SSOracleBehavior.SubBehavior subBehavior = null;
            for (int i = 0; i < self.allSubBehaviors.Count; i++)
            {
                if (self.allSubBehaviors[i].ID == SubBehavID)
                {
                    subBehavior = self.allSubBehaviors[i];
                    break;
                }
            }
            if (subBehavior == null)
            {
                subBehavior = new SSOracleSubBehavior(self);
                self.allSubBehaviors.Add(subBehavior);
            }
            subBehavior.Activate(self.action, nextAction);
            self.currSubBehavior.Deactivate();
            Plugin.Log("Switching subbehavior to: " + subBehavior.ID.ToString() + " from: " + self.currSubBehavior.ID.ToString());
            self.currSubBehavior = subBehavior;
            self.inActionCounter = 0;
            self.action = nextAction;
            return;
        }
        else if (self.oracle.room.game.IsStorySession && self.oracle.room.game.IsCaterator())
        {

            if (nextAction == self.action) return;
            Plugin.Log("old action:", self.action.ToString(), "new action:", nextAction.ToString());

            // 防止一切洗脑失败的情况（。）直接给你堵死。乐
            nextAction = ActionID;

        }
        orig(self, nextAction);
    }


}










public class SSOracleSubBehavior : SSOracleBehavior.ConversationBehavior
{
    public bool firstMetOnThisCycle;

    public float lastGetToWork;

    public float tagTimer;
    private PlayerModule PlayerModule;


    // 千万别调用convoID，因为它是我瞎写的占位符，啥也不是
    public SSOracleSubBehavior(SSOracleBehavior owner) : base(owner, SSOracleHooks.SubBehavID, SSOracleHooks.ConversationID)
    {
        if (oracle.ID != Oracle.OracleID.SS) return;
        Plugin.Log("SSoracleBehavior - subBehavior ctor");
        this.owner.TurnOffSSMusic(true);

        if (player != null && player.room != null && player.room == this.owner.oracle.room)
        {
            PlayerModule = Plugin.playerModules.TryGetValue(player, out var module) && module.isCaterator ? module : null;
        }
        if (this.owner.conversation != null)
        {
            this.owner.conversation.Destroy();
            this.owner.conversation = null;
        }
        this.owner.movementBehavior = SSOracleBehavior.MovementBehavior.Meditate;

    }


    public override void Update()
    {
        base.Update();
        if (player == null)
        {
            return;
        }
        owner.movementBehavior = SSOracleBehavior.MovementBehavior.Meditate;
        if (tagTimer > 0f && owner.inspectPearl != null)
        {
            owner.killFac = Mathf.Clamp(tagTimer / 120f, 0f, 1f);
            tagTimer -= 1f;
            if (tagTimer <= 0f)
            {
                for (int i = 0; i < 20; i++)
                {
                    oracle.room.AddObject(new Spark(owner.inspectPearl.firstChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                }
                oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.inspectPearl.firstChunk.pos, 1f, 0.5f + Random.value * 0.5f);
                owner.killFac = 0f;
            }
        }
    }



    // 这个本来和console有关的 我懒得搬运，把console删了 所以只能瞎改一气（
    /*public override Vector2? LookPoint
    {
        get
        {
            if (base.player == null) return null;
            if (Enums.IsCaterator(player.SlugCatClass) && player.grasps.Count() > 0 && (player.grasps[0].grabbed != null || player.grasps[1].grabbed != null))
            {
                return player.mainBodyChunk.pos;
            }
            return this.oracle.firstChunk.pos;
        }
    }*/




    public override float LowGravity
    {
        get
        {
            if (PlayerModule != null && PlayerModule.gravityController != null)
            {
                return PlayerModule.gravityController.gravityBonus * 0.1f;
            }
            return -1f;
        }
    }









    public override void Deactivate()
    {
        base.Deactivate();
    }



    public override void Activate(SSOracleBehavior.Action oldAction, SSOracleBehavior.Action newAction)
    {
        base.Activate(oldAction, newAction);
    }



    /*public override void NewAction(SSOracleBehavior.Action oldAction, SSOracleBehavior.Action newAction)
    {
        base.NewAction(oldAction, newAction);
        if (newAction == SSOracleBehavior.Action.ThrowOut_KillOnSight && this.player.conversation != null)
        {
            this.player.conversation.Destroy();
            this.player.conversation = null;
        }
    }*/


}

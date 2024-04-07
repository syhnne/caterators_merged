using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_by_syhnne._public;


// 这其实没什么用罢，到最后我还是得手搓一切roomSettings
public class SSRoomEffects
{
    public static void Apply()
    {
        On.GravityDisruptor.Update += GravityDisruptor_Update;
        On.CoralBrain.SSMusicTrigger.Trigger += CoralBrain_SSMusicTrigger_Trigger;
        On.ZapCoil.Update += ZapCoil_Update;
        On.ZapCoilLight.Update += ZapCoilLight_Update;
        On.AbstractRoom.RealizeRoom += AbstractRoom_RealizeRoom;
        On.SSLightRod.Update += SSLightRod_Update;
        On.Room.Loaded += Room_Loaded;


    }

















    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        if (self.game != null && self.game.IsStorySession && self.game.IsCaterator() && !self.game.GetStorySession.saveState.deathPersistentSaveData.altEnding && self.abstractRoom.name.StartsWith("SS")
            && self.roomSettings != null && self.roomSettings.placedObjects.Count > 0)
        {

            for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
            {
                PlacedObject obj = self.roomSettings.placedObjects[i];
                if (obj.type == PlacedObject.Type.ProjectedStars)
                {
                    obj.active = false;
                }
            }
        }
        orig(self);
    }




    private static void ZapCoilLight_Update(On.ZapCoilLight.orig_Update orig, ZapCoilLight self, bool eu)
    {
        orig(self, eu);
        if (self.room != null && self.room.game.IsStorySession && self.room.game.IsCaterator() && self.room.abstractRoom.name.StartsWith("SS"))
        {
            self.lightSource.alpha = 0f;
        }
    }






    private static void AbstractRoom_RealizeRoom(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
    {
        orig(self, world, game);
        if (self.realizedRoom == null || !game.IsStorySession || !game.IsCaterator() || !self.name.StartsWith("SS")) { return; }


        RoomSettings settings = self.realizedRoom.roomSettings;

        settings.RemoveEffect(RoomSettings.RoomEffect.Type.SSMusic);

        if (game.StoryCharacter != Enums.FPname)
        {
            settings.RemoveEffect(RoomSettings.RoomEffect.Type.ProjectedScanLines);
            settings.RemoveEffect(RoomSettings.RoomEffect.Type.SuperStructureProjector);
            settings.RemoveEffect(RoomSettings.RoomEffect.Type.SSSwarmers);


            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_IND-Turbine.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_IND-Deep50hz.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-Escape.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Omnidirectional, "AM_IND-SuperStructure.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-DataTrans.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-DataTrans2.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-DataStream.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-Data.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-Data2.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-Data3.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-Data4.ogg");
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-Data5.ogg");

            if (self.name.StartsWith("SS"))
            {
                if (settings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) != null)
                {
                    settings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG).amount = 0.9f;
                }
                if (settings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG) != null)
                {
                    settings.GetEffect(RoomSettings.RoomEffect.Type.BrokenZeroG).amount = 0f;
                }
            }

            if (self.name == "SS_AI")
            {
                if (settings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) == null)
                {
                    settings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.DarkenLights, 0.2f, false));
                }
                if (settings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) == null)
                {
                    settings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Darkness, 0.1f, false));
                }
                if (settings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
                {
                    settings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 0.1f, false));
                }
            }

        }
        else if (!self.world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
        {
            settings.RemoveEffect(RoomSettings.RoomEffect.Type.ProjectedScanLines);
            settings.RemoveEffect(RoomSettings.RoomEffect.Type.SuperStructureProjector);

            // 擦 原来gravityDisrupter那个巨大的动静是底下这个 怪不得老挪不掉 看名字谁看得出来啊（。
            settings.RemoveAmbientSound(AmbientSound.Type.Spot, "SO_SFX-Escape.ogg");
        }

        /*foreach (AmbientSound sound in settings.ambientSounds)
           {
               Plugin.Log("room:", self.name, "ambientSound:", sound.type.ToString(), sound.sample);
           }*/
    }







    private static void SSLightRod_Update(On.SSLightRod.orig_Update orig, SSLightRod self, bool eu)
    {
        orig(self, eu);
        if (self.room != null && self.room.game.IsStorySession && self.room.game.IsCaterator() && self.room.abstractRoom.name.StartsWith("SS"))
        {
            self.lights = new List<SSLightRod.LightVessel>();
            self.color = new Color(0.1f, 0.1f, 0.1f);
        }

    }





















    // 怪了，这个ssmusic死活去不掉，只有在这才能去掉，但是这加不了判定（恼
    private static void RoomSettings_LoadEffects(On.RoomSettings.orig_LoadEffects orig, RoomSettings self, string[] s)
    {
        orig(self, s);
        self.RemoveEffect(RoomSettings.RoomEffect.Type.SSMusic);

    }





    // TODO: 修好这个东西（我不想修了，反正他卡bug的也就那么几帧，无脑catch完事
    // 关掉！必须要关掉！
    private static void ZapCoil_Update(On.ZapCoil.orig_Update orig, ZapCoil self, bool eu)
    {
        try
        {
            if (self.room.game.IsStorySession && self.room.game.IsCaterator() && self.room.abstractRoom.name.StartsWith("SS"))
            {
                self.powered = false;
            }
            orig(self, eu);
        }
        catch
        {
            // base.Logger.LogError(ex);
        }

    }




    private static void GravityDisruptor_Update(On.GravityDisruptor.orig_Update orig, GravityDisruptor self, bool eu)
    {
        orig(self, eu);
        if (self.room != null && self.room.game.IsStorySession && self.room.game.IsCaterator() && self.room.abstractRoom.name.StartsWith("SS"))
        {
            self.power = 0f;

        }
    }






    // 啊啊啊啊啊啊啊啊啊别放音乐了
    private static void CoralBrain_SSMusicTrigger_Trigger(On.CoralBrain.SSMusicTrigger.orig_Trigger orig, CoralBrain.SSMusicTrigger self)
    {
        if (self.room.game.IsStorySession && self.room.game.IsCaterator() && self.room.abstractRoom.name.StartsWith("SS"))
        {
            return;
        }
        orig(self);
    }


}

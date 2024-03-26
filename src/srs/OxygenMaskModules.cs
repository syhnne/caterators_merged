using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_by_syhnne.srs;



// 合着我捯饬半天没有用 是没挂上啊（擦汗）
public class OxygenMaskModules
{

    public static SLOracleBehaviorHasMark.MiscItemType OxygenMaskMisc = new("OxygenMask", false);



    public class OxygenMask : Weapon
    {



        public int count;
        public OxygenMaskAbstract Abstr;

        public OxygenMask(OxygenMaskAbstract abstr) : base(abstr, abstr.world)
        {
            Abstr = abstr;


            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.14f);
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
            base.airFriction = 0.999f;
            base.gravity = 0.9f;
            this.bounce = 0.4f;
            this.surfaceFriction = 0.3f;
            this.collisionLayer = 2;
            base.waterFriction = 0.98f;
            base.buoyancy = 0.6f;
            // bodyChunks[0].pos = new Vector2(300, 300);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);


            ChangeCollisionLayer(grabbedBy.Count == 0 ? 2 : 1);
            firstChunk.collideWithTerrain = grabbedBy.Count == 0;
            firstChunk.collideWithSlopes = grabbedBy.Count == 0;
            if (base.Submersion >= 0.5f)
            {
                this.room.AddObject(new Bubble(base.firstChunk.pos, base.firstChunk.vel, false, false));
            }

            // Plugin.Log("o pos:", firstChunk.pos);
            if (grabbedBy.Count > 0 && grabbedBy[0].grabber != null && grabbedBy[0].grabber is Player)
            {
                if (this.room != null && room.game.IsStorySession)
                {
                    /*Plugin.Log("1", room.game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken);
                    Plugin.Log("2", room.game.GetStorySession.lastEverMetPebbles);
                    Plugin.Log("3", room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad);*/
                    room.game.GetDeathPersistent().OxygenMaskTaken = true;

                }

                if (count == Abstr.lungCapacityBonus)
                {
                    count = 0;
                }
                count++;
            }




        }



        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            AddToContainer(sLeaser, rCam, null);
        }


        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            float num = Mathf.InverseLerp(305f, 380f, timeStacker);
            pos.y -= 20f * Mathf.Pow(num, 3f);

            sLeaser.sprites[0].isVisible = true;
            sLeaser.sprites[0].scale = 1f;
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }


        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = Color.white;
        }


        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContainer.AddChild(fsprite);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            if (Abstr.realizedObject != null)
            {
                Abstr.realizedObject = null;
            }
        }

    }










    #region Abstract

    public class OxygenMaskAbstract : AbstractPhysicalObject
    {
        public int lungCapacityBonus;

        public OxygenMaskAbstract(World world, WorldCoordinate pos, EntityID ID, int lungCapBonus) : base(world, OxygenMaskFisob.OxygenMask, null, pos, ID)
        {
            lungCapacityBonus = lungCapBonus;
        }
        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new OxygenMask(this);
            }

        }
        public override string ToString()
        {
            return this.SaveToString($"{lungCapacityBonus}");
        }
    }

    #endregion





    #region Fisob

    public class OxygenMaskFisob : Fisob
    {
        private static readonly OxygenMaskProperties properties = new();
        public static readonly AbstractPhysicalObject.AbstractObjectType OxygenMask = new("OxygenMask", true);
        public OxygenMaskFisob() : base(OxygenMask)
        {
            Icon = new OxygenMaskIcon();
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {
            // Centi shield data is just floats separated by ; characters.
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 1)
            {
                p = new string[1];
            }

            var result = new OxygenMaskAbstract(world, saveData.Pos, saveData.ID, 3)
            {
                lungCapacityBonus = int.TryParse(p[0], out var b) ? b : 0,
            };

            // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see CentiShieldIcon below).
            if (unlock is SandboxUnlock u)
            {
                result.lungCapacityBonus = u.Data;

            }

            return result;
        }

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
            // The Mosquitoes example demonstrates this.
            return properties;
        }
    }

    #endregion





    #region Icon

    public class OxygenMaskIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is OxygenMaskAbstract m ? (int)m.lungCapacityBonus : 0;
        }

        public override Color SpriteColor(int data)
        {
            return Color.white;
        }

        public override string SpriteName(int data)
        {
            // Fisobs autoloads the file in the mod folder named "icon_{Type}.png"
            // To use that, just remove the png suffix: "icon_CentiShield"
            return "icon_OxygenMask";
        }
    }


    #endregion





    #region Properties

    public class OxygenMaskProperties : ItemProperties
    {

        public override void Throwable(Player player, ref bool throwable)
            => throwable = false;

        // 把这个送给拾荒者还不如赶紧产几根矛（
        // 应该不会有人这么干的罢
        public override void ScavCollectScore(Scavenger scavenger, ref int score)
            => score = 2;

        public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
            => score = 0;

        // Don't throw shields
        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
            => score = 0;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            // The player can only grab one centishield at a time,
            // but that shouldn't prevent them from grabbing a spear,
            // so don't use Player.ObjectGrabability.BigOneHand

            if (player.grasps.Any(g => g?.grabbed is OxygenMask))
            {
                grabability = Player.ObjectGrabability.CantGrab;
            }
            else
            {
                grabability = Player.ObjectGrabability.OneHand;
            }
        }
    }

    #endregion



}

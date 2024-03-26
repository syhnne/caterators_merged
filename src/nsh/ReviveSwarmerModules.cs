using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Caterators_by_syhnne.srs.OxygenMaskModules;
using System.Security.Permissions;

namespace Caterators_by_syhnne.nsh;


// 还是这个写的爽 复制粘贴就完事了（。
public class ReviveSwarmerModules
{
    

    public class ReviveSwarmerCreatureSelector : UpdatableAndDeletable
    {
        public ReviveSwarmerCreatureSelector(Room room) 
        { 
            this.room = room;
        }


    }



    public class ReviveSwarmer : PlayerCarryableItem, IDrawable
    {


        public ReviveSwarmerAbstract Abstr;
        public float rotation;
        public float lastRotation;
        public Creature selectedCreature;


        public ReviveSwarmer(ReviveSwarmerAbstract abstr) : base(abstr)
        {
            Abstr = abstr;
            this.collisionLayer = 1;
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), 3f, 0.2f);
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
            base.airFriction = 0.999f;
            base.gravity = 0.9f;
            this.bounce = 0.4f;
            this.surfaceFriction = 0.4f;
            base.waterFriction = 0.94f;
            base.buoyancy = 1.1f;
            this.rotation = 0.25f;
            this.lastRotation = this.rotation;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            ChangeCollisionLayer(grabbedBy.Count == 0 ? 2 : 1);
            firstChunk.collideWithTerrain = grabbedBy.Count == 0;
            firstChunk.collideWithSlopes = grabbedBy.Count == 0;


            return;

            // 参考：JollyPointUpdate()
            if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player)
            {
                Player player = (grabbedBy[0].grabber as Player);
                if (player.input[0].pckp && player.input[0].y == -1)
                {
                    selectedCreature = null;
                    return;
                }
                else if (player.room != null && player.input[0].pckp && player.input[0].y == 1)
                {
                    if (selectedCreature == null)
                    {
                        /*foreach (var obj in player.room.physicalObjects)
                        {
                            foreach (var obj2 in obj)
                            {
                                phyObj += obj2.GetType().Name + " ";
                            }
                        }*/
                    }
                }
            }
        }


        public void Use()
        {
            if (!Abstr.isActive || selectedCreature == null || grabbedBy.Count <= 0 || grabbedBy[0].grabber is not Player
                 || grabbedBy[0].grabber.room == null || selectedCreature.room != grabbedBy[0].grabber.room) 
            { return; }
            Plugin.Log("NSHswarmer use:", selectedCreature?.GetType().Name);
            AllGraspsLetGoOfThisObject(true);

            Destroy();
        }



        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            
            AddToContainer(sLeaser, rCam, null);
        }


        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContainer.AddChild(fsprite);
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
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

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = new Color(0f, 1f, 0.3f);
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








    public class ReviveSwarmerAbstract : AbstractPhysicalObject
    {

        public bool isActive;
        public ReviveSwarmerAbstract(World world, WorldCoordinate pos, EntityID ID, bool isActive) : base(world, ReviveSwarmerFisob.ReviveSwarmer, null, pos, ID)
        {
            this.isActive = isActive;
        }
        public override void Realize()
        {
            base.Realize();
            realizedObject ??= new ReviveSwarmer(this);

        }
        public override string ToString()
        {
            return this.SaveToString($"{isActive}");
        }

    }












    public class ReviveSwarmerFisob : Fisob
    {
        private static readonly ReviveSwarmerProperties properties = new();
        public static readonly AbstractPhysicalObject.AbstractObjectType ReviveSwarmer = new("ReviveSwarmer", true);
        public ReviveSwarmerFisob() : base(ReviveSwarmer)
        {
            Icon = new ReviveSwarmerIcon();
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {
            // Centi shield data is just floats separated by ; characters.
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 1)
            {
                p = new string[1];
            }

            var result = new ReviveSwarmerAbstract(world, saveData.Pos, saveData.ID, true)
            {
                isActive = bool.TryParse(p[0], out var b) ? b : true,
                //lungCapacityBonus = int.TryParse(p[0], out var b) ? b : 0,
            };

            // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see CentiShieldIcon below).
            if (unlock is SandboxUnlock u)
            {
                //result.lungCapacityBonus = u.Data;
                result.isActive = u.Data == 1;
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










    public class ReviveSwarmerIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is ReviveSwarmerAbstract m ? 1 : 0;
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









    public class ReviveSwarmerProperties : ItemProperties
    {

        public override void Throwable(Player player, ref bool throwable)
            => throwable = true;

        // 拾荒者拿了也不知道怎么用，所以他们不要（（
        public override void ScavCollectScore(Scavenger scavenger, ref int score)
            => score = 0;

        public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
            => score = 0;

        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
            => score = 0;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
    }




}

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
using System.Security.Permissions;
using RWCustom;

namespace Caterators_by_syhnne.nsh;


// 是我想多了 死的从来都不是猫崽而是我。。
public class ReviveSwarmerModules
{
    public static Color NSHswarmerColor = new Color(0f, 1f, 0.3f);

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

        public LightSource lightsource;
        public float roomDarkness;
        public Vector2 direction;
        public Vector2 lastDirection;
        public Vector2 lazyDirection;
        public Vector2 lastLazyDirection;


        public ReviveSwarmer(ReviveSwarmerAbstract abstr) : base(abstr)
        {
            Abstr = abstr;
            this.collisionLayer = 1;
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), 3f, 0.2f);
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
            base.airFriction = 0.999f;
            base.gravity = 0.9f;
            this.bounce = 0.2f;
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

            this.lastDirection = this.direction;
            this.lastLazyDirection = this.lazyDirection;
            this.lastRotation = this.rotation;

            if (this.lightsource != null && Abstr.isActive)
            {
                this.lightsource.setPos = new Vector2?(base.firstChunk.pos);
                if (this.roomDarkness < 0.2f || this.lightsource.room != this.room)
                {
                    this.room.RemoveObject(this.lightsource);
                    this.lightsource = null;
                }
                else if (this.lightsource.slatedForDeletetion)
                {
                    this.lightsource = null;
                }
            }
            else if (this.roomDarkness >= 0.2f && Abstr.isActive)
            {
                this.lightsource = new LightSource(base.firstChunk.pos, false, NSHswarmerColor, this);
                this.room.AddObject(this.lightsource);
            }

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
            Plugin.Log("reviveSwarmer use:", selectedCreature.GetType().Name, selectedCreature.abstractCreature.ID.number);
            AllGraspsLetGoOfThisObject(true);

            Destroy();
        }



        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[6];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
            sLeaser.sprites[0].scale = 1.5f;
            sLeaser.sprites[0].alpha = 0.2f;
            sLeaser.sprites[1] = new FSprite("JetFishEyeA", true);
            sLeaser.sprites[1].scaleY = 1.2f;
            sLeaser.sprites[1].scaleX = 0.75f;
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[2 + i] = new FSprite("deerEyeA2", true);
                sLeaser.sprites[2 + i].anchorX = 0f;
            }
            sLeaser.sprites[4] = new FSprite("JetFishEyeB", true);
            sLeaser.sprites[5] = new FSprite("Futile_White", true);
            sLeaser.sprites[5].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];

            for (int k = 0; k < sLeaser.sprites.Length; k++)
            {
                sLeaser.sprites[k].color = NSHswarmerColor;
            }
            sLeaser.sprites[4].color = Color.Lerp(NSHswarmerColor, new Color(1f, 1f, 1f), 0.5f);
            AddToContainer(sLeaser, rCam, null);
        }


        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
 
            


            newContainer ??= rCam.ReturnFContainer("Items");
            FContainer fcontainer = rCam.ReturnFContainer("Water");
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                if (i == 0 || i > 4)
                {
                    fcontainer.AddChild(sLeaser.sprites[i]);
                }
                else
                {
                    newContainer.AddChild(sLeaser.sprites[i]);
                }
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            Vector2 vector2 = Vector3.Slerp(this.lastDirection, this.direction, timeStacker);
            Vector2 vector3 = Vector3.Slerp(this.lastLazyDirection, this.lazyDirection, timeStacker);
            Vector3 vector4 = Custom.PerpendicularVector(vector2);
            float num = Mathf.Sin(Mathf.Lerp(this.lastRotation, this.rotation, timeStacker) * 3.1415927f * 2f);
            float num2 = Mathf.Cos(Mathf.Lerp(this.lastRotation, this.rotation, timeStacker) * 3.1415927f * 2f);
            sLeaser.sprites[0].x = vector.x - camPos.x;
            sLeaser.sprites[0].y = vector.y - camPos.y;
            sLeaser.sprites[1].x = vector.x - camPos.x;
            sLeaser.sprites[1].y = vector.y - camPos.y;
            sLeaser.sprites[4].x = vector.x + vector4.x * 2f * num2 * Mathf.Sign(num) - camPos.x;
            sLeaser.sprites[4].y = vector.y + vector4.y * 2f * num2 * Mathf.Sign(num) - camPos.y;
            sLeaser.sprites[1].rotation = Custom.VecToDeg(vector2);
            sLeaser.sprites[4].rotation = Custom.VecToDeg(vector2);
            sLeaser.sprites[4].scaleX = 1f - Mathf.Abs(num2);
            sLeaser.sprites[1].isVisible = true;
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[2 + i].x = vector.x - vector2.x * 4f - camPos.x;
                sLeaser.sprites[2 + i].y = vector.y - vector2.y * 4f - camPos.y;
                sLeaser.sprites[2 + i].rotation = Custom.VecToDeg(vector3) + 90f + ((i == 0) ? -1f : 1f) * Custom.LerpMap(Vector2.Distance(vector2, vector3), 0.06f, 0.7f, 10f, 45f, 2f) * num;
            }
            sLeaser.sprites[2].scaleY = -1f * num;
            sLeaser.sprites[3].scaleY = num;
            float num3 = 1f;
            Vector2 vector5 = vector;
            float num4 = 1f;
            sLeaser.sprites[5].isVisible = false;
            for (int j = 6; j < sLeaser.sprites.Length; j++)
            {
                sLeaser.sprites[j].isVisible = false;
            }
            if (this.lightsource != null)
            {
                this.lightsource.HardSetAlpha((0.3f + 0.7f * Custom.SCurve(Mathf.Pow(Mathf.InverseLerp(15f, 400f, num3), 0.5f), 0.8f) * Mathf.Pow(1f, 0.4f)) * Custom.LerpMap(this.roomDarkness, 0.2f, 0.7f, 0f, 0.5f));
                this.lightsource.HardSetPos(vector5);
                this.lightsource.HardSetRad(Custom.LerpMap(num3, 2f, 15f, 65f, 160f) + Mathf.Lerp(Custom.SCurve(Mathf.InverseLerp(5f, 300f, num3), 0.8f) * 120f, num4, 0.5f) * (0.5f + 0.5f * Mathf.Pow(1f, 0.4f)) * 2f);
            }
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.roomDarkness = palette.darkness;
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

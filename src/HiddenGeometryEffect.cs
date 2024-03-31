using EffExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Caterators_by_syhnne;


// 尝试用一块贴图遮住背景
// 只是一个非常粗略的写法，如果可以的话，换个好点的效果
public class HiddenGeometryEffect : UpdatableAndDeletable
{

    public BackgroundBlocker blocker;
    private bool _setupRan;

    public EffectExtraData EffectData { get; }

    public HiddenGeometryEffect(EffectExtraData effectData)
    {
        EffectData = effectData;
    }

    public override void Update(bool eu)
    {
        bool direction = EffectData.GetBool("direction");
        float position = EffectData.GetFloat("position");
        


        if (!_setupRan)
        {
            Plugin.Log($"Example effect go in room {room.abstractRoom.name} : {this.EffectData.GetString("stringfield")}");
            blocker = new(this);
            room.AddObject(blocker);
            _setupRan = true;
        }
        else if (blocker != null)
        {
            blocker.direction = direction;
            blocker.position = position;
        }


    }




    public class BackgroundBlocker : UpdatableAndDeletable, IDrawable
    {
        public HiddenGeometryEffect owner;
        public bool direction;
        public float position;

        public BackgroundBlocker(HiddenGeometryEffect owner) 
        { 
            this.owner = owner;
        }



        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Shadows");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContainer.AddChild(fsprite);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = Color.white;
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].isVisible = true;
            sLeaser.sprites[0].alpha = 0.5f;
            sLeaser.sprites[0].scaleX = 100f;
            sLeaser.sprites[0].scaleY = 100f * position;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            AddToContainer(sLeaser, rCam, null);
        }
    }
    
}

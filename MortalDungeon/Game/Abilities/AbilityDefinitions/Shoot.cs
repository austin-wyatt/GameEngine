using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Objects;
using OpenTK.Mathematics;
using Empyrean.Game.Map;
using System.Diagnostics;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes;

namespace Empyrean.Game.Abilities
{
    public class Shoot : TemplateRangedSingleTarget
    {
        public Shoot(Unit castingUnit, int range = 6, int minRange = 2, float damage = 10) : base(castingUnit)
        {
            Type = AbilityTypes.RangedAttack;
            DamageType = DamageType.Piercing;
            Range = range;
            MinRange = minRange;
            CastingUnit = castingUnit;

            CastRequirements.AddResourceCost(ResF.ActionEnergy, 2, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            CastingMethod |= CastingMethod.Weapon | CastingMethod.PhysicalDexterity;

            //Name = "Shoot";

            //Description = "Fire an arrow at a target within range. \nA direct line to the target must be present.";

            //Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.BowAndArrow, Spritesheets.IconSheet, true);
        }


        //public override void EnactEffect()
        //{
        //    BeginEffect();
        //    Sound sound = new Sound(Sounds.Shoot) { Gain = 0.75f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
        //    sound.Play();

        //    Casted();

        //    GameObject arrow = new GameObject(Spritesheets.ObjectSheet, 0);

        //    Vector3 a = CastingUnit.Position;
        //    Vector3 b = SelectedUnit.Position;

        //    float angle = (float)MathHelper.RadiansToDegrees(Math.Atan2(a.Y - b.Y, a.X - b.X) - Math.PI / 2);

        //    angle *= -1;

        //    arrow.BaseObject.BaseFrame.RotateZ(angle);

        //    GameObject.LoadTexture(arrow);

        //    arrow.SetPosition(new Vector3(0, 0, -1000));

        //    Scene._genericObjects.Add(arrow);


        //    float dist = Vector3.Distance(SelectedUnit.Position, CastingUnit.Position) / 200;

        //    //int samples = 20;
        //    int samples = (int) dist;

        //    Vector3 delta = (SelectedUnit.Position - CastingUnit.Position) / samples;

        //    TimedAnimation shootAnimation = new TimedAnimation();

        //    shootAnimation.BaseFrame = arrow.BaseObject.BaseFrame;

        //    for (int i = 0; i < samples; i++) 
        //    {
        //        int temp = i;

        //        TimedKeyframe frame = new TimedKeyframe(temp)
        //        {
        //            Action = () =>
        //            {
        //                arrow.SetPosition(CastingUnit.Position + delta * temp + new Vector3(0, 0, 0.2f));
        //            }
        //        };

        //        shootAnimation.Keyframes.Add(frame);
        //    }

        //    shootAnimation.OnFinish = () =>
        //    {
        //        Scene.PostTickEvent += removeArrow;
        //    };

        //    void removeArrow(SceneEventArgs args) 
        //    {
        //        Scene.Tick -= shootAnimation.Tick;
        //        Scene._genericObjects.Remove(arrow);

        //        DamageInstance damage = GetDamageInstance();

        //        SelectedUnit.ApplyDamage(new DamageParams(damage) { Ability = this });

        //        EffectEnded();
        //        Scene.PostTickEvent -= removeArrow;
        //    }


        //    shootAnimation.Play();
        //    Scene.Tick -= shootAnimation.Tick;
        //    Scene.Tick += shootAnimation.Tick;
        //}
    }
}

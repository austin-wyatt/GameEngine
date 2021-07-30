using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Game.Units
{
    public enum UnitTeam 
    {
        Ally,
        Enemy,
        Neutral
    }

    public class Unit : GameObject //main unit class. Tracks position on tilemap
    {
        public int TileMapPosition = -1; //can be anything from -1 to infinity. If the value is below 0 then it is not being positioned on the tilemap
        public Dictionary<int, Ability> Abilities = new Dictionary<int, Ability>();

        public float MaxEnergy = 10;
        public float CurrentEnergy = 10;

        public float EnergyCostMultiplier = 1;
        public float EnergyAddition = 0;
        public float DamageMultiplier = 1;
        public float DamageAddition = 0;
        public float DamageReduction = 0;

        public float Health = 100;
        public const float MaxHealth = 100;

        public int CurrentShields = 0;
        public float ShieldBlock = 10;
        public float DamageBlockedByShield = 0;

        public Dictionary<DamageType, float> DamageResistances = new Dictionary<DamageType, float>();

        public UnitTeam Team = UnitTeam.Ally;

        public bool Dead = false;
        public bool BlocksSpace = true;
        public bool PhasedMovement = false;
        public bool BlocksVision = false;

        public int VisionRadius = 6;

        public Direction Facing = Direction.North;

        public TileMap CurrentTileMap;

        public UnitStatusBar StatusBarComp = null;

        public Ability _movementAbility = null;
        public Unit() 
        {
            Move movement = new Move(this);
            Abilities.Add(movement.AbilityID, movement);

            _movementAbility = movement;

            foreach (DamageType damageType in Enum.GetValues(typeof(DamageType)))
            {
                DamageResistances.Add(damageType, 0);
            }
        }
        public Unit(Vector3 position, int tileMapPosition = 0, int id = 0, string name = "Unit")
        {
            Position = position;
            Name = name;
        }

        public Ability GetFirstAbilityOfType(AbilityTypes type)
        {
            for (int i = 0; i < Abilities.Count; i++)
            {
                if (Abilities[i].Type == type)
                    return Abilities[i];
            }

            return new Ability();
        }

        public List<Ability> GetAbilitiesOfType(AbilityTypes type)
        {
            List<Ability> abilities = new List<Ability>();
            for (int i = 0; i < Abilities.Count; i++)
            {
                if (Abilities[i].Type == type)
                    abilities.Add(Abilities[i]);
            }

            return abilities;
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            if (StatusBarComp != null) 
            {
                StatusBarComp.UpdateUnitStatusPosition();
            }
        }

        public override void SetRender(bool render)
        {
            base.SetRender(render);

            if (StatusBarComp != null) 
            {
                StatusBarComp.SetWillDisplay(render);
            }
        }

        public void OnTurnStart() 
        {
            if (StatusBarComp != null) 
            {
                StatusBarComp.SetIsTurn(true);
            }
        }

        public void OnTurnEnd()
        {
            if (StatusBarComp != null) 
            {
                StatusBarComp.SetIsTurn(false);
            }
        }

        public virtual void SetShields(int shields) 
        {
            CurrentShields = shields;
            StatusBarComp.ShieldBar.SetCurrentShields(CurrentShields);
        }

        public virtual void ApplyDamage(float damage, DamageType damageType)
        {
            float damageMultiplier = Math.Abs(DamageResistances[damageType] - 1);

            float actualDamage = damage * damageMultiplier - DamageReduction;

            float shieldDamageBlocked;

            DamageBlockedByShield += actualDamage;

            if (CurrentShields < 0)
            {
                actualDamage *= 1 + (0.25f * Math.Abs(CurrentShields));
            }
            else 
            {
                shieldDamageBlocked = CurrentShields * ShieldBlock;
                actualDamage -= shieldDamageBlocked;

                if (actualDamage < 0) 
                {
                    actualDamage = 0;
                }
            }

            if (DamageBlockedByShield > ShieldBlock) 
            {
                CurrentShields--;
                DamageBlockedByShield = 0;
            }


            Health -= actualDamage;

            StatusBarComp.HealthBar.SetHealthPercent(Health / MaxHealth);
            StatusBarComp.ShieldBar.SetCurrentShields(CurrentShields);

            if (Health <= 0) 
            {
                Kill();
            }
        }

        public virtual void Kill() 
        {
            Dead = true;
            OnKill();
        }

        public virtual void OnKill() { }

    }
}

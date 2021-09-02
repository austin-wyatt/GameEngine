using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MortalDungeon.Game.Units
{
    public class Unit : GameObject
    {
        public UnitAI AI;
        public UnitInfo Info;
        

        public bool VisibleThroughFog = false;
        

        public CombatScene Scene;

        public UnitStatusBar StatusBarComp = null;
        public static int BaseStatusBarZIndex = 100;

        public bool Selectable = false;

        public bool Selected = false;
        public bool Targeted = false;


        public UnitSelectionTile SelectionTile;


        public Unit(CombatScene scene) 
        {
            AI = new UnitAI(this);
            Info = new UnitInfo(this);

            Scene = scene;

            Hoverable = true;

            Info._visionRadius = 12;

            Move movement = new Move(this);
            Info.Abilities.Add(movement.AbilityID, movement);

            Info._movementAbility = movement;

            SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));
            Scene._genericObjects.Add(SelectionTile);
        }

        public Unit(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) : base(spritesheet, spritesheetPos, position) 
        {
            AI = new UnitAI(this);
            Info = new UnitInfo(this);

            Scene = scene;
            SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));

            AI.Team = UnitTeam.Neutral;
        }

        public Ability GetFirstAbilityOfType(AbilityTypes type)
        {
            foreach (Ability ability in Info.Abilities.Values) 
            {
                if (ability.Type == type)
                    return ability;
            }

            return new Ability();
        }

        public List<Ability> GetAbilitiesOfType(AbilityTypes type)
        {
            List<Ability> abilities = new List<Ability>();

            foreach (Ability ability in Info.Abilities.Values) 
            {
                if (ability.Type == type)
                    abilities.Add(ability);
            }

            return abilities;
        }

        public bool HasAbilityOfType(AbilityTypes type) 
        {
            foreach (Ability ability in Info.Abilities.Values)
            {
                if (ability.Type == type)
                    return true;
            }

            return false;
        }

        public TileMap GetTileMap() 
        {
            return Info.TileMapPosition.TilePoint.ParentTileMap;
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            if (StatusBarComp != null) 
            {
                StatusBarComp.UpdateUnitStatusPosition();
            }

            if (SelectionTile != null) 
            {
                SelectionTile.SetPosition(position);
            }
        }

        public override void SetRender(bool render)
        {
            base.SetRender(render);

            if (StatusBarComp != null) 
            {
                StatusBarComp.SetWillDisplay(render && !Info.Dead && Scene.DisplayUnitStatuses);
            }

            if (Targeted && render)
            {
                SelectionTile.Target();
            }
            else if (Selected && render)
            {
                SelectionTile.Select();
            }
            else 
            {
                SelectionTile.SetRender(false);
            }
        }

        public virtual void SetTileMapPosition(BaseTile baseTile) 
        {
            BaseTile prevTile = Info.TileMapPosition;

            if (prevTile != null)
                prevTile.UnitOnTile = null;

            baseTile.UnitOnTile = this;

            Info.TileMapPosition = baseTile;

            Scene.OnUnitMoved(this);
        }


        public virtual float GetBuffResistanceModifier(DamageType damageType) 
        {
            float modifier = 0;

            for (int i = 0; i < Info.Buffs.Count; i++) 
            {
                Info.Buffs[i].DamageResistances.TryGetValue(damageType, out float val);
                modifier += val;
            }

            return modifier;
        }

        public bool GetPierceShields(DamageType damageType) 
        {
            return damageType switch
            {
                DamageType.Poison => true,
                _ => false,
            };
        }

        public bool GetAmplifiedByNegativeShields(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Bleed:
                case DamageType.Poison:
                    return false;
                default:
                    return true;
            }
        }

        public void OnTurnStart() 
        {
            if (StatusBarComp != null) 
            {
                StatusBarComp.SetIsTurn(true);
            }

            if (AI.ControlType != ControlType.Controlled) 
            {
                AI.TakeTurn();
            }
        }

        public void OnTurnEnd()
        {
            if (StatusBarComp != null) 
            {
                StatusBarComp.SetIsTurn(false);
            }
        }

        public virtual void SetTeam(UnitTeam team) 
        {
            AI.Team = team;

            switch (team) 
            {
                case UnitTeam.Ally:
                    SelectionTile.SetColor(new Vector4(0, 0.75f, 0, 1));
                    break;
                case UnitTeam.Enemy:
                    SelectionTile.SetColor(new Vector4(0.75f, 0, 0, 1));
                    break;
                case UnitTeam.Neutral:
                    SelectionTile.SetColor(Colors.Tan);
                    break;
            }
        }

        public virtual void SetShields(int shields) 
        {
            Info.CurrentShields = shields;
            StatusBarComp.ShieldBar.SetCurrentShields(Info.CurrentShields);
        }

        public virtual void ApplyDamage(float damage, DamageType damageType)
        {
            Info.BaseDamageResistances.TryGetValue(damageType, out float baseResistance);

            float damageMultiplier = Math.Abs((baseResistance + GetBuffResistanceModifier(damageType)) - 1);

            
            float actualDamage = damage * damageMultiplier + Info.DamageAddition;

            if (actualDamage > Info.Stealth.Skill && Info.Stealth.Hiding)
            {
                Info.Stealth.SetHiding(false);
            }

            //shield piercing exceptions should go here
            if (GetPierceShields(damageType))
            {

            }
            else 
            {
                float shieldDamageBlocked;

                Info.DamageBlockedByShields += actualDamage;

                if (Info.CurrentShields < 0 && GetAmplifiedByNegativeShields(damageType))
                {
                    actualDamage *= 1 + (0.25f * Math.Abs(Info.CurrentShields));
                }
                else if (Info.CurrentShields > 0)
                {
                    shieldDamageBlocked = Info.CurrentShields * Info.ShieldBlock;
                    actualDamage -= shieldDamageBlocked;

                    if (actualDamage < 0)
                    {
                        actualDamage = 0;
                    }
                }

                if (Info.DamageBlockedByShields > Info.ShieldBlock) 
                {
                    Info.CurrentShields--;
                    Info.DamageBlockedByShields = 0;
                }
            }


            Info.Health -= actualDamage;


            StatusBarComp.HealthBar.SetHealthPercent(Info.Health / UnitInfo.MaxHealth, AI.Team);
            StatusBarComp.ShieldBar.SetCurrentShields(Info.CurrentShields);
            Scene.Footer.UpdateFooterInfo(Scene.Footer._currentUnit);

            if (Info.Health <= 0) 
            {
                Kill();
            }
        }

        public virtual void Kill() 
        {
            Info.Dead = true;
            OnKill();
        }

        public virtual void OnKill() 
        {
            if (StatusBarComp != null) 
            {
                Scene.OnUnitKilled(this);
            }
        }

        public override void OnHover()
        {
            if (Hoverable && !Hovered)
            {
                Hovered = true;

                if (StatusBarComp != null && StatusBarComp.Render) 
                {
                    StatusBarComp.ZIndex = BaseStatusBarZIndex + 1;
                    StatusBarComp.Parent.Children.Sort();
                }

                //Scene.Footer.UpdateFooterInfo(this);
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();

            //remove the objects that are related to the unit but not created by the unit
            Scene._genericObjects.Remove(SelectionTile);
            Scene.RemoveUI(StatusBarComp);
        }

        public override void HoverEnd()
        {
            if (Hovered)
            {
                Hovered = false;
                if (StatusBarComp != null && StatusBarComp.Render)
                {
                    StatusBarComp.ZIndex = BaseStatusBarZIndex;
                    StatusBarComp.Parent.Children.Sort();
                }

                base.HoverEnd();
            }
        }

        public override void OnTimedHover()
        {
            base.OnTimedHover();
        }

        public void Select() 
        {
            Scene.Footer.UpdateFooterInfo(this);
            SelectionTile.Select();
            Selected = true;
        }

        public void Deselect()
        {
            Scene.Footer.UpdateFooterInfo(Scene.Footer._currentUnit);
            SelectionTile.Deselect();
            Selected = false;
        }

        public void Target() 
        {
            SelectionTile.Target();
            Targeted = true;
        }

        public void Untarget() 
        {
            SelectionTile.Untarget();
            Targeted = false;
        }

        public override void Tick()
        {
            base.Tick();

            if (Info.Stealth.HidingBrokenActions.HasQueuedItems()) 
            {
                Info.Stealth.HidingBrokenActions.HandleQueuedItems();
            }
        }

        public virtual BaseObject CreateBaseObject() 
        {
            return null;
        }
    }

    public class UnitInfo
    {
        public static int OUT_OF_COMBAT_VISION = 5;
        public UnitInfo(Unit unit) 
        {
            Unit = unit;

            Stealth = new Hidden(unit);
            Scouting = new Scouting(unit);
        }

        public Unit Unit;
        public BaseTile TileMapPosition;

        public TilePoint TemporaryPosition = null; //used as a placeholder position for calculating things like vision before a unit moves

        public TilePoint Point => TileMapPosition.TilePoint;
        public CombatScene Scene => TileMapPosition.GetScene();
        public TileMap Map => TileMapPosition.TileMap;

        public Dictionary<int, Ability> Abilities = new Dictionary<int, Ability>();
        public List<Buff> Buffs = new List<Buff>();

        public Move _movementAbility = null;

        public float MaxEnergy = 10;
        public float CurrentEnergy => MaxEnergy + Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.EnergyBoost.Additive + seed); //Energy at the start of the turn
        public float Energy = 0; //internal unit energy tracker

        public float EnergyCostMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.EnergyCost.Multiplier * seed);
        public float EnergyAddition => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.EnergyCost.Additive + seed);
        public float DamageMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.OutgoingDamage.Multiplier * seed);
        public float DamageAddition => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.OutgoingDamage.Additive + seed);
        public float SpeedMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.SpeedModifier.Multiplier * seed);
        public float SpeedAddition => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.SpeedModifier.Additive + seed);



        public float Speed => _movementAbility != null ? _movementAbility.GetEnergyCost() : 10;

        public float Health = 100;
        public const float MaxHealth = 100;

        public int CurrentShields = 0;

        public float ShieldBlockMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.ShieldBlock.Multiplier * seed);

        public float ShieldBlock => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.ShieldBlock.Additive + seed) * ShieldBlockMultiplier;

        public float DamageBlockedByShields = 0;

        public Dictionary<DamageType, float> BaseDamageResistances = new Dictionary<DamageType, float>();

        public bool NonCombatant = false;

        public bool PrimaryUnit = false;

        public int Height = 1;
        public int VisionRadius => _visionRadius + (!Scene.InCombat && Unit.AI.ControlType == ControlType.Controlled ? OUT_OF_COMBAT_VISION : 0);
        public int _visionRadius = 6;

        public Direction Facing = Direction.North;

        public bool Dead = false;
        public bool BlocksSpace = true;
        public bool PhasedMovement = false;
        public bool BlocksVision = false;

        public bool Visible(UnitTeam team) 
        {
            if (Unit.VisibleThroughFog && TileMapPosition.Explored[team])
                return true;

            return !TileMapPosition.InFog[team] && Stealth.Revealed[team];
        }


        public Hidden Stealth;
        public Scouting Scouting;
    }

    public class Hidden 
    {
        private Unit Unit;
        /// <summary>
        /// Whether a unit is currently attemping to hide
        /// </summary>
        public bool Hiding = false;

        /// <summary>
        /// The teams that can see this unit
        /// </summary>
        public Dictionary<UnitTeam, bool> Revealed = new Dictionary<UnitTeam, bool>();

        public float Skill = 0;

        public QueuedList<Action> HidingBrokenActions = new QueuedList<Action>();

        public Hidden(Unit unit) 
        {
            Unit = unit;

            foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam))) 
            {
                Revealed.TryAdd(team, true);
            }
        }

        public void SetHiding(bool hiding) 
        {
            if (Hiding && !hiding) 
            {
                Hiding = false;
                HidingBroken();
            }
            else 
            {
                Hiding = hiding;
            }
        }

        private void HidingBroken() 
        {
            SetAllRevealed();
            HidingBrokenActions.ForEach(a => a.Invoke());
        }

        public void SetAllRevealed(bool revealed = true) 
        {
            foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam)))
            {
                Revealed[team] = revealed;
            }

            Revealed[Unit.AI.Team] = true;
        }

        public void SetRevealed(UnitTeam team, bool revealed) 
        {
            Revealed[team] = revealed;
        }

        /// <summary>
        /// Returns false if any team that isn't the unit's team has vision of the space
        /// </summary>
        /// <returns></returns>
        public bool EnemyHasVision()
        {
            bool hasVision = false;

            foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam)))
            {
                if (team != Unit.AI.Team && !Unit.Info.TileMapPosition.InFog[team])
                {
                    hasVision = true;
                }
            }

            return hasVision;
        }

        public bool PositionInFog(UnitTeam team) 
        {
            bool inFog = true;

            if (team != Unit.AI.Team && !Unit.Info.TileMapPosition.InFog[team]) 
            {
                inFog = false;
            }

            return inFog;
        }
    }

    public class Scouting 
    {
        private Unit Unit;

        public const int DEFAULT_RANGE = 5;

        public int Skill = 0;

        public Scouting(Unit unit) 
        {
            Unit = unit;
        }

        /// <summary>
        /// Calculates whether a unit can scout a hiding unit. This does not take into account whether the tiles are actually/would be in vision.
        /// </summary>
        public bool CouldSeeUnit(Unit unit, int distance)
        {
            if (!unit.Info.Stealth.Hiding)
                return true;

            return Skill - unit.Info.Stealth.Skill - (distance - DEFAULT_RANGE) >= 0;
        }
    }
}

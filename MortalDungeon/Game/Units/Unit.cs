using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Lighting;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Particles;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MortalDungeon.Game.Units
{
    public class Unit : GameObject, ILoadableEntity
    {
        public UnitAI AI;
        public UnitInfo Info;


        public VisionGenerator VisionGenerator = new VisionGenerator();
        public LightObstruction LightObstruction = new LightObstruction();

        public bool VisibleThroughFog = false;
        
        public CombatScene Scene;

        public UnitStatusBar StatusBarComp = null;
        public static int BaseStatusBarZIndex = 100;

        public bool Selectable = false;

        public bool Selected = false;
        public bool Targeted = false;

        public Entity EntityHandle;
        

        public UnitSelectionTile SelectionTile;

        public bool _createStatusBar = false;
        public Unit() { }

        public Unit(CombatScene scene) 
        {
            InitializeUnitInfo();

            Scene = scene;

            Hoverable = true;

            //SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));
            //Scene._genericObjects.Add(SelectionTile);
        }

        public Unit(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) : base(spritesheet, spritesheetPos, position) 
        {
            InitializeUnitInfo();

            Scene = scene;
            SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));

            SetTeam(UnitTeam.Unknown);
        }

        public virtual void InitializeUnitInfo() 
        {
            AI = new UnitAI(this);
            Info = new UnitInfo(this);
        }

        /// <summary>
        /// Create and add the base object to the unit
        /// </summary>
        public virtual void InitializeVisualComponent()
        {

        }

        public virtual void EntityLoad(FeaturePoint position) 
        {
            Position = new Vector3();

            
            SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));
            SetSelectionTileColor();

            if (VisionGenerator.Team != UnitTeam.Unknown && !Scene.UnitVisionGenerators.Contains(VisionGenerator)) 
            {
                Scene.UnitVisionGenerators.Add(VisionGenerator);
            }

            Scene._genericObjects.Add(SelectionTile);

            InitializeVisualComponent();

            if (StatusBarComp == null && _createStatusBar) 
            {
                new UnitStatusBar(this, Scene._camera);
            }

            if (StatusBarComp != null) 
            {
                Scene.AddUI(StatusBarComp, 10);
            }


            SetTileMapPosition(Scene._tileMapController.GetTile(position));
        }

        public virtual void EntityUnload() 
        {
            CleanUp();

            Info.TileMapPosition = null;

            //might not wanna clear buffs or abilities depending on how things shake out.
            lock (Info.Abilities) 
            {
                Info.Abilities.Clear();
            }
            lock (Info.Buffs)
            {
                Info.Buffs.Clear();
            }

            BaseObjects.Clear();
        }
        public override void CleanUp()
        {
            base.CleanUp();

            //remove the objects that are related to the unit but not created by the unit
            Scene._genericObjects.Remove(SelectionTile);
            Scene.RemoveUI(StatusBarComp);

            SelectionTile = null;

            Scene.LightObstructions.Remove(LightObstruction);
            Scene.UnitVisionGenerators.Remove(VisionGenerator);

            Scene.DecollateUnit(this);
            Scene.RemoveUnit(this);
        }

        public override void AddBaseObject(BaseObject obj)
        {
            base.AddBaseObject(obj);
        }

        public Ability GetFirstAbilityOfType(AbilityTypes type)
        {
            foreach (Ability ability in Info.Abilities) 
            {
                if (ability.Type == type)
                    return ability;
            }

            return new Ability();
        }

        public List<Ability> GetAbilitiesOfType(AbilityTypes type)
        {
            List<Ability> abilities = new List<Ability>();

            foreach (Ability ability in Info.Abilities) 
            {
                if (ability.Type == type)
                    abilities.Add(ability);
            }

            return abilities;
        }

        public bool HasAbilityOfType(AbilityTypes type) 
        {
            foreach (Ability ability in Info.Abilities)
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

        public override void SetName(string name)
        {
            base.SetName(name);

            UpdateStatusBarInfo();
        }

        public void UpdateStatusBarInfo() 
        {
            if (StatusBarComp != null)
            {
                StatusBarComp.UpdateInfo();
            }
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

            if (SelectionTile != null)
            {
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
        }

        public virtual void SetTileMapPosition(BaseTile baseTile) 
        {
            BaseTile prevTile = Info.TileMapPosition;

            if (prevTile != null)
                prevTile.UnitOnTile = null;

            baseTile.UnitOnTile = this;

            Info.TileMapPosition = baseTile;

            VisionGenerator.SetPosition(baseTile.TilePoint);
            LightObstruction.SetPosition(baseTile);

            Scene.OnUnitMoved(this, prevTile);
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

            SetSelectionTileColor();

            UpdateStatusBarInfo();

            if (VisionGenerator.Team != UnitTeam.Unknown && team == UnitTeam.Unknown)
            {
                Scene.UnitVisionGenerators.Remove(VisionGenerator);
            }
            else if (VisionGenerator.Team == UnitTeam.Unknown && team != UnitTeam.Unknown) 
            {
                Scene.UnitVisionGenerators.Add(VisionGenerator);
            }

            VisionGenerator.Team = team;
        }

        public void SetSelectionTileColor() 
        {
            if (SelectionTile == null)
                return;

            switch (UnitTeam.PlayerUnits.GetRelation(AI.Team))
            {
                case Relation.Friendly:
                    SelectionTile.SetColor(new Vector4(0, 0.75f, 0, 1));
                    break;
                case Relation.Hostile:
                    SelectionTile.SetColor(new Vector4(0.75f, 0, 0, 1));
                    break;
                case Relation.Neutral:
                    SelectionTile.SetColor(Colors.Tan);
                    break;
                default:
                    SelectionTile.SetColor(new Vector4(0, 0, 0.75f, 1));
                    break;
            }
        }

        public virtual void SetShields(int shields) 
        {
            Info.CurrentShields = shields;

            if (StatusBarComp != null) 
            {
                StatusBarComp.ShieldBar.SetCurrentShields(Info.CurrentShields);
            }
        }


        public struct DamageParams 
        {
            public DamageInstance Instance;
            public Ability Ability;
            public Buff Buff;

            public DamageParams(DamageInstance instance)
            {
                Instance = instance;

                Ability = null;
                Buff = null;
            }
        }
        public virtual void ApplyDamage(DamageParams damageParams)
        {
            float preShieldDamage = 0;
            float finalDamage = 0;

            DamageInstance instance = damageParams.Instance;

            foreach (DamageType type in instance.Damage.Keys)
            {
                Info.BaseDamageResistances.TryGetValue(type, out float baseResistance);

                float damageMultiplier = Math.Abs((baseResistance + GetBuffResistanceModifier(type)) - 1);

                float actualDamage = instance.Damage[type] * damageMultiplier + Info.DamageAddition;

                preShieldDamage += actualDamage;
            

                //shield piercing exceptions should go here
                if (GetPierceShields(type))
                {

                }
                else
                {
                    if (type == DamageType.Piercing)
                    {
                        float piercingDamage = damageParams.Instance.PiercingPercent * actualDamage;

                        actualDamage -= piercingDamage;
                        finalDamage += piercingDamage;
                    }


                    float shieldDamageBlocked;

                    Info.DamageBlockedByShields += actualDamage;

                    if (Info.CurrentShields < 0 && GetAmplifiedByNegativeShields(type))
                    {
                        actualDamage *= 1 + (0.25f * Math.Abs(Info.CurrentShields));
                    }
                    else if (Info.CurrentShields > 0)
                    {
                        shieldDamageBlocked = Info.CurrentShields * Info.ShieldBlock;

                        actualDamage -= shieldDamageBlocked;

                        if (actualDamage <= 0)
                        {
                            actualDamage = 0;

                            OnShieldsHit();
                        }
                    }

                    if (Info.DamageBlockedByShields > Info.ShieldBlock)
                    {
                        Info.CurrentShields--;
                        Info.DamageBlockedByShields = 0;
                    }
                }

                //if (type == DamageType.Piercing)
                //{
                //    float piercingDamage = damageParams.Instance.PierceAmount;
                //    //if (damageParams.Ability != null)
                //    //{
                //    //    piercingDamage += damageParams.Ability.Grade;
                //    //}

                //    //if (damageParams.Buff != null)
                //    //{
                //    //    piercingDamage += damageParams.Buff.Grade;
                //    //}

                //    actualDamage += piercingDamage;
                //}

                finalDamage += actualDamage;
            }

            if (preShieldDamage > Info.Stealth.Skill && Info.Stealth.Hiding)
            {
                Info.Stealth.SetHiding(false);
            }

            Info.Health -= finalDamage;

            if (finalDamage > 0) 
            {
                OnHurt();
            }


            StatusBarComp.HealthBar.SetHealthPercent(Info.Health / Info.MaxHealth, AI.Team);
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

        public virtual void Revive()
        {
            Info.Dead = false;
            Info.Health = 1;

            StatusBarComp.ShieldBar.SetCurrentShields(Info.CurrentShields);

            OnRevive();
        }

        public virtual void OnKill() 
        {
            if (BaseObject != null) 
            {
                BaseObject.SetAnimation(AnimationType.Die);
            }

            if (StatusBarComp != null) 
            {
                Scene.OnUnitKilled(this);
            }

            Sound sound = new Sound(Sounds.Die) { Gain = 0.2f, Pitch = 1 };
            sound.Play();
        }

        public virtual void OnRevive() 
        {
            BaseObject.SetAnimation(0);
        }

        public virtual void OnHurt() 
        {
            Sound sound = new Sound(Sounds.UnitHurt) { Gain = 1f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
            sound.Play();

            var bloodExplosion = new Explosion(Position, new Vector4(1, 0, 0, 1), Explosion.ExplosionParams.Default);
            bloodExplosion.OnFinish = () =>
            {
                Scene._particleGenerators.Remove(bloodExplosion);
            };

            Scene._particleGenerators.Add(bloodExplosion);
        }

        public virtual void OnShieldsHit() 
        {
            if (Info.DamageBlockedByShields > Info.ShieldBlock)
            {
                Sound sound = new Sound(Sounds.ArmorHit) { Gain = 1f, Pitch = GlobalRandom.NextFloat(0.6f, 0.7f) };
                sound.Play();
            }
            else 
            {
                Sound sound = new Sound(Sounds.ArmorHit) { Gain = 1f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
                sound.Play();
            }

            var bloodExplosion = new Explosion(Position, new Vector4(0.592f, 0.631f, 0.627f, 1), Explosion.ExplosionParams.Default);
            bloodExplosion.OnFinish = () =>
            {
                Scene._particleGenerators.Remove(bloodExplosion);
            };

            Scene._particleGenerators.Add(bloodExplosion);
        }

        public override void OnHover()
        {
            if (Hoverable && !Hovered)
            {
                Hovered = true;

                if (StatusBarComp != null && StatusBarComp.Render) 
                {
                    StatusBarComp.ZIndex = BaseStatusBarZIndex + 1;
                    Scene.SortUIByZIndex();

                    //StatusBarComp.Parent.Children.Sort();
                }

                //Scene.Footer.UpdateFooterInfo(this);
            }
        }

        public override void OnHoverEnd()
        {
            if (Hovered)
            {
                Hovered = false;
                if (StatusBarComp != null && StatusBarComp.Render)
                {
                    StatusBarComp.ZIndex = BaseStatusBarZIndex;
                    Scene.SortUIByZIndex();
                    //StatusBarComp.Parent.Children.Sort();
                }

                base.OnHoverEnd();
            }
        }

        public override void OnTimedHover()
        {
            base.OnTimedHover();
        }

        public override void OnCull()
        {
            if (Cull)
            {
                Scene.DecollateUnit(this);

                if (StatusBarComp != null) 
                {
                    StatusBarComp.Cull = Cull;
                }
            }
            else 
            {
                Scene.CollateUnit(this);

                if (StatusBarComp != null)
                {
                    StatusBarComp.Cull = Cull;
                }
            }
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

        public bool OnTileMap(TileMap map) 
        {
            return Info != null && Info.TileMapPosition != null && Info.TileMapPosition.TileMap == map;
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

        public List<Ability> Abilities = new List<Ability>();
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
        public float MaxHealth = 100;

        public int CurrentShields = 0;

        public float ShieldBlockMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.ShieldBlock.Multiplier * seed);

        public float ShieldBlock => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.ShieldBlock.Additive + seed) * ShieldBlockMultiplier;

        public float DamageBlockedByShields = 0;

        public Dictionary<DamageType, float> BaseDamageResistances = new Dictionary<DamageType, float>();

        public bool NonCombatant = false;

        public bool PrimaryUnit = false;

        public int Height = 1;
        //public int VisionRadius => _visionRadius + (!Scene.InCombat && Unit.AI.ControlType == ControlType.Controlled ? OUT_OF_COMBAT_VISION : 0);
        //public int _visionRadius = 6;

        public Direction Facing = Direction.North;

        public bool Dead = false;
        public bool BlocksSpace = true;
        public bool PhasedMovement = false;

        /// <summary>
        /// If true, this unit/structure can always be seen through
        /// </summary>
        public bool Transparent = false;

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

        public float Skill = 0;

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

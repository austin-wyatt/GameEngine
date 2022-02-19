using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Items;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Units
{
    public enum Species
    {
        Humanoid,
        Beast,
        Bug,
        Vampir,
        Elemental,
        Undead,
        Spirit,
        Plant,
        Automata,
        Divine
    }

    public enum StatusCondition
    {
        None,
        Stunned = 1, //disables all
        Silenced = 2, //disables vocal
        Weakened = 4, //disables brute force
        Debilitated = 8, //disables dexterity
        Disarmed = 16, //disables weapon
        MagicBlocked = 64, //disables magic
        Confused = 128, //disables intelligence
        Exposed = 256, //disables passives
        Rooted = 512, //disables movement
        Blinded = 1024, //reduces vision radius by a certain amount
    }

    /// <summary>
    /// These are flags inherent to abilities/buffs <para/>
    /// This will be a very large enum.
    /// </summary>
    public enum UnitConditions
    {
        WebImmuneWeak,
        WebImmuneMed,
        WebImmuneStrong,
    }

    [Serializable]
    public class UnitInfo : ISerializable
    {
        public static int OUT_OF_COMBAT_VISION = 5;

        public UnitInfo() { }

        public UnitInfo(Unit unit)
        {
            Unit = unit;

            Stealth = new Hidden(unit);
            Scouting = new Scouting(unit);
            Equipment = new Equipment(unit);
        }


        [XmlIgnore]
        public Unit Unit;

        [XmlIgnore]
        public BaseTile TileMapPosition;

        [XmlIgnore]
        public TilePoint TemporaryPosition = null; //used as a placeholder position for calculating things like vision before a unit moves

        [XmlIgnore]
        public TilePoint Point => TileMapPosition.TilePoint;

        [XmlIgnore]
        public CombatScene Scene => TileMapPosition.GetScene();

        [XmlIgnore]
        public TileMap Map => TileMapPosition.TileMap;

        [XmlIgnore]
        public List<Ability> Abilities = new List<Ability>();

        public Equipment Equipment;
        public Inventory Inventory = new Inventory();

        //public List<AbilityCreationInfo> _abilityCreationInfo = new List<AbilityCreationInfo>();

        [XmlIgnore]
        public List<Buff> Buffs = new List<Buff>();

        //private List<BuffCreationInfo> _buffCreationInfo = new List<BuffCreationInfo>();

        [XmlIgnore]
        public Move _movementAbility = null;

        public float MaxEnergy = 10;
        public float CurrentEnergy => MaxEnergy + Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.EnergyBoost.Additive + seed); //Energy at the start of the turn
        public float Energy = 10; //public unit energy tracker

        public float MaxActionEnergy = 4;

        public float MaxFocus = 40;
        public float Focus = 40;

        public float CurrentActionEnergy => MaxActionEnergy + Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.ActionEnergyBoost.Additive + seed); //Energy at the start of the turn
        public float ActionEnergy = 0;

        public float EnergyCostMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.EnergyCost.Multiplier * seed);
        public float EnergyAddition => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.EnergyCost.Additive + seed);
        public float ActionEnergyAddition => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.EnergyCost.Additive + seed);
        public float DamageMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.OutgoingDamage.Multiplier * seed);
        public float DamageAddition => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.OutgoingDamage.Additive + seed);
        public float SpeedMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.SpeedModifier.Multiplier * seed);
        public float SpeedAddition => Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.SpeedModifier.Additive + seed);



        public float Speed => _movementAbility != null ? _movementAbility.GetEnergyCost() : 10;

        public float Health = 100;
        public float MaxHealth = 100;

        public int CurrentShields = 0;

        public float ShieldBlockMultiplier => Buffs.Aggregate<Buff, float>(1, (seed, buff) => buff.ShieldBlock.Multiplier * seed);

        public float ShieldBlock
        {
            get
            {
                float shieldBlock = Buffs.Aggregate<Buff, float>(0, (seed, buff) => buff.ShieldBlock.Additive + seed);

                shieldBlock += ApplyBuffAdditiveShieldBlockModifications();

                shieldBlock *= ShieldBlockMultiplier;
                return shieldBlock;
            }
        }


        public float DamageBlockedByShields = 0;

        [XmlIgnore]
        public Dictionary<DamageType, float> BaseDamageResistances = new Dictionary<DamageType, float>();

        [XmlElement(Namespace = "UIbdr")]
        private DeserializableDictionary<DamageType, float> _baseDamageResistances = new DeserializableDictionary<DamageType, float>();

        public bool NonCombatant = false;

        public int Height = 1;
        //public int VisionRadius => _visionRadius + (!Scene.InCombat && Unit.AI.ControlType == ControlType.Controlled ? OUT_OF_COMBAT_VISION : 0);
        //public int _visionRadius = 6;

        public Direction Facing = Direction.North;

        public bool Dead = false;
        public bool BlocksSpace = true;
        public bool PhasedMovement = false;

        public StatusCondition Status = StatusCondition.None;

        public Species Species = Species.Humanoid;

        public bool PartyMember = false;

        /// <summary>
        /// Tracks all of the innate/ability/buff related conditions affecting a unit <para/>
        /// This will need some special handling regarding saving and loading to ensure conditions don't remain permanently
        /// </summary>
        public HashSet<UnitConditions> UnitConditions = new HashSet<UnitConditions>();


        /// <summary>
        /// If true, this unit/structure can always be seen through
        /// </summary>
        public bool Transparent = false;

        public bool Visible(UnitTeam team)
        {
            if (TileMapPosition == null)
                return false;

            if (Unit.VisibleThroughFog && TileMapPosition.Explored(team))
                return true;

            return !TileMapPosition.InFog(team) && Stealth.Revealed[team];
        }


        public Hidden Stealth;
        public Scouting Scouting;

        [XmlIgnore]
        private object _buffLock = new object();
        public void AddBuff(Buff buff)
        {
            if (buff == null)
                return;

            lock (_buffLock)
            {
                Buffs.Add(buff);
                buff.AffectedUnit = Unit;

                EvaluateStatusCondition();

                Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground;

                switch (buff.BuffType)
                {
                    case BuffType.Debuff:
                        backgroundType = Icon.BackgroundType.DebuffBackground;
                        break;
                    case BuffType.Buff:
                        backgroundType = Icon.BackgroundType.BuffBackground;
                        break;
                }

                Icon icon = buff.GenerateIcon(new UIScale(0.5f * WindowConstants.AspectRatio, 0.5f), true, backgroundType);
                Vector3 pos = Unit.Position + new Vector3(0, -400, 0.3f);
                UIHelpers.CreateIconHoverEffect(icon, Scene, pos);
            }
        }

        public void RemoveBuff(Buff buff)
        {
            if (buff == null)
                return;

            lock (_buffLock)
            {
                Buffs.Remove(buff);
                buff.AffectedUnit = null;

                EvaluateStatusCondition();

                Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground;

                switch (buff.BuffType)
                {
                    case BuffType.Debuff:
                        backgroundType = Icon.BackgroundType.DebuffBackground;
                        break;
                    case BuffType.Buff:
                        backgroundType = Icon.BackgroundType.BuffBackground;
                        break;
                }

                Icon icon = buff.GenerateIcon(new UIScale(0.5f * WindowConstants.AspectRatio, 0.5f), true, backgroundType);
                Vector3 pos = Unit.Position + new Vector3(0, -400, 0.3f);
                UIHelpers.CreateIconHoverEffect(icon, Scene, pos);
            }
        }

        private float ApplyBuffAdditiveShieldBlockModifications()
        {
            float modifications = 0;
            foreach (var buff in Unit.Info.Buffs)
            {
                modifications += buff.ModifyShieldBlockAdditive(Unit);
            }

            return modifications;
        }

        public void EvaluateStatusCondition()
        {
            StatusCondition condition = StatusCondition.None;

            for (int i = 0; i < Buffs.Count; i++)
            {
                condition |= Buffs[i].StatusCondition;
            }

            Status = condition;
        }

        public bool CanUseAbility(Ability ability)
        {
            CastingMethod method = ability.CastingMethod;

            int propertyCount = Enum.GetValues(typeof(CastingMethod)).Length;

            for (int i = 0; i < propertyCount; i++)
            {
                if (BitOps.GetBit((int)method, i) && BitOps.GetBit((int)Status, i))
                    return false;
            }

            return true;
        }

        public void PrepareForSerialization()
        {
            Stealth.PrepareForSerialization();
            Scouting.PrepareForSerialization();
            Equipment.PrepareForSerialization();

            //foreach (var item in _abilityCreationInfo)
            //{
            //    item.PrepareForSerialization();
            //}

            _baseDamageResistances = new DeserializableDictionary<DamageType, float>(BaseDamageResistances);
        }

        public void CompleteDeserialization()
        {
            Stealth.CompleteDeserialization();
            Scouting.CompleteDeserialization();
            Equipment.CompleteDeserialization();

            //foreach (var item in _abilityCreationInfo)
            //{
            //    item.CompleteDeserialization();
            //}

            BaseDamageResistances.Clear();
            _baseDamageResistances.FillDictionary(BaseDamageResistances);
        }

        public static void AttachUnitToInfo(UnitInfo info, Unit unit)
        {
            info.Unit = unit;
            info.Stealth.Unit = unit;
            info.Scouting.Unit = unit;
            info.Equipment.Unit = unit;
        }
    }

    [Serializable]
    public class Hidden : ISerializable
    {
        [XmlIgnore]
        public Unit Unit;
        /// <summary>
        /// Whether a unit is currently attemping to hide
        /// </summary>
        public bool Hiding = false;

        /// <summary>
        /// The teams that can see this unit
        /// </summary>
        [XmlIgnore]
        public Dictionary<UnitTeam, bool> Revealed = new Dictionary<UnitTeam, bool>();
        [XmlElement(Namespace = "Hir")]
        private DeserializableDictionary<UnitTeam, bool> _revealed = new DeserializableDictionary<UnitTeam, bool>();

        public float Skill = 0;

        [XmlIgnore]
        public QueuedList<Action> HidingBrokenActions = new QueuedList<Action>();

        public Hidden() { }
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
                if (team != Unit.AI.Team && !Unit.Info.TileMapPosition.InFog(team))
                {
                    hasVision = true;
                }
            }

            return hasVision;
        }

        public bool PositionInFog(UnitTeam team)
        {
            bool inFog = true;

            if (team != Unit.AI.Team && !Unit.Info.TileMapPosition.InFog(team))
            {
                inFog = false;
            }

            return inFog;
        }

        public void PrepareForSerialization()
        {
            _revealed = new DeserializableDictionary<UnitTeam, bool>(Revealed);
        }

        public void CompleteDeserialization()
        {
            _revealed.FillDictionary(Revealed);
        }
    }

    [Serializable]
    public class Scouting : ISerializable
    {
        [XmlIgnore]
        public Unit Unit;

        public const int DEFAULT_RANGE = 5;

        public float Skill = 0;

        public Scouting() { }
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

        public void PrepareForSerialization()
        {

        }

        public void CompleteDeserialization()
        {

        }
    }
}

using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Items;
using Empyrean.Game.Player;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Units
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
    public enum UnitCondition
    {
        None,
        WebImmuneWeak,
        WebImmuneMed,
        WebImmuneStrong,
    }

    /// <summary>
    /// Short term context values for 
    /// </summary>
    public enum UnitContext
    {
        WeaponSwappedThisTurn
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

            AttachUnitToInfo(this, unit);
        }


        [XmlIgnore]
        public Unit Unit;

        [XmlIgnore]
        public Tile TileMapPosition;

        [XmlIgnore]
        public TilePoint TemporaryPosition = null; //used as a placeholder position for calculating things like vision before a unit moves

        [XmlIgnore]
        public TilePoint Point => TileMapPosition.TilePoint;

        [XmlIgnore]
        public CombatScene Scene => TileMapManager.Scene;

        [XmlIgnore]
        public TileMap Map => TileMapPosition.TileMap;

        [XmlIgnore]
        public List<Ability> Abilities = new List<Ability>();

        [XmlIgnore]
        public UnitGroup Group = null;

        [XmlIgnore]
        public ContextManager<UnitContext> Context = new ContextManager<UnitContext>();

        public Equipment Equipment;
        public Inventory Inventory = new Inventory();

        public ResourceManager ResourceManager = new ResourceManager();

        //public List<AbilityCreationInfo> _abilityCreationInfo = new List<AbilityCreationInfo>();

        public BuffManager BuffManager = new BuffManager();

        [XmlIgnore]
        public StatusManager StatusManager = new StatusManager();

        //private List<BuffCreationInfo> _buffCreationInfo = new List<BuffCreationInfo>();

        [XmlIgnore]
        public Move _movementAbility = null;

        public int AbilityVariation = 0;

        public bool NonCombatant = false;

        public float Height = 1;
        //public int VisionRadius => _visionRadius + (!Scene.InCombat && Unit.AI.ControlType == ControlType.Controlled ? OUT_OF_COMBAT_VISION : 0);
        //public int _visionRadius = 6;

        public Direction Facing = Direction.North;

        public bool Dead = false;
        public bool BlocksSpace = true;
        public bool PhasedMovement = false;

        public Species Species = Species.Humanoid;

        public bool PartyMember = false;


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

        public void AddBuff(Buff buff)
        {
            BuffManager.AddBuff(buff);
            Unit?.OnStateChanged();
        }

        public void RemoveBuff(Buff buff)
        {
            BuffManager.RemoveBuff(buff);
            Unit?.OnStateChanged();
        }

        public bool CanUseAbility(Ability ability)
        {
            CastingMethod method = ability.CastingMethod;

            bool canUse = !StatusManager.CheckCondition((StatusCondition)method);

            return canUse;
        }

        public bool CanSwapWeapons()
        {
            return !Context.GetFlag(UnitContext.WeaponSwappedThisTurn);
        }

        public void PrepareForSerialization()
        {
            Stealth.PrepareForSerialization();
            Scouting.PrepareForSerialization();
            Equipment.PrepareForSerialization();
            BuffManager.PrepareForSerialization();
            //StatusManager.PrepareForSerialization();
        }

        public void CompleteDeserialization()
        {
            Stealth.CompleteDeserialization();
            Scouting.CompleteDeserialization();
            Equipment.CompleteDeserialization();
            BuffManager.CompleteDeserialization();
            //StatusManager.CompleteDeserialization();
        }

        public static void AttachUnitToInfo(UnitInfo info, Unit unit)
        {
            info.Unit = unit;
            info.Stealth.Unit = unit;
            info.Scouting.Unit = unit;
            info.Equipment.Unit = unit;
            info.BuffManager.Unit = unit;
            info.StatusManager.Unit = unit;
            info.ResourceManager.Unit = unit;
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
        public List<Action> HidingBrokenActions = new List<Action>();

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

using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Save
{
    [Serializable]
    public class UnitSaveInfo
    {
        [XmlElement("Una")]
        public string Name;

        [XmlElement("Ute")]
        public UnitTeam Team;
        [XmlElement("Uct")]
        public ControlType ControlType;

        [XmlElement("Uf")]
        public bool Fighting;

        [XmlElement("Ume")]
        public float MaxEnergy;
        [XmlElement("Ue")]
        public float Energy;

        [XmlElement("Uma")]
        public float MaxAction;
        [XmlElement("Ua")]
        public float Action;

        [XmlElement("Ufo")]
        public float Focus;
        [XmlElement("Umf")]
        public float MaxFocus;

        [XmlElement("Uh")]
        public float Health;
        [XmlElement("Umh")]
        public float MaxHealth;

        [XmlElement("Ucs")]
        public int CurrentShields;

        [XmlElement("Unc")]
        public bool NonCombatant;

        [XmlElement("Ufa")]
        public Tiles.Direction Facing;

        [XmlElement("Ude")]
        public bool Dead = false;
        [XmlElement("Ubs")]
        public bool BlocksSpace = true;
        [XmlElement("Uph")]
        public bool PhasedMovement = false;

        [XmlElement("Ust")]
        public StatusCondition Status = StatusCondition.None;

        [XmlElement("Utr")]
        public bool Transparent;

        [XmlElement("Up")]
        public FeaturePoint Position;

        [XmlElement("Upn")]
        public string pack_name;
        [XmlElement("Upt")]
        public UnitProfileType ProfileType;
        [XmlElement("Usp")]
        public Species Species;

        [XmlElement("Ufi")]
        public long FeatureID;
        [XmlElement("Ufh")]
        public long FeatureHash;

        [XmlElement("Ual")]
        public AbilityLoadout AbilityLoadout;

        [XmlElement("Ucin")]
        public int UnitCreationInfoId;

        [XmlElement("Uc")]
        public Vector4 Color;

        [XmlElement("Ugs")]
        public int GroupStatus;

        [XmlElement("Upar")]
        public ParameterDict UnitParameters = new ParameterDict();

        public UnitSaveInfo() { }

        public UnitSaveInfo(Unit unit)
        {
            Name = unit.Name;
            Team = unit.AI.Team;
            ControlType = unit.AI.ControlType;

            Fighting = unit.AI.Fighting;

            MaxEnergy = unit.Info.MaxEnergy;
            Energy = unit.Info.Energy;

            MaxAction = unit.Info.MaxActionEnergy;
            Action = unit.Info.ActionEnergy;

            Focus = unit.Info.Focus;
            MaxFocus = unit.Info.MaxFocus;

            Health = unit.Info.Health;
            MaxHealth = unit.Info.MaxHealth;

            CurrentShields = unit.Info.CurrentShields;

            NonCombatant = unit.Info.NonCombatant;

            Facing = unit.Info.Facing;

            Dead = unit.Info.Dead;
            BlocksSpace = unit.Info.BlocksSpace;
            PhasedMovement = unit.Info.PhasedMovement;

            Status = unit.Info.Status;

            Transparent = unit.Info.Transparent;

            if (unit.Info.TileMapPosition != null)
            {
                Position = unit.Info.TileMapPosition.ToFeaturePoint();
            }

            pack_name = unit.pack_name;
            ProfileType = unit.ProfileType;
            Species = unit.Info.Species;

            FeatureHash = unit.ObjectHash;
            FeatureID = unit.FeatureID;

            AbilityLoadout = unit.AbilityLoadout;
            UnitCreationInfoId = unit.UnitCreationInfoId;

            foreach (var ability in unit.Info.Abilities)
            {
                var loadout = AbilityLoadout.Items.Find(a => a.NodeID == ability.NodeID 
                    && a.AbilityTreeType == ability.AbilityTreeType 
                    && (a.BasicAbility > 0) == ability.BasicAbility);

                if(loadout != null)
                {
                    loadout.CurrentCharges = ability.Charges < ability.MaxCharges ? ability.Charges : -1;
                }
            }

            Color = unit.Color;

            if (unit.Scene.UnitGroup != null && unit.Scene.UnitGroup.SecondaryUnitsInGroup.Contains(unit))
            {
                GroupStatus = 2;
            }
            else if (unit.Scene.UnitGroup != null && unit.Scene.UnitGroup.PrimaryUnit == unit)
            {
                GroupStatus = 1;
            }

            UnitParameters = unit.UnitParameters;

            UnitParameters.PrepareForSerialization();
        }

        public void ApplyUnitInfoToUnit(Unit unit)
        {
            unit.Name = Name;
            unit.SetTeam(Team);
            unit.AI.ControlType = ControlType;

            unit.AI.Fighting = Fighting;

            unit.Info.MaxEnergy = MaxEnergy;
            unit.Info.Energy = Energy;

            unit.Info.MaxActionEnergy = MaxAction;
            unit.Info.ActionEnergy = Action;

            unit.Info.Focus = Focus;
            unit.Info.MaxFocus = MaxFocus;

            unit.Info.Health = Health;
            unit.Info.MaxHealth = MaxHealth;

            unit.Info.CurrentShields = CurrentShields;

            unit.Info.NonCombatant = NonCombatant;

            unit.Info.Facing = Facing;

            unit.Info.Dead = Dead;
            unit.Info.BlocksSpace = BlocksSpace;
            unit.Info.PhasedMovement = PhasedMovement;

            unit.Info.Status = Status;

            unit.Info.Transparent = Transparent;

            unit.pack_name = pack_name;
            unit.ProfileType = ProfileType;
            unit.Info.Species = Species;

            unit.ObjectHash = FeatureHash;
            unit.FeatureID = FeatureID;

            unit.AbilityLoadout = AbilityLoadout;
            unit.UnitCreationInfoId = UnitCreationInfoId;


            UnitParameters.CompleteDeserialization();
            unit.UnitParameters = UnitParameters;

            unit.ApplyUnitParameters(UnitParameters.Parameters);
        }

    }
}

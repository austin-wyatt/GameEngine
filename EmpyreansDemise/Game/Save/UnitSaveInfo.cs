using Empyrean.Game.Abilities;
using Empyrean.Game.Entities;
using Empyrean.Game.Map;
using Empyrean.Game.Player;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Save
{
    [Serializable]
    public class UnitSaveInfo : ISerializable
    {
        [XmlElement("Una")]
        public string Name;

        [XmlElement("Unai")]
        public UnitAI UnitAI;

        [XmlElement("Uninf")]
        public UnitInfo UnitInfo;

        [XmlElement("Up")]
        public FeaturePoint Position;

        [XmlElement("Upn")]
        public string pack_name;

        [XmlElement("Upi")]
        public int PermanentId;

        [XmlElement("Ual")]
        public AbilityLoadout AbilityLoadout;

        [XmlElement("Ucin")]
        public int UnitCreationInfoId;

        [XmlElement("Uc")]
        public Vector4 Color;

        [XmlElement("Upar", Namespace = "Usi")]
        public ParameterDict UnitParameters = new ParameterDict();

        public bool Grouped = false;

        public UnitSaveInfo() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="unloadingUnit">
        /// Whether the unit will be unloaded after this snapshot is created <para/>
        /// If they aren't being unloaded, the abilities and items that were removed 
        /// during the save process are readded to the unit
        /// </param>
        public UnitSaveInfo(Unit unit, bool unloadingUnit)
        {
            Name = unit.Name;

            //These need to be deep copies
            UnitAI = unit.AI;
            UnitInfo = unit.Info;

            AbilityLoadout = unit.AbilityLoadout;
            UnitCreationInfoId = unit.UnitCreationInfoId;

            foreach (var ability in unit.Info.Abilities)
            {
                var loadout = AbilityLoadout.GetLoadout(unit.Info.AbilityVariation).Find(a => a.NodeID == ability.NodeID
                    && a.AbilityTreeType == ability.AbilityTreeType);

                if (loadout != null)
                {
                    loadout.CurrentCharges = ability.Charges < ability.MaxCharges ? ability.Charges : -1;
                }

                ability.RemoveAbilityFromUnit(fromLoad: true);
            }

            foreach (var itemEntry in unit.Info.Equipment.EquippedItems)
            {
                itemEntry.Value.OnUnequipped(fromLoad: true);
            }

            UnitInfo.PrepareForSerialization();
            UnitAI.PrepareForSerialization();

            if(PlayerParty.Grouped && PlayerParty.PrimaryUnit != unit && PlayerParty.UnitsInParty.Contains(unit))
            {
                Grouped = true;
            }


            if (unit.Info.TileMapPosition != null)
            {
                Position = unit.Info.TileMapPosition.ToFeaturePoint();
            }
            else
            {
                Position = FeaturePoint.MinPoint;
            }

            pack_name = unit.pack_name;

            PermanentId = unit.PermanentId.Id;

            Color = unit.Color;

            UnitParameters = unit.UnitParameters;

            PrepareForSerialization();

            if (!unloadingUnit)
            {
                foreach (var ability in unit.Info.Abilities)
                {
                    ability.AddAbilityToUnit(fromLoad: true);
                }

                foreach (var itemEntry in unit.Info.Equipment.EquippedItems)
                {
                    itemEntry.Value.OnEquipped(fromLoad: true);
                }
            }
        }

        public void ApplyUnitInfoToUnit(Unit unit)
        {
            UnitInfo.AttachUnitToInfo(UnitInfo, unit);
            UnitAI.AttachUnitToAI(UnitAI, unit);

            unit.Info = UnitInfo;
            unit.AI = UnitAI;

            CompleteDeserialization();

            unit.Name = Name;
            unit.SetTeam(UnitAI.Team);
            
            unit.pack_name = pack_name;

            unit.SetPermanentId(PermanentId);

            unit.AbilityLoadout = AbilityLoadout;
            unit.UnitCreationInfoId = UnitCreationInfoId;
            
            unit.UnitParameters = UnitParameters;

            unit.ApplyUnitParameters(UnitParameters.Parameters);
        }

        public void CompleteDeserialization()
        {
            UnitAI.CompleteDeserialization();
            UnitInfo.CompleteDeserialization();
            UnitParameters.CompleteDeserialization();
        }

        public void PrepareForSerialization()
        {
            UnitParameters.PrepareForSerialization();
        }
    }
}

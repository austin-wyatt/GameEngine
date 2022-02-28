using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Player;
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

        [XmlElement("Upar", Namespace = "Usi")]
        public ParameterDict UnitParameters = new ParameterDict();

        public bool Grouped = false;

        public UnitSaveInfo() { }

        public UnitSaveInfo(Unit unit)
        {
            Name = unit.Name;

            UnitAI = unit.AI;

            UnitInfo = unit.Info;

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

            FeatureHash = unit.ObjectHash;
            FeatureID = unit.FeatureID;

            AbilityLoadout = unit.AbilityLoadout;
            UnitCreationInfoId = unit.UnitCreationInfoId;

            foreach (var ability in unit.Info.Abilities)
            {
                var loadout = AbilityLoadout.GetLoadout(unit.Info.AbilityVariation).Find(a => a.NodeID == ability.NodeID 
                    && a.AbilityTreeType == ability.AbilityTreeType);

                if(loadout != null)
                {
                    loadout.CurrentCharges = ability.Charges < ability.MaxCharges ? ability.Charges : -1;
                }
            }

            Color = unit.Color;

            UnitParameters = unit.UnitParameters;

            PrepareForSerialization();
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

            unit.ObjectHash = FeatureHash;
            unit.FeatureID = FeatureID;

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

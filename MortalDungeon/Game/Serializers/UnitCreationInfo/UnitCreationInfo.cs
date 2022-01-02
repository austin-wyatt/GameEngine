using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [Serializable]
    public class UnitCreationInfo
    {
        #region very important to set
        [XmlElement("UdN")]
        public string DescriptiveName; //this is a dev name that will be shown in the engine tools to make it easier to track things with similar names

        [XmlElement("UdID")]
        public int Id; //this will seed the file names


        [XmlElement("Ual")]
        public int AbilityLoadoutId;
        [XmlElement("Uans")]
        public string AnimationSetName;
        #endregion

        #region pretty important to set
        [XmlElement("Una")]
        public string Name;

        [XmlElement("Ute")]
        public UnitTeam Team;
        [XmlElement("Uct")]
        public ControlType ControlType;
        #endregion 

        #region decently important to set
        [XmlElement("Ume")]
        public float MaxEnergy = 10;

        [XmlElement("Uma")]
        public float MaxAction = 3;

        [XmlElement("Umf")]
        public float MaxFocus = 0;

        [XmlElement("Umh")]
        public float MaxHealth = 100;

        [XmlElement("Ucs")]
        public int CurrentShields = 0;

        [XmlElement("Usp")]
        public Species Species;
        #endregion

        #region probably not very important to set
        [XmlElement("Unc")]
        public int NonCombatant = 0;

        [XmlElement("Ubs")]
        public int BlocksSpace = 1;
        [XmlElement("Uph")]
        public int PhasedMovement = 0;

        [XmlElement("Ust")]
        public StatusCondition Status = StatusCondition.None;
        #endregion

        #region extra info not important to set unless the unit specifically needs it
        [XmlElement("Uc")]
        public string Color = "1, 1, 1, 1";

        [XmlElement("Ucsb")]
        public int CreateStatusBar = 1;
        [XmlElement("Uxr")]
        public int XRotation = 25;
        [XmlElement("UtO")]
        public string TileOffset = "0, -213.8889f, 0.2f";
        [XmlElement("UstO")]
        public string SelectionTileOffset = "0, 213.8889f, -0.19f";
        #endregion

        public UnitCreationInfo() { }

        public Unit CreateUnit(CombatScene scene) 
        {
            Unit unit = new Unit(scene);

            unit.UnitCreationInfoId = Id;

            //create an ability and animation manager that manages loading and unloading these files so that units aren't directly reading from files
            unit.AbilityLoadout = AbilityLoadoutSerializer.LoadAbilityLoadoutFromFile(AbilityLoadoutId); //Check to see if this has already been loaded
            unit.AnimationSet = AnimationSerializer.LoadAnimationFromFileWithName(AnimationSetName); //Check to see if this has already been loaded

            unit.Name = Name;

            unit.SetTeam(Team);
            unit.AI.ControlType = ControlType;

            unit.Info.MaxEnergy = MaxEnergy;
            unit.Info.Energy = MaxEnergy;
            unit.Info.MaxActionEnergy = MaxAction;
            unit.Info.ActionEnergy = MaxAction;
            unit.Info.MaxFocus = MaxFocus;
            unit.Info.Focus = MaxFocus;
            unit.Info.CurrentShields = CurrentShields;

            unit.Info.Species = Species;

            unit.Info.NonCombatant = NonCombatant > 0;
            unit.Info.BlocksSpace = BlocksSpace > 0;
            unit.Info.PhasedMovement = PhasedMovement > 0;

            unit.Info.Status = Status;

            Color = Color.Replace(" ", "").Replace("f", "");
            string[] colorVals = Color.Split(",");
            if(colorVals.Length == 4)
            {
                try
                {
                    unit.Color = new Vector4(float.Parse(colorVals[0]), float.Parse(colorVals[1]), 
                        float.Parse(colorVals[2]), float.Parse(colorVals[3]));
                }
                catch(Exception ex) { }
            }

            unit._createStatusBar = CreateStatusBar > 0;

            unit._xRotation = XRotation;

            TileOffset = TileOffset.Replace(" ", "").Replace("f", "");
            string[] tileOffsetVals = TileOffset.Split(",");
            if (tileOffsetVals.Length == 3)
            {
                try
                {
                    unit.TileOffset = new Vector3(float.Parse(tileOffsetVals[0]), float.Parse(tileOffsetVals[1]), float.Parse(tileOffsetVals[2]));
                }
                catch (Exception ex) { }
            }

            SelectionTileOffset = SelectionTileOffset.Replace(" ", "").Replace("f", "");
            string[] selectionTileOffsetVals = SelectionTileOffset.Split(",");
            if (selectionTileOffsetVals.Length == 3)
            {
                try
                {
                    unit.SelectionTileOffset = new Vector3(float.Parse(selectionTileOffsetVals[0]), float.Parse(selectionTileOffsetVals[1]), float.Parse(selectionTileOffsetVals[2]));
                }
                catch (Exception ex) { }
            }

            return unit;
        }
    }
}

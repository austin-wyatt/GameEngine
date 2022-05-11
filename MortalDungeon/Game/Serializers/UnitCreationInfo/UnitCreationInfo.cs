using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Events;
using Empyrean.Game.Items;
using Empyrean.Game.Serializers.Abilities;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    [Serializable]
    public class UnitCreationInfo : ISerializable
    {
        #region very important to set
        [XmlElement("UdN")]
        public string DescriptiveName; //this is a dev name that will be shown in the engine tools to make it easier to track things with similar names

        [XmlElement("UdID")]
        public int Id; //this will seed the file names


        [XmlElement("Uans")]
        public int AnimationSetId;

        [XmlElement("UaEV")]
        public List<EventActionBuilder> EventActionBuilders = new List<EventActionBuilder>();

        [XmlElement("UaDi")]
        public List<int> UnitDialogueIDs = new List<int>();

        /// <summary>
        /// Maybe a new object dedicated to adding items to units should be created to simplify things. <para/>
        /// (ie all it needs is Id, modifier, stack size, charges, and whether to equip it and in what slot)
        /// </summary>
        [XmlElement("UaIt")]
        public List<UnitItemEntry> UnitItems = new List<UnitItemEntry>();

        [XmlElement("UaGo")]
        public long Gold = 0;

        [XmlElement("UAl")]
        public AbilityLoadout AbilityLoadout = new AbilityLoadout();

        [XmlElement("URM")]
        public ResourceManager ResourceManager = new ResourceManager();

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
        public string TileOffset = "0f, 0f, 0f";
        [XmlElement("UstO")]
        public string SelectionTileOffset = "0f, 0f, 0f";

        [XmlElement("UciSc")]
        public float Scale = 1;
        #endregion

        public UnitCreationInfo() 
        {
            
        }

        public Unit CreateUnit(CombatScene scene) 
        {
            Unit unit = new Unit(scene);

            unit.UnitCreationInfoId = Id;

            unit.AbilityLoadout = new AbilityLoadout(AbilityLoadout);
            unit.AnimationSet = AnimationSetManager.GetAnimationSet(AnimationSetId); //Check to see if this has already been loaded

            foreach(var entry in UnitItems)
            {
                var item = entry.ItemEntry.GetItemFromEntry();

                if(entry.Location == ItemLocation.Equipment)
                {
                    unit.Info.Equipment.EquipItem(item, entry.EquipmentSlot);
                }
                else
                {
                    unit.Info.Inventory.AddItemToInventory(item);
                }
            }

            unit.Info.Inventory.Gold = Gold;

            unit.Name = Name;

            unit.SetTeam(Team);
            unit.AI.ControlType = ControlType;

            unit.Info.ResourceManager = new ResourceManager(ResourceManager);
            unit.Info.ResourceManager.Unit = unit;

            unit.Info.Species = Species;

            unit.Info.NonCombatant = NonCombatant > 0;
            unit.Info.BlocksSpace = BlocksSpace > 0;
            unit.Info.PhasedMovement = PhasedMovement > 0;

            Color = Color.Replace(" ", "").Replace("f", "");
            string[] colorVals = Color.Split(",");
            if(colorVals.Length == 4)
            {
                try
                {
                    unit.Color = new Vector4(float.Parse(colorVals[0]), float.Parse(colorVals[1]), 
                        float.Parse(colorVals[2]), float.Parse(colorVals[3]));
                }
                catch { }
            }

            unit._createStatusBar = CreateStatusBar > 0;

            unit._xRotation = XRotation;
            unit._scale = Scale;


            TileOffset = TileOffset.Replace(" ", "").Replace("f", "");
            string[] tileOffsetVals = TileOffset.Split(",");
            if (tileOffsetVals.Length == 3)
            {
                try
                {
                    unit.TileOffset = new Vector3(float.Parse(tileOffsetVals[0]), float.Parse(tileOffsetVals[1]), float.Parse(tileOffsetVals[2]));
                }
                catch { }
            }

            SelectionTileOffset = SelectionTileOffset.Replace(" ", "").Replace("f", "");
            string[] selectionTileOffsetVals = SelectionTileOffset.Split(",");
            if (selectionTileOffsetVals.Length == 3)
            {
                try
                {
                    unit.SelectionTileOffset = new Vector3(float.Parse(selectionTileOffsetVals[0]), float.Parse(selectionTileOffsetVals[1]), float.Parse(selectionTileOffsetVals[2]));
                }
                catch { }
            }

            foreach(var eventActionBuilder in EventActionBuilders)
            {
                var action = eventActionBuilder.BuildAction();
                if(unit.EventActions.TryGetValue(action.EventTrigger, out var list))
                {
                    list.Add(action);
                }
                else
                {
                    unit.EventActions.Add(action.EventTrigger, new List<EventAction> { action });
                }
            }

            return unit;
        }

        public void CompleteDeserialization()
        {
            //foreach (var item in AbilityCreationList)
            //{
            //    item.CompleteDeserialization();
            //}
            ResourceManager.CompleteDeserialization();

            foreach(var item in EventActionBuilders)
            {
                item.CompleteDeserialization();
            }
        }

        public void PrepareForSerialization()
        {
            //foreach (var item in AbilityCreationList)
            //{
            //    item.PrepareForSerialization();
            //}
            ResourceManager.PrepareForSerialization();

            foreach (var item in EventActionBuilders)
            {
                item.PrepareForSerialization();
            }
        }
    }
}

﻿using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Abilities;
using Empyrean.Game.Entities;
using Empyrean.Game.Ledger;
using Empyrean.Game.Map;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.Player;
using System.Linq;
using Empyrean.Game.Items;
using Empyrean.Game.Abilities.TileEffects;
using Empyrean.Game.Ledger.Units;
using System.Numerics;
using Vector3 = OpenTK.Mathematics.Vector3;
using DataObjects;

namespace Empyrean.Game.Save
{
    [Serializable]
    public class SaveState
    {
        public SaveState() { }

        public List<UnitSaveInfo> UnitSaveInfo = new List<UnitSaveInfo>();

        public int Time;
        public int Days;

        public Vector2i TileMapCoords;

        [XmlElement(Namespace = "relations")]
        public Dictionary<long, Relation> UnitRelations;

        public List<QuestSaveInfo> QuestSaveInfo;
        public List<DialogueSaveInfo> DialogueSaveInfo;

        //these will actually evaluate a bool based on current state values instead of trying to exactly match a passed value
        public List<Instructions> SubscribedInstructions = new List<Instructions>();

        public List<UnitSaveInfo> PlayerPartySaveInfo = new List<UnitSaveInfo>();
        public bool PartyGrouped = false;
        public Inventory PartyInventory;

        public TileEffectsSaveInfo TileEffectsSaveInfo;

        public List<LedgeredUnit> SavedLedgeredUnits;

        public GlobalInfo OverwrittenGlobalInfo;

        public HashSet<PermanentUnitInfo> PermanentUnitInfo = new HashSet<PermanentUnitInfo>();

        public static SaveState CreateSaveState(CombatScene scene) 
        {
            //TODO: QUESTS


            SaveState returnState = new SaveState();

            returnState.OverwrittenGlobalInfo = GlobalInfo.PlayerInfo;
            returnState.OverwrittenGlobalInfo.PrepareForSerialization();

            #region Loaded units
            HashSet<Unit> playerPartySet = PlayerParty.UnitsInParty.ToHashSet();
            foreach(var unit in scene._units)
            {
                if (!playerPartySet.Contains(unit))
                {
                    returnState.UnitSaveInfo.Add(new UnitSaveInfo(unit, unloadingUnit: false));
                }
            }
            #endregion

            #region Ledgered units
            returnState.SavedLedgeredUnits = UnitPositionLedger.LedgeredUnits.ToList();
            #endregion

            returnState.PermanentUnitInfo = PermanentUnitInfoLedger.UnitInfo;

            returnState.Time = scene.Time;
            returnState.Days = CombatScene.Days;

            returnState.TileMapCoords = new Vector2i(TileMapManager.LoadedCenter.X, TileMapManager.LoadedCenter.Y);

            returnState.UnitRelations = new Dictionary<long,Relation>(UnitAI.GetTeamRelationsDictionary());


            DataSourceManager.SaveSources();


            #region Player party
            returnState.PlayerPartySaveInfo = new List<UnitSaveInfo>();
            foreach (var unit in PlayerParty.UnitsInParty)
            {
                returnState.PlayerPartySaveInfo.Add(new UnitSaveInfo(unit, unloadingUnit: false));
            }
            returnState.PartyGrouped = PlayerParty.Grouped;
            returnState.PartyInventory = PlayerParty.Inventory;

            returnState.PartyInventory.PrepareForSerialization();
            #endregion

            returnState.TileEffectsSaveInfo = new TileEffectsSaveInfo();
            returnState.TileEffectsSaveInfo.PrepareForSerialization();

            return returnState;
        }

        public static void LoadSaveState(CombatScene scene, SaveState state)
        {
            scene.ContextManager.SetFlag(GeneralContextFlags.SaveStateLoadInProgress, true);
            scene.ContextManager.SetFlag(GeneralContextFlags.DisableVisionMapUpdate, true);

            scene.UnitVisionGenerators.Clear();

            state.OverwrittenGlobalInfo.CompleteDeserialization();
            GlobalInfo.PlayerInfo = state.OverwrittenGlobalInfo;

            DataSourceManager.LoadSources();

            

            for (int i = EntityManager.Entities.Count - 1; i >= 0; i--)
            {
                EntityManager.RemoveEntity(EntityManager.Entities.First());
            }

            #region Map load
            TileMapManager.SetCenter(new TileMapPoint(state.TileMapCoords));
            TileMapManager.LoadMapsAroundCenter();
            #endregion


            #region Player party
            PlayerParty.UnitsInParty.Clear();

            PlayerParty.Grouped = state.PartyGrouped;

            foreach (var item in state.PlayerPartySaveInfo)
            {
                UnitCreationInfo info = UnitInfoBlockManager.GetUnit(item.UnitCreationInfoId);

                if (info == null)
                    continue;

                Unit unit = info.CreateUnit(scene, firstLoad: false);

                item.ApplyUnitInfoToUnit(unit);

                Entity entity = new Entity(unit);
                EntityManager.AddEntity(entity);

                if (!item.Grouped)
                {
                    EntityManager.LoadEntity(entity, item.Position);

                    if (PlayerParty.Grouped)
                    {
                        PlayerParty.PrimaryUnit = unit;
                    }
                }

                unit.SetColor(item.Color);

                if (unit.GetResF(ResF.Health) <= 0)
                {
                    unit.Kill();
                }

                PlayerParty.UnitsInParty.Add(unit);
            }

            state.PartyInventory.CompleteDeserialization();
            PlayerParty.Inventory = state.PartyInventory;
            #endregion

            //scene._tileMapController.LoadSurroundingTileMaps(new TileMapPoint(state.TileMapCoords), applyFeatures: false, forceMapRegeneration: true);

            #region Loaded units
            UnitAI.SetTeamRelationsDictionary(state.UnitRelations);

            foreach(var unitInfo in state.UnitSaveInfo)
            {
                UnitCreationInfo info = UnitInfoBlockManager.GetUnit(unitInfo.UnitCreationInfoId);

                if (info == null)
                    continue;

                Unit unit = info.CreateUnit(scene, firstLoad: false);

                unitInfo.ApplyUnitInfoToUnit(unit);

                Entity entity = new Entity(unit);
                EntityManager.AddEntity(entity);

                EntityManager.LoadEntity(entity, unitInfo.Position);

                unit.SetColor(unitInfo.Color);

                if(unit.GetResF(ResF.Health) <= 0)
                {
                    unit.Kill();
                }
            }
            #endregion

            #region Ledgered units
            UnitPositionLedger.LedgeredUnits = state.SavedLedgeredUnits.ToHashSet();
            UnitPositionLedger.BuildTileMapPositionDictionary();
            #endregion

            PermanentUnitInfoLedger.UnitInfo = state.PermanentUnitInfo;

            void finishLoad(SceneEventArgs _)
            {
                scene.RenderEnd -= finishLoad;

                scene.ContextManager.SetFlag(GeneralContextFlags.SaveStateLoadInProgress, false);
                scene.ContextManager.SetFlag(GeneralContextFlags.DisableVisionMapUpdate, false);

                TileMapManager.ApplyLoadedFeaturesToMaps(TileMapManager.ActiveMaps);

                scene.SetTime(state.Time);
                scene.SetDay(state.Days);

                Unit unit = scene._units.Find(u => u.AI.Team == UnitTeam.PlayerUnits);
                if(unit != null) 
                {
                    scene.SmoothPanCameraToUnit(unit, 1);
                    scene.CurrentUnit = unit;
                }
                else
                {
                    scene.SmoothPanCamera(new Vector3(0, 0, scene._camera.Position.Z), 1);
                }

                scene.EndCombat();

                state.TileEffectsSaveInfo.CompleteDeserialization();
            }

            scene.RenderEnd += finishLoad;
        }

        public static SaveState LoadSaveStateFromFile(string path) 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SaveState));

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);

            TextReader reader = new StreamReader(fs);


            SaveState loadedState = (SaveState)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static void WriteSaveStateToFile(string path, SaveState state)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SaveState));

            XmlWriterSettings settings = new XmlWriterSettings() 
            {
                Indent = false,
                NewLineHandling = NewLineHandling.None
            };

            XmlWriter writer = XmlWriter.Create(path, settings);


            serializer.Serialize(writer, state);

            writer.Close();
        }
    }

    

    [XmlType(TypeName = "DD")]
    [Serializable]
    public class DeserializableDictionary<T, Y>
    {
        [XmlElement("Dk")]
        public List<T> Keys = new List<T>();
        [XmlElement("Dv")]
        public List<Y> Values = new List<Y>();


        public DeserializableDictionary() { }

        public DeserializableDictionary(Dictionary<T, Y> dict)
        {
            foreach(var kvp in dict) 
            {
                Keys.Add(kvp.Key);
                Values.Add(kvp.Value);
            }
        }

        public DeserializableDictionary(DeserializableDictionary<T, Y> dict)
        {
            for(int i = 0; i < dict.Keys.Count; i++)
            {
                Keys.Add(dict.Keys[i]);
            }

            for (int i = 0; i < dict.Values.Count; i++)
            {
                Values.Add(dict.Values[i]);
            }
        }

        public void FillDictionary(Dictionary<T, Y> dict)
        {
            for(int i = 0; i < Keys.Count; i++)
            {
                if(!dict.TryAdd(Keys[i], Values[i]))
                {
                    dict[Keys[i]] = Values[i];
                }
            }
        }

        public void Clear()
        {
            Keys.Clear();
            Values.Clear();
        }
    }

    /// <summary>
    /// A copy of the DeserializableDictionary class but with a different XML namespace to allow nesting
    /// a DeserializableDictionary_ inside of a DeserializableDictionary
    /// </summary>
    [XmlType(TypeName = "DD_", Namespace = "dd_")]
    public class DeserializableDictionary_<T, Y> : DeserializableDictionary<T, Y> 
    {
        public DeserializableDictionary_() { }
        public DeserializableDictionary_(Dictionary<T, Y> dict)
        {
            foreach (var kvp in dict)
            {
                Keys.Add(kvp.Key);
                Values.Add(kvp.Value);
            }
        }
    }


    [XmlType(TypeName = "DHs")]
    [Serializable]
    public class DeserializableHashset<T>
    {
        [XmlElement("Dhk")]
        public List<T> Keys = new List<T>();

        public DeserializableHashset() { }

        public DeserializableHashset(HashSet<T> set)
        {
            foreach (var value in set)
            {
                Keys.Add(value);
            }
        }

        public void FillHashSet(HashSet<T> set)
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                set.Add(Keys[i]);
            }
        }

    }
}

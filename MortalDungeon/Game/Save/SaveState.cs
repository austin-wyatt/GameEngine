using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Save
{
    [Serializable]
    public class SaveState
    {
        public SaveState() { }

        public List<UnitSaveInfo> UnitSaveInfo = new List<UnitSaveInfo>();

        public int Time;

        public Vector2i TileMapCoords;

        [XmlElement(Namespace = "relations")]
        public DeserializableDictionary<long, Relation> UnitRelations;

        public List<FeatureSaveInfo> FeatureSaveInfo;
        public List<QuestSaveInfo> QuestSaveInfo;
        public List<DialogueSaveInfo> DialogueSaveInfo;



        public static SaveState CreateSaveState(CombatScene scene) 
        {
            SaveState returnState = new SaveState();

            foreach(var unit in scene._units)
            {
                returnState.UnitSaveInfo.Add(new UnitSaveInfo(unit));
            }

            if(scene.UnitGroup != null)
            {
                foreach (var unit in scene.UnitGroup.SecondaryUnitsInGroup)
                {
                    returnState.UnitSaveInfo.Add(new UnitSaveInfo(unit));
                }
            }

            returnState.Time = scene.Time;

            returnState.TileMapCoords = scene._tileMapController.GetCenterMapCoords();

            returnState.UnitRelations = new DeserializableDictionary<long, Relation>(UnitAI.GetTeamRelationsDictionary());

            returnState.FeatureSaveInfo = new List<FeatureSaveInfo>();
            foreach(var f in FeatureLedger.LedgeredFeatures)
            {
                FeatureSaveInfo info = new FeatureSaveInfo()
                {
                    ID = f.Value.ID,
                    SignificantInteractions = new DeserializableDictionary<long, short>(f.Value.SignificantInteractions)
                };

                returnState.FeatureSaveInfo.Add(info);
            }

            returnState.QuestSaveInfo = new List<QuestSaveInfo>();
            foreach (var q in QuestLedger.LedgeredQuests)
            {
                QuestSaveInfo info = new QuestSaveInfo()
                {
                    ID = q.Value.ID,
                    QuestState = new DeserializableHashset<int>(q.Value.QuestState)
                };

                returnState.QuestSaveInfo.Add(info);
            }

            returnState.DialogueSaveInfo = new List<DialogueSaveInfo>();
            foreach (var d in DialogueLedger.LedgeredDialogues)
            {
                DialogueSaveInfo info = new DialogueSaveInfo()
                {
                    ID = d.Value.ID,
                    RecievedOutcomes = new DeserializableHashset<int>(d.Value.RecievedOutcomes)
                };

                returnState.DialogueSaveInfo.Add(info);
            }


            return returnState;
        }

        public static void LoadSaveState(CombatScene scene, SaveState state)
        {
            scene.ContextManager.SetFlag(GeneralContextFlags.SaveStateLoadInProgress, true);
            scene.ContextManager.SetFlag(GeneralContextFlags.DisableVisionMapUpdate, true);

            scene.UnitVisionGenerators.Clear();
            scene.LightObstructions.Clear();

            scene.UnitGroup = new UnitGroup(scene); //TEMP

            for (int i = EntityManager.Entities.Count - 1; i >= 0; i--)
            {
                EntityManager.RemoveEntity(EntityManager.Entities[i]);
            }

            foreach(var info in state.FeatureSaveInfo)
            {
                FeatureLedgerNode node;

                if(FeatureLedger.LedgeredFeatures.TryGetValue(info.ID, out var n))
                {
                    node = n;
                }
                else
                {
                    node = new FeatureLedgerNode();
                    FeatureLedger.LedgeredFeatures.Add(info.ID, node);
                }

                node.ID = info.ID;
                info.SignificantInteractions.FillDictionary(node.SignificantInteractions);
            }

            foreach (var info in state.DialogueSaveInfo)
            {
                DialogueLedgerNode node;

                if (DialogueLedger.LedgeredDialogues.TryGetValue(info.ID, out var n))
                {
                    node = n;
                }
                else
                {
                    node = new DialogueLedgerNode();
                    DialogueLedger.LedgeredDialogues.Add(info.ID, node);
                }

                node.ID = info.ID;
                info.RecievedOutcomes.FillHashSet(node.RecievedOutcomes);
            }

            foreach (var info in state.QuestSaveInfo)
            {
                QuestLedgerNode node;

                if (QuestLedger.LedgeredQuests.TryGetValue(info.ID, out var n))
                {
                    node = n;
                }
                else
                {
                    node = new QuestLedgerNode();
                    QuestLedger.LedgeredQuests.Add(info.ID, node);
                }

                node.ID = info.ID;
                info.QuestState.FillHashSet(node.QuestState);
            }


            scene._tileMapController.LoadSurroundingTileMaps(new Tiles.TileMapPoint(state.TileMapCoords), applyFeatures: false, forceMapRegeneration: true);

            state.UnitRelations.FillDictionary(UnitAI.GetTeamRelationsDictionary());

            foreach(var unitInfo in state.UnitSaveInfo)
            {
                UnitProfile profile = UnitProfiles.Profiles.Find(p => p.Type == unitInfo.ProfileType);

                if (profile == null)
                    continue;

                Unit unit = profile.CreateUnit(scene);

                unitInfo.ApplyUnitInfoToUnit(unit);

                Entity entity = new Entity(unit);
                EntityManager.AddEntity(entity);

                entity.Load(unitInfo.Position, unitInfo.GroupStatus != 2);

                unit.SetColor(unitInfo.Color);

                if(unit.Info.Health <= 0)
                {
                    unit.Kill();
                }

                if (unitInfo.GroupStatus == 1)
                {
                    scene.UnitGroup.SetPrimaryUnit(unit);
                }
                else if (unitInfo.GroupStatus == 2)
                {
                    scene.UnitGroup.AddUnitToGroup(unit);
                }
            }

            void finishLoad(SceneEventArgs _)
            {
                scene.ContextManager.SetFlag(GeneralContextFlags.SaveStateLoadInProgress, false);
                scene.ContextManager.SetFlag(GeneralContextFlags.DisableVisionMapUpdate, false);

                scene._tileMapController.ApplyLoadedFeaturesToMaps(scene._tileMapController.TileMaps);

                scene.QueueLightObstructionUpdate();
                scene.UpdateVisionMap();

                scene.SetTime(state.Time);

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


                scene.OnRenderEvent -= finishLoad;
            }

            scene.OnRenderEvent += finishLoad;
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

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

        public DeserializableDictionary<long, Relation> UnitRelations;

        public DeserializableDictionary<int, FeatureStateData> FeatureStates;


        public static SaveState CreateSaveState(CombatScene scene) 
        {
            SaveState returnState = new SaveState();

            foreach(var unit in scene._units)
            {
                returnState.UnitSaveInfo.Add(new UnitSaveInfo(unit));
            }

            returnState.Time = scene.Time;

            returnState.TileMapCoords = scene._tileMapController.GetCenterMapCoords();

            returnState.UnitRelations = new DeserializableDictionary<long, Relation>(UnitAI.GetTeamRelationsDictionary());

            returnState.FeatureStates = new DeserializableDictionary<int, FeatureStateData>(FeatureState.States);

            return returnState;
        }

        public static void LoadSaveState(CombatScene scene, SaveState state)
        {
            scene.ContextManager.SetFlag(GeneralContextFlags.SaveStateLoadInProgress, true);
            scene.ContextManager.SetFlag(GeneralContextFlags.DisableVisionMapUpdate, true);

            scene.UnitVisionGenerators.Clear();
            scene.LightObstructions.Clear();

            for (int i = EntityManager.Entities.Count - 1; i >= 0; i--)
            {
                EntityManager.RemoveEntity(EntityManager.Entities[i]);
            }

            state.FeatureStates.FillDictionary(FeatureState.States);

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

                entity.Load(unitInfo.Position);

                unit.SetColor(unitInfo.Color);

                if(unit.Info.Health <= 0)
                {
                    unit.Kill();
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

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }
    }

    [Serializable]
    public class UnitSaveInfo
    {
        public string Name;

        public UnitTeam Team;
        public ControlType ControlType;

        public bool Fighting;

        public float MaxEnergy;
        public float Energy;

        public float MaxAction;
        public float Action;

        public float Health;
        public float MaxHealth;

        public int CurrentShields;

        public bool NonCombatant;

        public Tiles.Direction Facing;

        public bool Dead = false;
        public bool BlocksSpace = true;
        public bool PhasedMovement = false;

        public StatusCondition Status = StatusCondition.None;

        public bool Transparent;

        public FeaturePoint Position;

        public string pack_name;
        public UnitProfileType ProfileType;

        public int FeatureID;
        public long FeatureHash;

        public AbilityLoadout AbilityLoadout;

        public Vector4 Color;

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

            Position = unit.Info.TileMapPosition.ToFeaturePoint();

            pack_name = unit.pack_name;
            ProfileType = unit.ProfileType;

            FeatureHash = unit.FeatureHash;
            FeatureID = unit.FeatureID;

            AbilityLoadout = unit.AbilityLoadout;

            Color = unit.BaseObject.BaseFrame.BaseColor;
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

            unit.FeatureHash = FeatureHash;
            unit.FeatureID = FeatureID;

            unit.AbilityLoadout = AbilityLoadout;
        }
        
    }

    [Serializable]
    public class DeserializableDictionary<T, Y>
    {
        public List<T> Keys = new List<T>();
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
}

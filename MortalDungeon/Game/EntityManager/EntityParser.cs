using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Units;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MortalDungeon.Game.Entities
{
    internal enum PrefabType 
    {
        Unit, 
        Disposition,
        Ability,
        Buff,
        Unknown
    }
    internal class Prefab 
    {
        internal PrefabType Type;
        internal string Name = "";
        internal string File = "";
        internal bool HasProfile = false;
    }
    internal static class EntityParser
    {
        internal static List<Prefab> Prefabs = new List<Prefab>();

        static EntityParser() 
        {
            GatherPrefabs();
        }

        internal static void GatherPrefabs() 
        {
            string[] fileList = Directory.GetFiles(@"Resources\Prefabs\");

            Prefabs.Clear();

            foreach (string item in fileList) 
            {
                string info = File.ReadAllText(item);

                JsonTextReader reader = new JsonTextReader(new StringReader(info));

                List<Dictionary<string, object>> readObjects = GetPrefabObjects(reader);

                foreach (var readObject in readObjects) 
                {
                    if (readObject.TryGetValue("TYPE", out var value)) 
                    {
                        Prefab objPrefab = new Prefab();

                        objPrefab.Type = StringToPrefabType((string)value);

                        if (readObject.TryGetValue("Name", out var name))
                        {
                            objPrefab.Name = (string)name;
                        }
                        else
                        {
                            objPrefab.Name = "Name not found";
                        }

                        objPrefab.File = item;

                        if (readObject.TryGetValue("UnitProfile", out var profileName)) 
                        {
                            objPrefab.HasProfile = true;
                        }

                        Prefabs.Add(objPrefab);
                    }
                }
            }
        }

        private static List<Dictionary<string, object>> GetPrefabObjects(JsonTextReader reader) 
        {
            List<Dictionary<string, object>> foundObjs = new List<Dictionary<string, object>>();

            Dictionary<string, object> currObj = new Dictionary<string, object>();

            while (reader.Read()) 
            {
                //if (reader.Value != null)
                //{
                //    Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                //}
                //else
                //{
                //    Console.WriteLine("Token: {0}", reader.TokenType);
                //}

                if (reader.TokenType == JsonToken.EndObject) 
                {
                    if (currObj != null) 
                    {
                        foundObjs.Add(currObj);
                    }

                    currObj = null;
                }
                else if (reader.TokenType == JsonToken.PropertyName) 
                {
                    //add consideration for nested objects

                    string propertyName = (string)reader.Value;
                    reader.Read();

                    //Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);

                    object value = reader.Value;

                    if (reader.Value == null && reader.TokenType == JsonToken.StartObject)
                    {
                        currObj = new Dictionary<string, object>();
                        currObj.Add("Name", propertyName);
                    }
                    else 
                    {
                        currObj.Add(propertyName, value);
                    }
                }
            }

            return foundObjs;
        }

        internal static Prefab FindPrefab(PrefabType type, string name) 
        {
            return Prefabs.Find(p => p.Type == type && p.Name == name);
        }

        private static PrefabType StringToPrefabType(string val) 
        {
            switch (val) 
            {
                case "UNIT":
                    return PrefabType.Unit;
                case "DISPOSITION":
                    return PrefabType.Disposition;
                case "ABILITY":
                    return PrefabType.Ability;
                case "BUFF":
                    return PrefabType.Buff;
                default:
                    return PrefabType.Unknown;
            }
        }

        private static Dictionary<string, object> GetPrefabObject(Prefab prefab) 
        {
            string info = File.ReadAllText(prefab.File);

            JsonTextReader reader = new JsonTextReader(new StringReader(info));
            var prefabObjs = GetPrefabObjects(reader);

            Dictionary<string, object> prefabToApply = null;

            foreach (var obj in prefabObjs)
            {
                if (obj.TryGetValue("TYPE", out object val) && StringToPrefabType((string)val) == prefab.Type)
                {
                    if (obj.TryGetValue("Name", out object name) && (string)name == prefab.Name)
                    {
                        prefabToApply = obj;
                        break;
                    }
                }
            }

            prefabObjs.Clear();

            return prefabToApply;
        }


        /// <summary>
        /// 
        /// </summary>
        internal static Unit ApplyPrefabToUnit(Prefab prefab, CombatScene scene, Unit unit = null) 
        {
            Dictionary<string, object> prefabToApply = GetPrefabObject(prefab);

            Unit returnUnit = null;


            if (prefabToApply == null)
                return null;

            if (prefabToApply.TryGetValue("UnitProfile", out object val) && (unit == null))
            {
                foreach (var profile in UnitProfiles.Profiles)
                {
                    if (profile.Name == (string)val)
                    {
                        returnUnit = profile.CreateUnit(scene);
                        break;
                    }
                }
            }
            else if (unit != null)
            {
                returnUnit = unit;
            }
            else 
            {
                return null;
            }


            ParseDict(prefabToApply, returnUnit);

            return returnUnit;
        }

        internal static void ParseDict(Dictionary<string, object> dict, Unit unit) 
        {
            foreach (string key in dict.Keys)
            {
                object objVal = dict[key];

                switch (key)
                {
                    case "UnitName":
                        unit.Name = (string)objVal;
                        break;
                    case "UnitTeam":
                        unit.SetTeam((UnitTeam)Convert.ToInt32(objVal));
                        break;
                    case "ControlType":
                        unit.AI.ControlType = (ControlType)Convert.ToInt32(objVal);
                        break;
                    case "MaxEnergy":
                        unit.Info.MaxEnergy = Convert.ToInt32(objVal);
                        break;
                    case "Health":
                        unit.Info.Health = Convert.ToSingle(objVal);
                        break;
                    case "MaxHealth":
                        unit.Info.MaxHealth = Convert.ToSingle(objVal);
                        break;
                    case "CurrentShields":
                        unit.Info.CurrentShields = Convert.ToInt32(objVal);
                        break;
                    case "Facing":
                        unit.Info.Facing = (Tiles.Direction)Convert.ToInt32(objVal);
                        break;
                    case "StealthSkill":
                        unit.Info.Stealth.Skill = Convert.ToSingle(objVal);
                        break;
                    case "ScoutSkill":
                        unit.Info.Scouting.Skill = Convert.ToSingle(objVal);
                        break;
                }
            }
        }


        internal static Ability ApplyPrefabToAbility(Prefab prefab, Unit castingUnit, Ability ability = null)
        {
            Dictionary<string, object> prefabToApply = GetPrefabObject(prefab);

            Ability returnAbility = null;


            if (prefabToApply == null)
                return null;

            if (prefabToApply.TryGetValue("AbilityProfile", out object val) && (ability == null))
            {
                foreach (var profile in AbilityProfiles.Profiles)
                {
                    if (profile.Name == (string)val)
                    {
                        returnAbility = profile.CreateAbility(castingUnit);
                        break;
                    }
                }
            }
            else if (ability != null)
            {
                returnAbility = ability;
            }
            else
            {
                return null;
            }


            ParseDict(prefabToApply, returnAbility);

            return returnAbility;
        }

        internal static void ParseDict(Dictionary<string, object> dict, Ability ability)
        {
            foreach (string key in dict.Keys)
            {
                object objVal = dict[key];

                switch (key)
                {
                    case "AbilityName":
                        ability.Name = (string)objVal;
                        break;
                    case "DamageType":
                        ability.DamageType = (DamageType)Convert.ToInt32(objVal);
                        break;
                    case "Grade":
                        ability.Grade = Convert.ToInt32(objVal);
                        break;
                    case "DecayToFirst":
                        ability.DecayToFirst = Convert.ToInt32(objVal) == 1;
                        break;
                    case "ComboAdvanceCost":
                        ability.ComboAdvanceCost = Convert.ToInt32(objVal);
                        break;
                    case "ComboDecayCost":
                        ability.ComboDecayCost = Convert.ToInt32(objVal);
                        break;
                    case "Castable":
                        ability.Castable = Convert.ToInt32(objVal) == 1;
                        break;
                    case "BreakStealth":
                        ability.BreakStealth = Convert.ToInt32(objVal) == 1;
                        break;
                    case "EnergyCost":
                        ability.EnergyCost = Convert.ToSingle(objVal);
                        break;
                    case "Range":
                        ability.Range = Convert.ToSingle(objVal);
                        break;
                    case "MinRange":
                        ability.MinRange = Convert.ToInt32(objVal);
                        break;
                    case "Damage":
                        ability.Damage = Convert.ToSingle(objVal);
                        break;
                    case "Duration":
                        ability.Duration = Convert.ToInt32(objVal);
                        break;
                    case "Sound":
                        ability.Sound = Convert.ToSingle(objVal);
                        break;
                }
            }
        }
    }
}

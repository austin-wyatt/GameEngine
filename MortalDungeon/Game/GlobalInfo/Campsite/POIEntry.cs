using Empyrean.Game.Map;
using Empyrean.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Save
{
    public enum POIType
    {
        None,
        Campsite,
        City,
    }

    public enum POIParameterType
    {
        /// <summary>
        /// 1 if the POI has ever been entered. <para/> 
        /// 2 if the POI has only been heard of. <para/>
        /// 3 if the POI has been heard of but should display on the map (if applicable). <para/>
        /// </summary>
        Discovered,

        /// <summary>
        /// 1 if the player has physically entered the POI
        /// </summary>
        Explored,

        /// <summary>
        /// The in-game minute that a reset was first requested for the POI. <para/> 
        /// When requested for the first time, an appointment should be made in a scheduler 
        /// for timed events. The scheduled time would be ResetDate + ResetTimer in-game minutes.
        /// When the scheduled event occurs the Cleared and ResetDate fields will be removed if present 
        /// </summary>
        ResetDate,

        /// <summary>
        /// 1 if the POI can be fast traveled to
        /// </summary>
        CanFastTravel,

        /// <summary>
        /// 1 if everything has been completed in a given POI
        /// </summary>
        Cleared,

        /// <summary>
        /// How long in in-game minutes until the POI should be reset. <para/>
        /// Defaults to 7 days if the parameter is not present
        /// </summary>
        ResetTimer,

        /// <summary>
        /// The id of the AnimationSet of this POI. Requires a corresponding value of the
        /// Discovered parameter to be displayed.
        /// </summary>
        VisibleOnMap,

        /// <summary>
        /// 1 if a player is currently inside.
        /// </summary>
        PlayerInside, 

        /// <summary>
        /// A placeholder value that acts as a buffer when a player leaves a POI.
        /// </summary>
        PlayerInsideCounter,
    }

    [Serializable]
    public class POIParameter
    {
        public POIParameterType Field;
        public int Value;

        public override bool Equals(object obj)
        {
            return obj is POIParameter parameter &&
                   Field == parameter.Field &&
                   Value == parameter.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field);
        }
    }

    [Serializable]
    public class POIEntry
    {
        public POIType Type;
        public int Id;
        public FeaturePoint Origin = new FeaturePoint();
        public TextInfo Title = new TextInfo();
        public TextInfo Description = new TextInfo();
        public HashSet<POIParameter> Parameters = new HashSet<POIParameter>(0);
        public bool IsDefault = true;
        public string Name = "";

        public POIEntry() { }

        public POIEntry(int id) 
        {
            Id = id;
        }

        public POIEntry(POIEntry entry) 
        {
            Type = entry.Type;
            Id = entry.Id;
            Origin = entry.Origin;
            Title = new TextInfo(ref entry.Title);
            Description = new TextInfo(ref entry.Description);
            foreach(var item in entry.Parameters)
            {
                Parameters.Add(item);
            }
            IsDefault = entry.IsDefault;
            Name = entry.Name;
        }

        private static POIParameter _tempPOIParameter = new POIParameter();
        private static object _tempLockObject = new object();
        public int GetParameterValue(POIParameterType type)
        {
            lock (_tempLockObject)
            {
                _tempPOIParameter.Field = type;
                if(Parameters.TryGetValue(_tempPOIParameter, out var foundVal))
                {
                    return foundVal.Value;
                }

                return 0;
            }
        }

        public void SetParameterValue(POIParameterType type, int value)
        {
            lock (_tempLockObject)
            {
                _tempPOIParameter.Field = type;
                if (Parameters.TryGetValue(_tempPOIParameter, out var foundVal))
                {
                    if(value == 0)
                    {
                        Parameters.Remove(foundVal);
                    }
                    else
                    {
                        foundVal.Value = value;
                    }
                }
                else if(value != 0)
                {
                    Parameters.Add(new POIParameter() { Field = type, Value = value });
                }
            }
        }

        /// <summary>
        /// Increase the value stored in the parameter field by 1.
        /// If the parameter does not exist, it is created and set to 1.
        /// </summary>
        public void IncrementParameterValue(POIParameterType type)
        {
            lock (_tempLockObject)
            {
                _tempPOIParameter.Field = type;
                if (Parameters.TryGetValue(_tempPOIParameter, out var foundVal))
                {
                    foundVal.Value++;
                }
                else
                {
                    Parameters.Add(new POIParameter() { Field = type, Value = 1 });
                }
            }
        }

        /// <summary>
        /// Lower the value stored in the parameter field by 1 to a minimum of 0.
        /// </summary>
        public void DecrementParameterValue(POIParameterType type)
        {
            lock (_tempLockObject)
            {
                _tempPOIParameter.Field = type;
                if (Parameters.TryGetValue(_tempPOIParameter, out var foundVal))
                {
                    foundVal.Value--;

                    if (foundVal.Value == 0)
                    {
                        Parameters.Remove(foundVal);
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            return obj is POIEntry entry &&
                   Type == entry.Type &&
                   Id == entry.Id &&
                   EqualityComparer<FeaturePoint>.Default.Equals(Origin, entry.Origin) &&
                   EqualityComparer<TextInfo>.Default.Equals(Title, entry.Title) &&
                   EqualityComparer<TextInfo>.Default.Equals(Description, entry.Description) &&
                   EqualityComparer<HashSet<POIParameter>>.Default.Equals(Parameters, entry.Parameters);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}

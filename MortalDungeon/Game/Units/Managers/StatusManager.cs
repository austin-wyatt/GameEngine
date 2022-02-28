using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Units
{
    [Serializable]
    public class StatusManager : ISerializable
    {
        [XmlIgnore]
        public Unit Unit;

        #region status conditions
        public List<StatusCondition> StatusConditions = new List<StatusCondition>();
        public List<StatusCondition> ConditionImmunities = new List<StatusCondition>();

        [XmlIgnore]
        private StatusCondition _condition = StatusCondition.None;
        [XmlIgnore]
        private StatusCondition _immunities = StatusCondition.None;

        public void AddStatusCondition(StatusCondition condition)
        {
            StatusConditions.Add(condition);

            CollateStatusConditions();
        }
        public void RemoveStatusCondition(StatusCondition condition)
        {
            StatusConditions.Remove(condition);

            CollateStatusConditions();
        }

        public void AddStatusImmunity(StatusCondition immunity)
        {
            ConditionImmunities.Add(immunity);

            CollateStatusConditions();
        }
        public void RemoveStatusImmunity(StatusCondition immunity)
        {
            ConditionImmunities.Remove(immunity);

            CollateStatusConditions();
        }

        private void CollateStatusConditions()
        {
            _condition = StatusCondition.None;
            _immunities = StatusCondition.None;

            foreach(var condition in StatusConditions)
            {
                _condition |= condition;
            }

            foreach(var condition in ConditionImmunities)
            {
                _immunities |= condition;
            }
        }

        /// <summary>
        /// Returns true if the passed StatusCondition is affecting the unit, false otherwise
        /// </summary>
        public bool CheckCondition(StatusCondition condition)
        {
            return (_condition & condition) != 0 && ((condition & _immunities) == 0);
        }
        #endregion

        #region unit conditions
        public List<UnitCondition> UnitConditions = new List<UnitCondition>();



        [XmlIgnore]
        private HashSet<UnitCondition> _unitConditions = new HashSet<UnitCondition>();

        public void AddUnitCondition(UnitCondition condition)
        {
            UnitConditions.Add(condition);

            CollateUnitConditions();
        }
        public void RemoveUnitCondition(UnitCondition condition)
        {
            UnitConditions.Remove(condition);

            CollateUnitConditions();
        }

        private void CollateUnitConditions()
        {
            _unitConditions.Clear();

            foreach (var condition in UnitConditions)
            {
                _unitConditions.Add(condition);
            }
        }

        /// <summary>
        /// Returns true if the unit condition is present
        /// </summary>
        public bool CheckCondition(UnitCondition condition)
        {
            return _unitConditions.Contains(condition);
        }
        #endregion

        public void CompleteDeserialization()
        {

        }

        public void PrepareForSerialization()
        {

        }
    }
}

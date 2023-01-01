using Empyrean.Game.Abilities;
using Empyrean.Game.Map;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Logger
{
    public enum LoggerEventType
    {
        Unknown,

        UnitMove,
        UnitDamaged,
        UnitKilled,
        UnitSpawned,
        UnitAggroed,

        DialogueStart,
        DialogueOption,
        DialogueCancel,
        DialogueEnd,

        MenuOpened,
        MenuClosed,

        InventoryChanged,
        GoldChanged,

        SkillsModified,

        TimeOfDay,
        DayChange,

        QuestStart,
        QuestObjective,
        QuestCompleted,
        QuestFailed,
        QuestCancelled,

        LocationEntered,
        LocationExited,
        FastTravel
    }

    /// <summary>
    /// Helper class to build logger packets
    /// </summary>
    public class LoggerPacket : Dictionary<string, object>
    {
        public static LoggerPacket BuildPacket_UnitMove(UnitInfo movedUnit, FeaturePoint source, string cause) 
        {
            return new LoggerPacket()
            {
                { "id", LoggerEventType.UnitMove },
                { "Unit", movedUnit },
                { "SourcePoint", source },
                { "Cause", cause }
            };
        }

        public static LoggerPacket BuildPacket_UnitKilled(Unit killedUnit, Unit killingUnit)
        {
            return new LoggerPacket()
            {
                { "id", LoggerEventType.UnitKilled },
                { "killedUnit", killedUnit },
                { "killingUnit", killingUnit },
            };
        }
    }
}

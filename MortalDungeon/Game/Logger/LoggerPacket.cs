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
        const string ID_STRING = "id";

        public static LoggerPacket BuildPacket_UnitMove(UnitInfo movedUnit, FeaturePoint source, string cause) 
        {
            return new LoggerPacket()
            {
                { ID_STRING, LoggerEventType.UnitMove },
                { "Unit", movedUnit },
                { "SourcePoint", source },
                { "Cause", cause }
            };
        }

        public static LoggerPacket BuildPacket_UnitKilled(Unit killedUnit, Unit killingUnit)
        {
            return new LoggerPacket()
            {
                { ID_STRING, LoggerEventType.UnitKilled },
                { "killedUnit", killedUnit },
                { "killingUnit", killingUnit },
            };
        }

        public static LoggerPacket BuildPacket_UnitHurt(Unit hurtUnit, float damageTaken)
        {
            return new LoggerPacket()
            {
                { ID_STRING, LoggerEventType.UnitDamaged },
                { "damageTaken", damageTaken },
                { "hurtUnit", hurtUnit },
            };
        }
    }
}

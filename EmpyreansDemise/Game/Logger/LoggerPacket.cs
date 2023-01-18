using Empyrean.Engine_Classes;
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
        FastTravel,
    }

    /// <summary>
    /// Helper class to build logger packets
    /// </summary>
    public class LoggerPacket : Dictionary<string, object>
    {
        static LoggerPacket()
        {
            FillDataObjectPool();
        }

        const string ID_STRING = "id";
        const string PATH_STRING = "path";

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


        private static ObjectPool<LoggerPacket> _dataObjectChangedPool = new ObjectPool<LoggerPacket>(10);
        private static void FillDataObjectPool()
        {
            for(int i = 0; i < 10; i++)
            {
                _dataObjectChangedPool.FreeObject(new LoggerPacket()
                {
                    { ID_STRING, "" },
                    { DATA_OBJECT_CHANGED_IDENT, 0 }
                });
            }
        }

        public const string DATA_OBJECT_CHANGED_IDENT = "+DOC";
        public static LoggerPacket BuildPacket_DataObjectChanged(string path)
        {
            LoggerPacket packet = _dataObjectChangedPool.GetObject();

            packet[ID_STRING] = path;
            packet[DATA_OBJECT_CHANGED_IDENT] = 0;
            return packet;
        }
        public static void ReturnPacket_DataObjectChanged(LoggerPacket packet)
        {
            _dataObjectChangedPool.FreeObject(packet);
        }
    }
}

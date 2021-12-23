using MortalDungeon.Game.Quests;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Ledger
{
    public enum LedgerUpdateType
    {
        Dialogue,
        Feature,
        Quest,
        GeneralState
    }

    public static class Ledgers
    {
        public static void LedgerUpdated(LedgerUpdateType type, long id, long data)
        {
            for(int i = QuestManager.Quests.Count - 1; i >= 0; i--)
            {
                QuestManager.Quests[i].CheckObjectives(type, id, data);
            }
        }

        public static void OnUnitKilled(Unit unit)
        {
            //update the feature ledger stating that this unit has died
            if (unit.FeatureID != 0)
            {
                FeatureLedger.AddInteraction(unit.FeatureID, FeatureInteraction.Killed, unit.FeatureHash);
            }


        }

        public static void OnUnitRevived(Unit unit)
        {
            //update the feature ledger stating that this unit has been revived
            if (unit.FeatureID != 0)
            {
                FeatureLedger.AddInteraction(unit.FeatureID, FeatureInteraction.Revived, unit.FeatureHash);
            }
        }
    }
}

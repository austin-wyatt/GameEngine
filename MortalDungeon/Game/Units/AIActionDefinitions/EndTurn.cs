﻿using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class EndTurn : UnitAIAction
    {
        internal EndTurn(Unit castingUnit, Ability ability = null, BaseTile tile = null, Unit unit = null) : base(castingUnit, AIAction.EndTurn, ability, tile, unit) { }

        internal override void EnactEffect()
        {
            Scene.CompleteTurn();
        }
    }
}

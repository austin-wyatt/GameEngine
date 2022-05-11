using Empyrean.Game.Abilities;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Units.AI
{
    class EndTurn : UnitAIAction
    {
        public EndTurn(Unit castingUnit, Ability ability = null, Tile tile = null, Unit unit = null) : base(castingUnit, AIAction.EndTurn, ability, tile, unit) { }

        public override void EnactEffect()
        {
            Scene.CompleteTurn();
        }
    }
}

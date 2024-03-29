﻿using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Events;
using Empyrean.Game.Player;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Empyrean.Definitions.EventActions
{
    /// <summary>
    /// Starts the dialogue of the passed Id with Speaker 0 as the target
    /// </summary>
    internal class StartDialogue : EventAction
    {
        int DialogueId;

        /// <summary>
        /// Parameters[0]: int (dialogue id)
        /// </summary>
        public static List<string> PARAMETERS = new List<string> { "int" };
        public static List<string> PARAMETER_NAMES = new List<string> { "Dialogue id" };

        public override void BuildEvent(List<dynamic> parameters)
        {
            DialogueId = parameters[0];
        }

        public override void Invoke(params dynamic[] parameters)
        {
            if (!Conditional.Check()) 
                return;

            Unit unit = parameters[0];
            var dialogue = DialogueManager.GetDialogue(DialogueId);

            CombatScene.DialogueWindow.StartDialogue(dialogue, new List<Unit> { unit, PlayerParty.UnitsInParty.First() });
        }
    }
}

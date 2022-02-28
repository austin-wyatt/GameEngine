using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Events;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Definitions.EventActions
{
    /// <summary>
    /// Creates a context menu or adds an item to an existing context menu associated with an event target <para/>
    /// </summary>
    internal class AddMenuItem : EventAction
    {
        TextInfo MenuText = new TextInfo();
        EventAction MenuClickAction;

        /// <summary>
        /// Parameters[0]: TextInfo (the string that will be displayed in the context menu) <para/>
        /// Parameters[1]: EventAction (the event action that will be triggered when the menu item is clicked) <para/>
        /// </summary>
        public static readonly List<string> PARAMETERS = new List<string> { "TextInfo", "EventAction" };
        public static List<string> PARAMETER_NAMES = new List<string> { "Menu text", "Menu click action" };

        public override void BuildEvent(List<dynamic> parameters)
        {
            MenuText = parameters[0];
            EventActionBuilder action = (EventActionBuilder)parameters[1];

            MenuClickAction = action.BuildAction();
        }

        public override void Invoke(params dynamic[] parameters)
        {
            if (!Conditional.Check())
                return;

            IEventTarget target = parameters[0];

            UIList list;

            if (target.EventObjects.TryGetValue("context_menu", out var obj))
            {
                list = obj as UIList;
            }
            else
            {
                string name = "<context_menu>";
                var unit = target as Unit;

                if (unit != null)
                {
                    name = unit.Name;
                }

                (Tooltip menu, UIList uiList) = UIHelpers.GenerateContextMenuWithList(name);
                list = uiList;

                UIHelpers.CreateContextMenu(EventManager.Scene, menu, EventManager.Scene._tooltipBlock);

                target.EventObjects.AddOrSet("context_menu", uiList);

                menu.OnCleanUp += (s) =>
                {
                    target.EventObjects.Remove("context_menu");
                };
            }

            list.AddItem(MenuText.ToString(), (_) =>
            {
                MenuClickAction.Invoke(target);
            });
        }
    }
}

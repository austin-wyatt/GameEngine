using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class Footer : UIObject
    {
        public List<Button> Buttons = new List<Button>();
        public Footer(float height = 100)
        {
            Position = new Vector3(WindowConstants.ScreenUnits.X / 2, WindowConstants.ScreenUnits.Y - height / 4 + height / 200, 0);
            Name = "Footer";

            Clickable = true;
            Hoverable = true;

            UIBlock window = new UIBlock(Position, new Vector2(2, height / WindowConstants.ScreenUnits.Y), default, 90, false);
            BaseComponent = window;

            AddChild(window);


            //ToggleableButton testButton = new ToggleableButton(window.Origin + new Vector3(140, height / 2, 0), new Vector2(0.5f, 0.15f), "Move", 0.75f) { Draggable = true };
            //AddChild(testButton, 100);

            //ToggleableButton button2 = new ToggleableButton(window.Origin + new Vector3(290, height / 2, 0), new Vector2(0.5f, 0.15f), "Melee", 0.75f);
            //AddChild(button2, 100);

            //ToggleableButton button3 = new ToggleableButton(window.Origin + new Vector3(440, height / 2, 0), new Vector2(0.5f, 0.15f), "Range", 0.75f);
            //AddChild(button3, 100);

            //Buttons.Add(testButton);
            //Buttons.Add(button2);
            //Buttons.Add(button3);

            ValidateObject(this);
        }
    }
}

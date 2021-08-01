using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class Footer : UIObject
    {
        public List<Button> Buttons = new List<Button>();
        public Footer(float height)
        {
            Position = new Vector3(WindowConstants.ScreenUnits.X / 2, WindowConstants.ScreenUnits.Y - height / 4 + height / 200, 0);
            Name = "Footer";
            Size = new UIScale(2, height / WindowConstants.ScreenUnits.Y);

            Clickable = true;
            Hoverable = true;

            UIBlock window = new UIBlock(Position, Size, default, 90, false);
            BaseComponent = window;

            AddChild(window);


            ValidateObject(this);
        }
    }
}

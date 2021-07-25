using OpenTK.Mathematics;

namespace MortalDungeon.Game.UI
{
    public class Footer : UIObject
    {
        public Footer(float height = 100)
        {
            Position = new Vector3(WindowConstants.ScreenUnits.X / 2, WindowConstants.ScreenUnits.Y - height / 4 + height / 200, 0);
            Name = "Footer";

            Clickable = true;

            UIBlock window = new UIBlock(Position, new Vector2(2, height / WindowConstants.ScreenUnits.Y), default, 90, false);
            AddChild(window);


            Button testButton = new Button(window.Origin + new Vector3(140, height / 2, 0), new Vector2(500, 150), "Move", 0.75f);
            AddChild(testButton);

            Button button2 = new Button(window.Origin + new Vector3(290, height / 2, 0), new Vector2(500, 150), "Melee", 0.75f);
            AddChild(button2);

            Button button3 = new Button(window.Origin + new Vector3(440, height / 2, 0), new Vector2(500, 150), "Range", 0.75f);
            AddChild(button3);
        }
    }
}

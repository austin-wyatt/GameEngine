using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class UIObject : GameObject
    {
        public bool Render = true;
        List<UIObject> nestedObjects = new List<UIObject>(); //nested objects will be placed based off of their positional offset from the parent
        public UIObject() { }
    }

    public class TextBox : UIObject
    {
        public TextBox()
        {

        }
    }

    public class Footer : UIObject
    {
        public Footer()
        {

        }
    }

    public class SideBar : UIObject
    {
        public SideBar()
        {

        }
    }
}

using Empyrean.Game.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Objects.UIComponents
{
    public class MouseCursor : UIObject
    {
        public MouseCursor()
        {
            BaseObject windowObj = new BaseObject(CURSOR_1_ANIMATION.List, 0, "MouseCursor", default, EnvironmentObjects.UIBlockBounds);
            windowObj.BaseFrame.CameraPerspective = false;

            AddBaseObject(windowObj);
            _baseObject = windowObj;

            SetSize(new UIScale(0.1f, 0.1f));

            ValidateObject(this);
        }
    }
}

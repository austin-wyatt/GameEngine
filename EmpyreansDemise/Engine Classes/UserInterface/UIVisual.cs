using Empyrean.Engine_Classes.Rendering;
using Empyrean.Game.Objects;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.UserInterface
{
    public class UIVisualInfo
    {
        public float[] TextureCoordinates = null;
        public List<Vector4> ExtraColors = null;

        public SpritesheetObject SpritesheetObject = null;
        public Texture Texture = null;
    }

    public enum VisualType
    {
        Color,
        Gradient,
        Texture,
        SpritesheetEntry,
    }

    public class UIVisual
    {
        public VisualType RenderType = VisualType.Color;

        public Vector4 Color = Vector4.One;

        private VisualTransform _visualTransform = null;
        private bool _hasVisualTransform = false;

        public float[] Vertices = StaticObjects.QUAD_VERTICES;

        public UIVisualInfo VisualInfo = null;

        public UIVisual(VisualType type)
        {
            SetVisualType(type);
        }

        public void SetVisualType(VisualType type)
        {
            if (type == RenderType)
                return;

            switch (type)
            {
                case VisualType.Color:
                    VisualInfo = null;
                    break;
                case VisualType.Gradient:
                    VisualInfo = VisualInfo == null ? new UIVisualInfo() : VisualInfo;
                    if(VisualInfo.ExtraColors == null)
                        VisualInfo.ExtraColors = new List<Vector4>();
                    break;
                case VisualType.Texture:
                    VisualInfo = VisualInfo == null ? new UIVisualInfo() : VisualInfo;
                    if (VisualInfo.TextureCoordinates == null)
                        VisualInfo.TextureCoordinates = StaticObjects.TEXTURE_COORDS;
                    break;
                case VisualType.SpritesheetEntry:
                    VisualInfo = VisualInfo == null ? new UIVisualInfo() : VisualInfo;
                    if (VisualInfo.TextureCoordinates == null)
                        VisualInfo.TextureCoordinates = StaticObjects.TEXTURE_COORDS;
                    break;
            }
        }
        public void ApplyTexture(Texture texture)
        {
            if(RenderType != VisualType.Texture)
                SetVisualType(VisualType.Texture);

            VisualInfo.Texture = texture;
        }

        public void ApplySpritesheet(int index, Spritesheet spritesheet = null)
        {
            if (RenderType != VisualType.SpritesheetEntry)
                SetVisualType(VisualType.SpritesheetEntry);

            bool checkTexture = false;

            if(spritesheet != null)
            {
                VisualInfo.SpritesheetObject = new SpritesheetObject(index, spritesheet);
                checkTexture = true;
            }
            else if (spritesheet == VisualInfo.SpritesheetObject.Spritesheet)
            {
                VisualInfo.SpritesheetObject.SpritesheetPosition = index;
            }
            else 
            {
                VisualInfo.SpritesheetObject.Spritesheet = spritesheet;
                VisualInfo.SpritesheetObject.SpritesheetPosition = index;
                checkTexture = true;
            }

            if (checkTexture)
            {
                if (Renderer._textures.TryGetValue(spritesheet.TextureId, out Texture tex))
                {
                    VisualInfo.Texture = tex;
                }
                else
                {
                    Window.InvokeOnMainThread(() =>
                    {
                        VisualInfo.Texture = Renderer.LoadTextureFromSpritesheet(spritesheet);
                    });
                }
            }
        }

        public void SetVertices(float[] vertices)
        {
            Vertices = vertices;
        }

        public bool TryGetVisualTransform(out VisualTransform transform)
        {
            transform = _visualTransform;
            return _hasVisualTransform;
        }
    }
}

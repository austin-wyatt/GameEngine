using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Empyrean.Engine_Classes.Text
{
    public enum TextAlignment
    {
        LeftAlign,
        Center,
        RightAlign,
    }

    public class TextString
    {
        public string Text;
        public List<TextCharacter> Characters = new List<TextCharacter>();

        //some way to access the desired font
        //public string FontName;
        public LoadedFont Font;

        //Position in screen space coordinates
        public Vector3 Position;

        public Vector4 TextColor = new Vector4(1, 1, 1, 1);

        public TextAlignment TextAlignment;

        public float LineHeightMultiplier = 1;

        public Vector2 TextScale = new Vector2(1, 1);

        private object _textEditLock = new object();

        public TextString(LoadedFont font, TextAlignment alignment = TextAlignment.LeftAlign)
        {
            Font = font;
            TextAlignment = alignment;
        }
        public void SetText(string newText)
        {
            Monitor.Enter(_textEditLock);

            bool textChanged = Text != newText;

            Text = newText;

            if (Characters.Count > Text.Length)
            {
                Characters.RemoveRange(Text.Length, Characters.Count - Text.Length);
            }

            for (int i = 0; i < Text.Length; i++)
            {
                if(Characters.Count > i)
                {
                    Characters[i].SetCharacter(Text[i]);
                }
                else
                {
                    Characters.Add(Font.GetCharacter(Text[i]));
                }

                Characters[i].SetScale(TextScale.X, TextScale.Y, 1);
                Characters[i].Color = TextColor;
            }

            if (textChanged)
            {
                PositionCharacters();
            }

            Monitor.Exit(_textEditLock);
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;

            PositionCharacters();
        }

        public void SetTextScale(float x, float y)
        {
            TextScale.X = x;
            TextScale.Y = y;

            for(int i = 0; i < Characters.Count; i++)
            {
                Characters[i].SetScale(TextScale.X, TextScale.Y, 1);
            }

            PositionCharacters();
        }

        public void PositionCharacters()
        {
            Monitor.Enter(_textEditLock);

            List<Range> lines = new List<Range>(5);

            int start = 0;
            for(int i = 0; i < Characters.Count; i++)
            {
                if(Characters[i].Glyph.CharacterValue == '\n')
                {
                    lines.Add(new Range(start, i));
                    start = i + 1;
                }
            }

            lines.Add(new Range(start, Characters.Count));

            float lineHeight = (float)Font.LineHeight / WindowConstants.ClientSize.Y * WindowConstants.ScreenUnits.Y;

            for (int i = 0; i < lines.Count; i++)
            {
                Vector3 baseLinePosition = new Vector3(Position);
                baseLinePosition.Y += i * lineHeight * LineHeightMultiplier;

                switch (TextAlignment)
                {
                    case TextAlignment.LeftAlign:
                        LeftAlignRange(lines[i], baseLinePosition);
                        break;
                    case TextAlignment.Center:
                        break;
                    case TextAlignment.RightAlign:
                        break;
                }
            }

            Monitor.Exit(_textEditLock);
        }

        private void LeftAlignRange(Range range, Vector3 basePosition)
        {
            Vector3 currPosition;

            //test1234q0z

            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                TextCharacter character = Characters[i];

                Vector2 screenBearing = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(character.Glyph.Bearing);

                UIDimensions dim = character.GetDimensions();

                currPosition = basePosition;
                currPosition.X += screenBearing.X * 0.5f * character.CurrentScale.X;
                currPosition.Y += dim.Y - screenBearing.Y * character.CurrentScale.Y;

                character.SAP(currPosition, UIAnchorPosition.BottomLeft);

                float screenAdvance = (float)character.Glyph.Advance / WindowConstants.ClientSize.Y * 
                    WindowConstants.ScreenUnits.Y * 0.5f * character.CurrentScale.X * 1.2f;

                basePosition.X += screenAdvance;
            }
        }
    }
}

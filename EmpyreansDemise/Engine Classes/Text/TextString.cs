using OpenTK.Mathematics;
using SharpFont;
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

    public enum VerticalAlignment
    {
        Top,
        Center
    }

    public class TextString
    {
        public string Text;
        public List<TextCharacter> Characters = new List<TextCharacter>();

        public FontInfo FontInfo;

        //Position in screen space coordinates
        public Vector3 Position;

        public Vector4 TextColor = new Vector4(1, 1, 1, 1);

        public TextAlignment TextAlignment;
        public VerticalAlignment VerticalAlignment = VerticalAlignment.Top;

        public float LineHeightMultiplier = 1;

        public Vector2 TextScale = new Vector2(1, 1);

        private object _textEditLock = new object();

        private UIDimensions _dimensions = new UIDimensions();

        public TextString(FontInfo font, TextAlignment horizontalAlignment = TextAlignment.LeftAlign)
        {
            //Font = font;
            FontInfo = font;
            TextAlignment = horizontalAlignment;
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
                    Characters[i].SetCharacter(Text[i], FontInfo);
                }
                else
                {
                    Characters.Add(GlyphLoader.GetCharacter(Text[i], FontInfo));
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

        public void SetTextColor(float r, float g, float b, float a)
        {
            SetTextColor(new Vector4(r, g, b, a));
        }

        public void SetTextColor(Vector4 color)
        {
            TextColor = color;
            for(int i = 0; i < Characters.Count; i++)
            {
                Characters[i].Color = TextColor;
            }
        }

        public UIDimensions GetDimensions()
        {
            if(Characters.Count == 0)
                return new UIDimensions();

            return _dimensions;
        }

        public float GetCharacterHeight()
        {
            return (float)FontInfo.GetFace().Size.Metrics.Height.ToInt32() / WindowConstants.ClientSize.Y * WindowConstants.ScreenUnits.Y;
        }

        public float GetDescender()
        {
            return (float)FontInfo.GetFace().Size.Metrics.Descender.ToInt32() / WindowConstants.ClientSize.Y * WindowConstants.ScreenUnits.Y;
        }

        public void PositionCharacters()
        {
            Monitor.Enter(_textEditLock);

            List<Range> lines = new List<Range>(5);

            List<float> lineHeights = new List<float>(5);

            float totalHeight = 0;

            float lineHeight = 0;

            int start = 0;
            for(int i = 0; i < Characters.Count; i++)
            {
                if (Characters[i].Glyph.CharacterValue == '\n')
                {
                    lines.Add(new Range(start, i));

                    lineHeight = Characters[i].Glyph.LineHeight;

                    lineHeights.Add(lineHeight);
                    start = i + 1;
                    totalHeight += lineHeight;
                }
            }

            if(Characters.Count > 0)
            {
                lineHeight = Characters[0].Glyph.LineHeight;
                totalHeight += lineHeight;
            }

            lines.Add(new Range(start, Characters.Count));
            lineHeights.Add(lineHeight);

            Vector4 bounds = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            for (int i = 0; i < lines.Count; i++)
            {
                lineHeight = lineHeights[i];

                Vector3 baseLinePosition = new Vector3(Position);

                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        baseLinePosition.Y += totalHeight / 2;
                        break;
                }

                baseLinePosition.Y += i * lineHeight * LineHeightMultiplier;

                Vector4 newBounds = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
                switch (TextAlignment)
                {
                    case TextAlignment.LeftAlign:
                        LeftAlignRange(lines[i], baseLinePosition, out newBounds);
                        break;
                    case TextAlignment.Center:
                        CenterRange(lines[i], baseLinePosition, out newBounds);
                        break;
                    case TextAlignment.RightAlign:
                        break;
                }

                //min
                bounds.X = newBounds.X < bounds.X ? newBounds.X : bounds.X;
                bounds.Y = newBounds.Y < bounds.Y ? newBounds.Y : bounds.Y;
                //max
                bounds.Z = newBounds.X > bounds.Z ? newBounds.X : bounds.X;
                bounds.W = newBounds.Y > bounds.W ? newBounds.Y : bounds.Y;
            }

            _dimensions = new UIDimensions(bounds.Z - bounds.X, bounds.W - bounds.Y);

            Monitor.Exit(_textEditLock);
        }

        private void LeftAlignRange(Range range, Vector3 basePosition, out Vector4 bounds)
        {
            Vector3 currPosition;
            bounds = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                TextCharacter character = Characters[i];
                var face = character.Glyph.FontInfo.GetFace();
                bool kerningEnabled = face.HasKerning;

                Vector2 kerning = new Vector2();

                if (i < range.End.Value - 1)
                {
                    if (kerningEnabled)
                    {
                        FTVector26Dot6 rawKerning = face.GetKerning(character.Glyph.FreeTypeGlyphIndex,
                        Characters[i + 1].Glyph.FreeTypeGlyphIndex, KerningMode.Default);

                        kerning.X = (float)rawKerning.X.ToDouble();
                        kerning.Y = (float)rawKerning.Y.ToDouble();

                        kerning = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(kerning);
                    }
                }

                Vector2 screenBearing = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(character.Glyph.Bearing);

                UIDimensions dim = character.GetDimensions();

                currPosition = basePosition;
                currPosition.X += screenBearing.X * character.CurrentScale.X + kerning.X;
                currPosition.Y += dim.Y - screenBearing.Y * character.CurrentScale.Y + kerning.Y;

                character.SAP(currPosition, UIAnchorPosition.BottomLeft);

                //min
                bounds.X = currPosition.X < bounds.X ? currPosition.X : bounds.X;
                bounds.Y = currPosition.Y < bounds.Y ? currPosition.Y : bounds.Y;
                //max
                bounds.Z = currPosition.X > bounds.Z ? currPosition.X : bounds.X;
                bounds.W = currPosition.Y > bounds.W ? currPosition.Y : bounds.Y;

                float screenAdvance = (float)character.Glyph.Advance / WindowConstants.ClientSize.Y *
                    WindowConstants.ScreenUnits.Y * character.CurrentScale.X / WindowConstants.AspectRatio;

                basePosition.X += screenAdvance;
            }
        }

        private void CenterRange(Range range, Vector3 basePosition, out Vector4 bounds)
        {
            Vector3 currPosition;
            bounds = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            List<Vector3> calculatedPositions = new List<Vector3>(range.End.Value - range.Start.Value);

            Vector3 posOffset = new Vector3();

            Vector3 initialPos = basePosition;

            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                TextCharacter character = Characters[i];
                var face = character.Glyph.FontInfo.GetFace();
                bool kerningEnabled = face.HasKerning;

                Vector2 kerning = new Vector2();

                if (i < range.End.Value - 1)
                {
                    if (kerningEnabled)
                    {
                        FTVector26Dot6 rawKerning = face.GetKerning(character.Glyph.FreeTypeGlyphIndex,
                        Characters[i + 1].Glyph.FreeTypeGlyphIndex, KerningMode.Default);

                        kerning.X = (float)rawKerning.X.ToDouble();
                        kerning.Y = (float)rawKerning.Y.ToDouble();

                        kerning = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(kerning);
                    }
                }

                Vector2 screenBearing = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(character.Glyph.Bearing);

                UIDimensions dim = character.GetDimensions();

                currPosition = basePosition;
                currPosition.X += screenBearing.X * character.CurrentScale.X + kerning.X;
                currPosition.Y += dim.Y - screenBearing.Y * character.CurrentScale.Y + kerning.Y;

                calculatedPositions.Add(currPosition);

                //character.SAP(currPosition, UIAnchorPosition.BottomLeft);


                float screenAdvance = (float)character.Glyph.Advance / WindowConstants.ClientSize.Y *
                    WindowConstants.ScreenUnits.Y * character.CurrentScale.X / WindowConstants.AspectRatio;

                basePosition.X += screenAdvance;
            }

            posOffset.X = (basePosition.X - initialPos.X) / 2;

            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                TextCharacter character = Characters[i];
                currPosition = calculatedPositions[i - range.Start.Value] - posOffset;

                character.SAP(currPosition, UIAnchorPosition.BottomLeft);

                //min
                bounds.X = currPosition.X < bounds.X ? currPosition.X : bounds.X;
                bounds.Y = currPosition.Y < bounds.Y ? currPosition.Y : bounds.Y;
                //max
                bounds.Z = currPosition.X > bounds.Z ? currPosition.X : bounds.X;
                bounds.W = currPosition.Y > bounds.W ? currPosition.Y : bounds.Y;
            }
        }
    }
}

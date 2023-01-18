using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Text
{
    public class Glyph
    {
        public readonly int CharacterValue = '~';
        public int GlyphIndex;

        //All dimension properties are stored as Global coordinates
        //ie [0, ClientSize]

        /// <summary>
        /// Size of the glyph in pixels
        /// </summary>
        public readonly Vector2 Size;

        /// <summary>
        /// The left and top offset of the glyph in pixels
        /// </summary>
        public readonly Vector2 Bearing;

        /// <summary>
        /// The width of the glyph in pixels
        /// </summary>
        public readonly int Advance;
        public readonly LoadedFont Font;

        public bool Render = true;

        public Glyph(int characterValue, int glyphIndex, Vector2 size, Vector2 bearing, int advance, LoadedFont font)
        {
            CharacterValue = characterValue;
            GlyphIndex = glyphIndex;
            Size = size;
            Bearing = bearing;
            Advance = advance;
            Font = font;
        }
    }


}

using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Text
{
    public class Glyph
    {
        public readonly int CharacterValue = '~';
        public int GlyphSSBOIndex;

        /// <summary>
        /// The glyph index as retrieved by the FreeType face
        /// </summary>
        public uint FreeTypeGlyphIndex;

        //All dimension properties are stored as Global coordinates
        //ie [0, ClientSize]

        /// <summary>
        /// Size of the glyph in pixels
        /// </summary>
        public Vector2 Size;

        /// <summary>
        /// The left and top offset of the glyph in pixels
        /// </summary>
        public Vector2 Bearing;

        /// <summary>
        /// The width of the glyph in pixels
        /// </summary>
        public int Advance;

        public float LineHeight;
        public float Descender;

        public bool Render = true;

        public FontInfo FontInfo;

        public Glyph(int characterValue, int glyphIndex, Vector2 size, Vector2 bearing, int advance, uint freeTypeGlyphIndex, FontInfo fontInfo)
        {
            CharacterValue = characterValue;
            GlyphSSBOIndex = glyphIndex;
            Size = size;
            Bearing = bearing;
            Advance = advance;
            FreeTypeGlyphIndex = freeTypeGlyphIndex;
            FontInfo = fontInfo;
        }
    }


}

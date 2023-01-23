using SharpFont;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Text
{
    public class FontInfo
    {
        private static Library _library;

        public string BasePath;
        public string FullPath;
        public int FontSize;
        public bool IsLocalPath = false;

        private static Dictionary<string, Dictionary<int, SharpFont.Face>> FaceMap = new Dictionary<string, Dictionary<int, SharpFont.Face>>();

        public FontInfo(string path, int fontSize, bool isLocalPath = false)
        {
            BasePath = path;
            FullPath = GetFullFontPath(path);
            FontSize = fontSize;
            IsLocalPath = isLocalPath;
        }

        public FontInfo(FontInfo info, int fontSize)
        {
            BasePath = info.BasePath;
            FullPath = info.FullPath;
            FontSize = fontSize;
            IsLocalPath = info.IsLocalPath;
        }

        static FontInfo()
        {
            _library = new Library();
        }

        public SharpFont.Face GetFace()
        {
            SharpFont.Face face;
            Dictionary<int, SharpFont.Face> faceDict;

            lock (FaceMap)
            {
                if (FaceMap.TryGetValue(FullPath, out faceDict))
                {
                    if (faceDict.TryGetValue(FontSize, out face))
                    {
                        return face;
                    }

                    face = new SharpFont.Face(_library, FullPath);
                    face.SetCharSize(FontSize, FontSize, 0, GlyphLoader.SCREEN_DPI);

                    faceDict.Add(FontSize, face);
                    return face;
                }

                faceDict = new Dictionary<int, SharpFont.Face>();
                face = new SharpFont.Face(_library, FullPath);
                face.SetCharSize(FontSize, FontSize, 0, GlyphLoader.SCREEN_DPI);

                faceDict.Add(FontSize, face);
                FaceMap.Add(FullPath, faceDict);

                return face;
            }
        }

        public static string GetFullFontPath(string fontName, bool fontIsLocalPath = false)
        {
            string baseFontPath;

            if (fontIsLocalPath)
            {
                baseFontPath = @".\";
            }
            else
            {
                switch (WindowConstants.CurrentOS)
                {
                    case OSType.OSX:
                        baseFontPath = @"System\Library\Fonts\";
                        break;
                    case OSType.Linux:
                        baseFontPath = @"System\Library\Fonts\";
                        break;
                    case OSType.Windows:
                    default:
                        baseFontPath = @"C:\Windows\Fonts\";
                        break;
                }
            }

            return baseFontPath + fontName;
        }
    }
}

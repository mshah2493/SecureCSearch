using O2S.Components.PDF4NET.Graphics.Fonts;
using System;
using System.IO;
using System.Linq;

namespace securePDFmerging
{
    class FontsProcessor
    {
        /**
         * Set values of font and returns objects of FontBase
         * 
         */
        public PDFFontBase GetFontBase(String fontName, double fontSize)
        {
            String fontFace = null;
            String[] f = null;
            String FILE_PATH = Directory.GetCurrentDirectory() + "/TrueTypeFonts/";
            String fName = fontName;

            if (fontName.Contains(',') & !fontName.Contains("PS") & !fontName.Contains("MT"))
            {
                f = fontName.Split(',');
            }
            else if (fontName.Contains('-') & !fontName.Contains("PS") & !fontName.Contains("MT"))
            {
                f = fontName.Split('-');
            }

            if (f != null && f.Length > 1)
            {
                fName = f[0];
                fontFace = f[1];
            }
            if (fontName.Contains("-BoldItalic"))
            {
                fontFace = "BoldItalic";
                fName = fontName.Replace("-BoldItalic", "");
            }
            else if (fontName.Contains("-Italic"))
            {
                fontFace = "Italic";
                fName = fontName.Replace("-Italic", "");
            }
            else if (fontName.Contains("-Bold"))
            {
                fontFace = "Bold";
                fName = fontName.Replace("-Bold", "");
            }

            PDFFontBase font;

            if (fName != null && (fName == "Times New Roman" || fName == "TimesNewRomanPSMT") && fontFace == null)
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "times.ttf"), fontSize, true);
            }
            else if (fName != null && (fName == "Times New Roman" || fName == "TimesNewRomanPSMT") && fontFace != null && fontFace.Equals("Bold"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "timesbd.ttf"), fontSize, true);
            }
            else if (fName != null && (fName == "Times New Roman" || fName == "TimesNewRomanPSMT") && fontFace != null && fontFace.Equals("Italic"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "timesi.ttf"), fontSize, true);
            }
            else if (fName != null && (fName == "Times New Roman" || fName == "TimesNewRomanPSMT") && fontFace != null && fontFace.Equals("BoldItalic"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "timesbi.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Calibri" && fontFace == null)
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "calibri.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Calibri" && fontFace != null && fontFace.Equals("Bold"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "calibrib.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Calibri" && fontFace != null && fontFace.Equals("Italic"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "calibrii.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Calibri" && fontFace != null && fontFace.Equals("BoldItalic"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "calibriz.ttf"), fontSize, true);
            }
            else if (fName != null && (fName == "Arial" || fName == "ArialMT") && fontFace == null)
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "arial.ttf"), fontSize, true);
            }
            else if (fName != null && (fName == "Arial" || fName == "ArialMT") && fontFace != null && fontFace.Equals("Bold"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "arialbd.ttf"), fontSize, true);
            }
            else if (fName != null && (fName == "Arial" || fName == "ArialMT") && fontFace != null && fontFace.Equals("Italic"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "ariali.ttf"), fontSize, true);
            }
            else if (fName != null && (fName == "Arial" || fName == "ArialMT") && fontFace != null && fontFace.Equals("BoldItalic"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "arialbi.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Kalinga" && fontFace == null)
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "kalinga.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Kalinga" && fontFace != null && fontFace.Equals("Bold"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "kalingab.ttf"), fontSize, true);
            }
            else if (fName != null && (fName == "Symbol" || fName == "SymbolMT"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "Symbol.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Kalinga" && fontFace != null && fontFace.Equals("Bold"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "kalingab.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Helvetica" && fontFace == null)
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "helvetica.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Helvetica" && fontFace != null && fontFace.Equals("Bold"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "helveticab.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Georgia" && fontFace == null)
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "georgia.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Georgia" && fontFace != null && fontFace.Equals("Bold"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "georgiab.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Georgia" && fontFace != null && fontFace.Equals("Italic"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "georgiai.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "Georgia" && fontFace != null && fontFace.Equals("BoldItalic"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "georgiaz.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "YuGothic" && fontFace != null && fontFace.Equals("Bold"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "yugothib.ttf"), fontSize, true);
            }
            else if (fName != null && fName == "YuGothic" && fontFace != null && fontFace.Equals("Regular"))
            {
                font = new TrueTypeFont(Path.GetFullPath(FILE_PATH + "yugothic.ttf"), fontSize, true);
                font.Italic = true;
            }
            else
            {
                if (fName != null && fName.Equals("Times") && fontFace != null && fontFace.Equals("Roman"))
                {
                    font = new PDFFont(PDFFontFace.TimesRoman, fontSize);
                }
                else if (fName != null && fName.Equals("Courier"))
                {
                    font = new PDFFont(PDFFontFace.Courier, fontSize);
                }
                else if (fName != null && fName.Equals("Symbol"))
                {
                    font = new PDFFont(PDFFontFace.Symbol, fontSize);
                }
                else if (fName != null && fName.Equals("ZapfDingbats"))
                {
                    font = new PDFFont(PDFFontFace.ZapfDingbats, fontSize);
                }
                else
                {
                    font = new PDFFont(PDFFontFace.Helvetica, fontSize);
                }

                if (fontFace != null && fontFace.Equals("Bold"))
                {
                    font.Bold = true;
                }

                if (fontFace != null && fontFace.Equals("Italic"))
                {
                    font.Italic = true;
                }
            }

            return font;
        }
    }
}

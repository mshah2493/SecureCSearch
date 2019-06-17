using O2S.Components.PDF4NET.Graphics;
using O2S.Components.PDF4NET.Graphics.Fonts;
using System;

namespace securePDFmerging
{
    [Serializable]
    public class Words
    {
        public Words(string word = "")
        {
            Word = word;
            WordComplete = false;
            Added = false;
        }

        public string Word { get; set; }

        public double Top { get; set; }

        public double Left { get; set; }

        public bool WordComplete { get; set; }

        public bool Added { get; set; }

        public PDFFontBase FontBase { get; set; }

        public PDFBrush FontBrush { get; set; }
    }
}

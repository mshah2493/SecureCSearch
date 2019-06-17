using O2S.Components.PDF4NET.Graphics;
using O2S.Components.PDF4NET.Graphics.Fonts;
using System.Collections.Generic;

namespace securePDFmerging.Models
{
    class Blocks
    {
        public Blocks(string block = "")
        {
            Block = block;
            BlockComplete = false;
            Chunks = new List<int>();
            Top = new List<double>();
            Left = new List<double>();
        }

        public string Block { get; set; }

        public List<double> Top { get; set; }

        public List<double> Left { get; set; }

        public bool BlockComplete { get; set; }

        public double TopOfLastCharacter { get; set; }

        public List<int> Chunks { get; set; }

        public PDFFontBase FontBase { get; set; }

        public PDFBrush FontBrush { get; set; }
    }
}

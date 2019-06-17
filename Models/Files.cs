using O2S.Components.PDF4NET;
using System.IO;

namespace securePDFmerging.Models
{
    class Files
    {
        public PDFDocument PlanTextPDF { get; set; }

        public PDFDocument EncryptedPDF { get; set; }

        public PDFDocument DecryptedPDF { get; set; }

        public FileStream EncryptedFile { get; set; }
    }
}

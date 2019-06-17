using O2S.Components.PDF4NET;
using securePDFmerging.Models;
using System.Collections.Generic;
using System.IO;

namespace securePDFmerging
{
    class FilesRetriever
    {
        private PDFDocument _plainPDF;
        private string _filePath = Path.GetFullPath("C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/bin/Release/UploadedFiles/OriginalFiles/Input.pdf");
        private string _encryptedFilesPath = Path.GetFullPath("C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/bin/Release/UploadedFiles/EncryptedFiles");

        public FilesRetriever()
        {
            CreatePlainTextPDF();
        }

        /**
         * Creates an object of the original PDF.
         * 
         */ 
        private void CreatePlainTextPDF()
        {
            PDFDocumentLoadOptions Op = new PDFDocumentLoadOptions(true);
            _plainPDF = new PDFDocument(_filePath, Op);
        }

        /**
         * Returns the original file
         * 
         */ 
        public Files GetOriginalFiles()
        {
            return new Files()
            {
                PlanTextPDF = _plainPDF,
            };
        }

        /**
         * Returns objects of encrypted shares.
         * 
         */ 
        public Dictionary<string, List<PDFDocument>> RetrieveEncryptedFiles()
        {
            string[] encryptedFilesPath = Directory.GetFiles(_encryptedFilesPath);
            Dictionary<string, List<PDFDocument>> encryptedDocuments 
                = new Dictionary<string, List<PDFDocument>>();

            for (int i = 0; i < encryptedFilesPath.Length; i++)
            {
                string filePath = encryptedFilesPath[i];
                string key = filePath.Substring(0, filePath.Length - 5);
                PDFDocument document = new PDFDocument(filePath);

                if (encryptedDocuments.ContainsKey(key))
                {
                    encryptedDocuments[key].Add(document);
                }
                else
                {
                    encryptedDocuments.Add(key, new List<PDFDocument>()
                    {
                        document
                    });
                } 
            }

            return encryptedDocuments;
        }
    }
}

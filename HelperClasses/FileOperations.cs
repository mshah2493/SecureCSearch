using O2S.Components.PDF4NET;
using O2S.Components.PDF4NET.PageObjects;
using O2S.Components.PDF4NET.PDFFile;
using securePDFmerging.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace securePDFmerging
{
    class FileOperations
    {
        private string _encryptedFilePath = Path.GetFullPath("C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/bin/Release/UploadedFiles/Encrypted_Letter.bin");
        private static List<PDFImagePageObject> _imageObjects = new List<PDFImagePageObject>();
        private static List<PDFPathPageObject> _pathObjects = new List<PDFPathPageObject>();


        /**
         * Reads input file to get PDFObjects from the original PDF
         * 
         */ 
        public Dictionary<object, object> ReadinputFile(PDFDocument pdfDoc)
        {
            PDFImportedPage ip;

            Dictionary<Object, Object> PlainDictionary = new Dictionary<Object, Object>();

            PDFObjectsProcessor objectsProcessor = new PDFObjectsProcessor();

            for (int PageIndex = 0; PageIndex < pdfDoc.Pages.Count; PageIndex++)
            {
                ip = (PDFImportedPage) pdfDoc.Pages[PageIndex];

                PDFPageObjectCollection Objects = ip.ExtractPageObjects();

                //SHA256 mySHA256 = SHA256Managed.Create();
                //string content = ip.ExtractText();
                //byte[] hashedContentKey = mySHA256.ComputeHash(GenerateStreamFromString(content));

                List<Words> WordObjects = new List<Words>();
                List<Blocks> BlockList = new List<Blocks>();

                for (int j = 0; j < Objects.Count; j++)
                {
                    if (Objects[j] is PDFTextPageObject)
                    {
                        PDFTextPageObject TextObject = (PDFTextPageObject) Objects[j];

                        WordObjects = objectsProcessor.GetWords(TextObject, WordObjects);
                        //BlockList = objectsProcessor.GetBlocks(TextObject, BlockList);
                    }
                    else if (Objects[j] is PDFImagePageObject)
                    {
                        _imageObjects.Add((PDFImagePageObject)Objects[j]);
                    }
                    else if (Objects[j] is PDFPathPageObject)
                    {
                        _pathObjects.Add((PDFPathPageObject)Objects[j]);
                    }
                }

                //PlainDictionary.Add(PageIndex, BlockList);
                PlainDictionary.Add(PageIndex, WordObjects);
            }

            pdfDoc.Dispose();
            return PlainDictionary;
        }
    }
}

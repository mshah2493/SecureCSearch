using O2S.Components.PDF4NET;
using System.IO;
using System.Collections.Generic;
using O2S.Components.PDF4NET.PageObjects;
using O2S.Components.PDF4NET.PDFFile;
using securePDFmergings;
using O2S.Components.PDF4NET.Graphics.Fonts;
using O2S.Components.PDF4NET.Graphics;

namespace securePDFmerging
{
    class ShareAssemblerController
    {
        private int _numOfShares;
        private int _shareforRec;
        private string _keyword;
        private List<int> _shareNumbers;
        private string _encryptedFilePath = Path.GetFullPath("C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/bin/Release/UploadedFiles/EncryptedFiles/Encrypted_LetterShare");
        private string _decryptedFilePath = Path.GetFullPath("C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/bin/Release/UploadedFiles/DecryptedFiles/Decrypted_Letter.pdf");
        private PDFDocument _decryptedPDF;
        private List<PDFDocument> _pdfShares = new List<PDFDocument>();
        private List<PDFDocument> _encryptedPdf = new List<PDFDocument>();


        public ShareAssemblerController(int numOfShares, int shareforRec, string keyword)
        {
            _numOfShares = numOfShares;
            _shareforRec = shareforRec;
            _keyword = keyword;

            RetrieveShares();
            RetrieveEncryptedPdf();
            CreateDecryptedPDF();
        }

        /**
         * Recontructs the encrypted shares of the PDF to get original PDF.
         *  Hightlights every occurrences of the word that a user has searched for.
         * 
         */
        public void SharesReconstructor()
        {
            List<List<PDFPageObjectCollection>> pDFPageObjects = ExtractObjects();
            List<PDFPageObjectCollection> ObjectsPerPage;
            PDFBrush redBrush = new PDFBrush(new PDFRgbColor(204, 255, 51));
            PDFPen pen = new PDFPen(new PDFRgbColor(204, 255, 51), 1);

            string[] shares;

            for(int pageIndex = 0; pageIndex < pDFPageObjects.Count; pageIndex++)
            {
                _decryptedPDF.AddPage();
                ObjectsPerPage = pDFPageObjects[pageIndex];

                for (int wordIndex = 0; wordIndex < ObjectsPerPage[0].Count; wordIndex++)
                {
                    shares = new string[_shareforRec];
                    PDFTextPageObject TextObject = null;
                    FontsProcessor fontsProcessor = null;
                    bool shouldProcess = false;

                    for (int shareIndex = 0; shareIndex < _shareforRec; shareIndex++)
                    {
                        PDFPageObjectCollection collection = ObjectsPerPage[shareIndex];
                        
                        if (collection[wordIndex] is PDFTextPageObject)
                        {
                            TextObject = (PDFTextPageObject) collection[wordIndex];

                            if (TextObject.Text == "PDF4NET evaluation version 5.0.1.0")
                            {
                                shouldProcess = false;
                                break;
                            }

                            shares[shareIndex] = TextObject.Text;
                            fontsProcessor = new FontsProcessor();
                            shouldProcess = true;
                        }
                    }

                    if (shouldProcess)
                    {
                        string decryptedText = ShareAssembler.TextReconstruction(shares, _shareNumbers, _numOfShares, _shareforRec);

                        PDFBrush brush = new PDFBrush(TextObject.FillColor);
                        PDFFontBase FontBase = fontsProcessor.GetFontBase(TextObject.FontName, TextObject.FontSize);
                        
                        if (decryptedText.ToLower().Equals(_keyword.ToLower()))
                        {
                            _decryptedPDF.Pages[pageIndex].Canvas.DrawRectangle(pen, redBrush, TextObject.DisplayBounds.Left, TextObject.DisplayBounds.Top, 4 * decryptedText.Length, FontBase.Size, 0);

                            for(int i = 0; i < _encryptedPdf.Count; i++)
                            {
                                _encryptedPdf[i].Pages[pageIndex].Canvas.DrawRectangle(pen, redBrush, TextObject.DisplayBounds.Left, TextObject.DisplayBounds.Top, 4 * decryptedText.Length, FontBase.Size, 0);
                                _encryptedPdf[i].Pages[pageIndex].Canvas.DrawText(shares[i > shares.Length - 1 ? 0 : i], FontBase, brush, TextObject.DisplayBounds.Left, TextObject.DisplayBounds.Top);
                            }
                        }

                        _decryptedPDF.Pages[pageIndex].Canvas.DrawText(decryptedText, FontBase, brush, TextObject.DisplayBounds.Left, TextObject.DisplayBounds.Top);
                    }
                }
            }

            for (int i = 0; i < _encryptedPdf.Count; i++)
            {
                _encryptedPdf[i].Save(_encryptedFilePath + (i + 1) + ".pdf");
            }

            _decryptedPDF.Save(_decryptedFilePath);
        }

        /**
         * Creates an object of PDFDocument.
         * Initializes the layout of the PDF based on the encrypted PDF.
         * 
         */ 
        private void CreateDecryptedPDF()
        {
            _decryptedPDF = new PDFDocument();
            _decryptedPDF.DisplayMode = _pdfShares[0].DisplayMode;
            _decryptedPDF.DocumentInformation = _pdfShares[0].DocumentInformation;
            _decryptedPDF.PageLayout = _pdfShares[0].PageLayout;
            _decryptedPDF.PageOrientation = _pdfShares[0].PageOrientation;
            _decryptedPDF.PortfolioInformation = _pdfShares[0].PortfolioInformation;
            _decryptedPDF.PageLayout = _pdfShares[0].PageLayout;
            _decryptedPDF.PageSize = _pdfShares[0].PageSize;
            _decryptedPDF.PageWidth = _pdfShares[0].PageWidth;
            _decryptedPDF.PageHeight = _pdfShares[0].PageHeight;
        }


        /**
         * Helper method to get an array of ShareNumbers.
         * ShareNumber will have integer values equals to number of shares required to decrypt PDF.
         * 
         */ 
        private void RetrieveShares()
        {
            _shareNumbers = new List<int>();

            for (int i = 0; i < _shareforRec; i++)
            {
                PDFDocumentLoadOptions Op = new PDFDocumentLoadOptions(true);
                _pdfShares.Add(new PDFDocument(_encryptedFilePath + (i + 1) + ".pdf", Op));

                _shareNumbers.Add(i);
            }
        }

        /**
         *  Retrieve encrypted pdf shares  
         */
        private void RetrieveEncryptedPdf()
        {
            _encryptedPdf = new List<PDFDocument>();

            for (int i = 0; i < _numOfShares; i++)
            {
                PDFDocumentLoadOptions Op = new PDFDocumentLoadOptions(true);
                _encryptedPdf.Add(new PDFDocument(_encryptedFilePath + (i + 1) + ".pdf", Op));
            }
        }

        /**
         * Extracts PDFObjects from the encrypted PDF shares
         * 
         */
        private List<List<PDFPageObjectCollection>> ExtractObjects()
        {
            List<List<PDFPageObjectCollection>> listOfObjectCollection = new List<List<PDFPageObjectCollection>>();
            List<PDFPageObjectCollection> ObjectsPerPage;

            for(int PageIndex = 0; PageIndex < _pdfShares[0].Pages.Count; PageIndex++)
            {
                ObjectsPerPage = new List<PDFPageObjectCollection>();

                for(int i = 0; i < _pdfShares.Count; i++)
                {
                    ObjectsPerPage.Add(((PDFImportedPage)_pdfShares[i].Pages[PageIndex]).ExtractPageObjects());
                }

                listOfObjectCollection.Add(ObjectsPerPage);
            }

            return listOfObjectCollection;
        }
    }
}

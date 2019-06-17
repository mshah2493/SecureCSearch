using O2S.Components.PDF4NET;
using securePDFmerging.HelperClasses;
using System;
using System.Collections.Generic;
using System.IO;

namespace securePDFmerging
{
    class ShareGeneratorController
    {
        private const int _textModulus = 127;
        private Random _rand = new Random();
        private Dictionary<Object, Object> _words;
        private PDFDocument _plainPDF;
        private string _encryptedFilePath = Path.GetFullPath("C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/bin/Release/UploadedFiles/EncryptedFiles/Encrypted_LetterShare");
        private int _numOfShares ;
        private int _shareforRec;
        private List<int> _coefficients;
        SortedDictionary<int, int> _avgFrequencyMap;

        public ShareGeneratorController(int numOfShares, int ShareforRec, List<int> coefficients, SortedDictionary<int, int> avgFrequencyMap)
        {
            _numOfShares = numOfShares;
            _shareforRec = ShareforRec;
            _coefficients = coefficients;
            _avgFrequencyMap = avgFrequencyMap;

            _plainPDF = new FilesRetriever().GetOriginalFiles().PlanTextPDF;
            _words = new FileOperations().ReadinputFile(_plainPDF);
        }

        /**
         * Creates PDFDocuments to create shares, encrypt the original PDF and draws texts on the PDF shares
         * 
         */ 
        public void createShares()
        {
            List<PDFDocument> pdfShares = new List<PDFDocument>();
            Dictionary<int, string> coefShares = new Dictionary<int, string>();

            for (int i = 0; i < _numOfShares; i++)
            {
                pdfShares.Add(new PDFDocument());
                pdfShares[i].DisplayMode = _plainPDF.DisplayMode;
                pdfShares[i].DocumentInformation = _plainPDF.DocumentInformation;
                pdfShares[i].PageLayout = _plainPDF.PageLayout;
                pdfShares[i].PageOrientation = _plainPDF.PageOrientation;
                pdfShares[i].PortfolioInformation = _plainPDF.PortfolioInformation;
                pdfShares[i].PageLayout = _plainPDF.PageLayout;
                pdfShares[i].PageSize = _plainPDF.PageSize;
                pdfShares[i].PageWidth = _plainPDF.PageWidth;
                pdfShares[i].PageHeight = _plainPDF.PageHeight;
            }

            foreach (KeyValuePair<Object, Object> entry in _words)
            {
                if (entry.Key.GetType() != typeof(string))
                {
                    List<Words> Words = (List<Words>) entry.Value;
                    int pageIndex = (int) entry.Key;

                    //add next page for future processing
                    for (int l = 0; l < pdfShares.Count; l++)
                    {
                        pdfShares[l].AddPage();
                        pdfShares[l].Pages[pageIndex].Document.DocumentInformation.Keywords = "";
                    }

                    for (int i = 0; i < Words.Count; i++)
                    {
                        Words word = Words[i];
                        string secret = word.Word;

                        int[] coefficients = new int[_shareforRec - 1];
                        // string str = "";

                        if (_shareforRec == 2)
                        {
                            coefficients[0] = _coefficients[0];
                        }
                        else
                        {
                            //Generate the random values and make sure our coefficients are less than the modulus value
                            for (int index = 0; index < coefficients.Length; index++)
                            {
                                int coefIndex = _rand.Next(1, _coefficients.Count) - 1;
                                coefficients[index] = _coefficients[coefIndex];
                            }
                        }
                                
                        Share[] textShares = ShareGenerator.GenerateShares(secret, _numOfShares, _shareforRec, coefficients, _avgFrequencyMap);
                        

                        for (int l = 0; l < textShares.Length; l++)
                        {
                            pdfShares[l].Pages[pageIndex].Canvas.DrawText(textShares[l].GetCipherText(), word.FontBase, word.FontBrush, word.Left, word.Top);

                            if (secret == "because")
                            {
                                //pdfShares[l].Pages[pageIndex].Canvas.DrawText(textShares[i].ToString().Length, word.FontBase, word.FontBrush, word.Left, word.Top);
                            }
                        }
                    }
                }
            }

            for(int i = 0; i < _coefficients.Count; i++)
            {
                int[] coefficientShares = new CoefficientCodec(_coefficients[i], _numOfShares, _shareforRec, "Encrypt").shares;

                for (int shareIndex = 0; shareIndex < coefficientShares.Length; shareIndex++)
                {
                    if (!coefShares.ContainsKey(shareIndex))
                    {
                        coefShares.Add(shareIndex, coefficientShares[shareIndex].ToString() + ",");
                    }
                    else
                    {
                        coefShares[shareIndex] += coefficientShares[shareIndex].ToString() + ",";
                    }
                }
            }

            for (int i = 0; i < pdfShares.Count; i++)
            {
                pdfShares[i].DocumentInformation.Keywords = coefShares[i];
                pdfShares[i].Save(_encryptedFilePath + (i + 1) + ".pdf");
                pdfShares[i].Dispose();
            }
        }
    }
}

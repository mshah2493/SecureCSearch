using O2S.Components.PDF4NET;
using O2S.Components.PDF4NET.PDFFile;
using securePDFmerging.HelperClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Accord.Math;
using O2S.Components.PDF4NET.PageObjects;

namespace securePDFmerging
{
    class SearchController
    {
        private int _numOfShares;
        private int _shareforRec;
        private const int _textModulus = 127;
        private string _keyword;
       
        Dictionary<string, List<PDFDocument>> _encryptedDocuments;
            
        public SearchController(string keyword, int numOfShares, int shareforRec)
        {
            _keyword = keyword;
            _numOfShares = numOfShares;
            _shareforRec = shareforRec;

            _encryptedDocuments = new FilesRetriever().RetrieveEncryptedFiles();
        }


        /**
         * Iterates through encrypted documents and encrypted keywords to check if provided keyword exists or not
         * 
         * 
        */
        public void SearchKeyWord()
        {
            List<PDFDocument> documentsList;

            foreach(KeyValuePair<string, List<PDFDocument>> document in _encryptedDocuments)
            {
                Stopwatch watch = new Stopwatch();

                watch.Start();

                documentsList = document.Value;

                List<string[]> coefficientList = RetrieveCoefficients(documentsList);

                List<int> decryptedCoef = new CoefficientCodec(0, _numOfShares, _shareforRec, "Decrypt", coefficientList).coefficients;

                List<int> CoefNeededToSearch = PerformCombinationOfCoef(decryptedCoef, _shareforRec - 1);

                PDFDocument docShare = documentsList[0];

                bool keywordFound = false;

                Console.WriteLine("CoefNeededToSearch : " +CoefNeededToSearch.Count);

                for (int coefIndex = 0; coefIndex < CoefNeededToSearch.Count; coefIndex++)
                {
                    Share[] share = ShareGenerator.GenerateShares(_keyword, _numOfShares, _shareforRec, new int[] { CoefNeededToSearch[coefIndex] }, new SortedDictionary<int, int>());

                    for (int pageIndex = 0; pageIndex < docShare.Pages.Count; pageIndex++)
                    {
                        PDFPage page = docShare.Pages[pageIndex];
                        string text = ((PDFImportedPage)page).ExtractText();

                        PDFImportedPage ip = (PDFImportedPage)docShare.Pages[pageIndex];

                        PDFPageObjectCollection Objects = ip.ExtractPageObjects();

                        for (int j = 0; j < Objects.Count; j++)
                        {
                            if (Objects[j] is PDFTextPageObject)
                            {
                                PDFTextPageObject TextObject = (PDFTextPageObject)Objects[j];


                                if (TextObject.Text == share[0].GetCipherText())
                                {
                                    keywordFound = true;    
                                }
                            }
                        }

                        
                    }
                }

                if (keywordFound)
                {
                    watch.Stop();

                    Console.WriteLine("Search Time : " + watch.ElapsedMilliseconds);

                    watch.Reset();

                    watch.Start();

                    ShareAssemblerController shareAssembler = new ShareAssemblerController(_numOfShares, _shareforRec, _keyword);
                    shareAssembler.SharesReconstructor();
                    keywordFound = true;

                    watch.Stop();

                    Console.WriteLine("Decrypt and Highlight Time : " + watch.ElapsedMilliseconds);
                }
                else
                {
                    Console.WriteLine("NO FILES CONTAIN THE KEYWORD YOU'RE LOOKING FOR.");
                }
            }
        }

        /**
         * Fetch shares of encrypted coefficients from the metadata of encrypted PDF Shares.
         * 
        */
        private List<string[]> RetrieveCoefficients(List<PDFDocument> documentList)
        {
            List<string[]> coefficientList = new List<string[]>(documentList.Count);
            
            string[] coefficients1 = documentList[0].DocumentInformation.Keywords.Split(',');
            string[] coefficients2 = documentList[1].DocumentInformation.Keywords.Split(',');
            string[] coefficients3 = documentList[2].DocumentInformation.Keywords.Split(',');

            string[] coef = null;
            int coefShareLength = documentList.Count;
            //int j = 0;

            for (int i = 0; i < coefficients1.Length - 1; i++)
            {
                //if (i == 0 || i % coefShareLength == 0)
                //{
                //    if (i != 0)
                //    {
                //        coefficientList.Add(coef);
                //    }

                //    j = 0;
                //    coef = new string[coefShareLength];
                //}
                coef = new string[coefShareLength];
                coef[0] = coefficients1[i];
                coef[1] = coefficients2[i];
                coef[2] = coefficients3[i];
                coefficientList.Add(coef);
                //j++;
            }

            return coefficientList;
        }


        public List<int> PerformCombinationOfCoef(List<int> coef, int r)
        {
            //    double count = Math.Pow(2, coef.Count);
                List<int> result = new List<int>();
            //    List<int> temp;

            //    for (int i = 1; i <= count - 1; i++)
            //    {
            //        temp = new List<int>();

            //        string str = Convert.ToString(i, 2).PadLeft(coef.Count, '0');

            //        for (int j = 0; j < str.Length; j++)
            //        {
            //            if (str[j] == '1')
            //            {
            //                temp.Add(coef[j]);
            //            }
            //        }

            //        if (temp.Count == r && temp.Count > 0)
            //        {
            //            result.Add(temp[0]);
            //        }
            //    }

            //    return result;

            foreach (int[] combination in Combinatorics.Combinations(coef.ToArray(), k: r))
            {
                result.Add(combination[0]);
            }

            return result;
        }
    }
}
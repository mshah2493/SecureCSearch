using O2S.Components.PDF4NET;
using O2S.Components.PDF4NET.PDFFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace securePDFmerging.Controller
{
    class ICController
    {
        Dictionary<string, List<PDFDocument>> _encryptedDocuments;
        SortedDictionary<int, int> _avgFrequenceMap;
        int _maxNumOfOccurrences;
        StreamWriter _writeText;
        SortedDictionary<string, double> _icMap;

        public ICController(int maxNumOfOccurrences, StreamWriter writeText, SortedDictionary<int, int> avgFrequencyMap, SortedDictionary<string, double> icMap)
        {
            _maxNumOfOccurrences = maxNumOfOccurrences;
            _encryptedDocuments = new FilesRetriever().RetrieveEncryptedFiles();
            _writeText = writeText;
            _avgFrequenceMap = avgFrequencyMap;
            _icMap = icMap;

            CalculateIC();
        }

        private void CalculateIC()
        {
            foreach (var doc in _encryptedDocuments)
            {
                var docList = doc.Value;

                for (int i = 0; i < docList.Count; i++)
                {
                    Dictionary<char, int> frequencyMap = new Dictionary<char, int>();

                    int textLength = 0;

                    for (int pageIndex = 0; pageIndex < docList[i].Pages.Count; pageIndex++)
                    {
                        PDFPage page = docList[i].Pages[pageIndex];

                        string text = (((PDFImportedPage)page).ExtractText());

                        textLength += text.Length;

                        for (int j = 0; j < text.Length; j++)
                        {
                            if (frequencyMap.ContainsKey(text[j]))
                            {
                                frequencyMap[text[j]]++;
                            }
                            else
                            {
                                frequencyMap[text[j]] = 1;
                            }
                        }
                    }

                    int ch;
                    double sum = 0;

                    foreach (var charFreq in frequencyMap)
                    {
                        ch = charFreq.Value;
                        sum += (ch * (ch - 1));
                    }

                    double total = sum / (textLength * (textLength - 1));

                    if (_icMap.ContainsKey("r = " + _maxNumOfOccurrences + " IC of share " + i))
                    {
                        _icMap["r = "+ _maxNumOfOccurrences + " IC of share " + i] += total;
                    }
                    else
                    {
                        _icMap["r = " + _maxNumOfOccurrences + " IC of share " + i] = total;
                    }

                    //Console.WriteLine(" IC of share " + i + " : " + total);
                }
            }
        }
    }
}

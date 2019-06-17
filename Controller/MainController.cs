using securePDFmerging.Controller;
using securePDFmerging.HelperClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace securePDFmerging
{
    static class MainController
    {
        static void Main()
        {
            const int numOfShares = 3;
            const int shareforRec = 2;

            string keyword = "Software";

            using (StreamWriter writetext = new StreamWriter("C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/frequency.txt"))
            {
                SortedDictionary<string, double> icMap = new SortedDictionary<string, double>();

                for (int i = 0; i < 1; i++)
                {
                    SortedDictionary<int, int> avgFrequencyMap = new SortedDictionary<int, int>();

                    int maxNumOfOccurrences = i + 1;
                    int numberofCoefNeeded = (shareforRec - 1) * maxNumOfOccurrences;

                    writetext.WriteLine("------------------------------------------------");
                    writetext.WriteLine("r : " + maxNumOfOccurrences);
                    writetext.WriteLine("------------------------------------------------");

                    for (int j = 0; j < 1; j++)
                    {
                        CoefficientCodec coef = new CoefficientCodec(numberofCoefNeeded);

                        Console.WriteLine("numberofCoefNeeded : " + numberofCoefNeeded);

                        Stopwatch watch = new Stopwatch();

                        watch.Start();
                        ShareGeneratorController shareGenerator = new ShareGeneratorController(numOfShares, shareforRec, coef.Coef, avgFrequencyMap);
                        shareGenerator.createShares();
                        watch.Stop();

                        Console.WriteLine("Time to Encrypt: " + watch.ElapsedMilliseconds);

                        watch.Reset();

                        ICController icController = new ICController(maxNumOfOccurrences, writetext, avgFrequencyMap, icMap);

                        SearchController searchController = new SearchController(keyword, numOfShares, shareforRec);
                        searchController.SearchKeyWord();

                        // Console.ReadLine();
                    }

                    foreach (var charFreq in avgFrequencyMap)
                    {
                        writetext.WriteLine((int)charFreq.Key + "-----" + (int) (charFreq.Value/10));
                    }
                }

                writetext.WriteLine("------------------------------------------------");
                writetext.WriteLine("---------------------IC Values-------------------");
                writetext.WriteLine("------------------------------------------------");

                foreach (var ic in icMap)
                {
                    writetext.WriteLine(ic.Key + "-----" + (ic.Value / 10));
                }
            }
        }
    }
}

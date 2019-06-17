using System;
using System.Collections.Generic;

namespace securePDFmerging.HelperClasses
{
    class CoefficientCodec
    {
        private int prime = 127;
        private int _secret;
        private int _numOfShare;
        private int _shareforRec;
        private int _numberOfCoef;

        public CoefficientCodec(int numberOfCoef)
        {
            _numberOfCoef = numberOfCoef;
            GenerateCoefficients();
        }

        public CoefficientCodec(int secret, int numOfShare, int shareforRec, string mode, List<string[]> coef = null)
        {
            _secret = secret;
            _numOfShare = numOfShare;
            _shareforRec = shareforRec;

            if (mode == "Encrypt")
            {
                shares = SplitShares(GenerateCooefficients());
            } else
            {
                coefficients = DecryptCoefficients(coef);
            }
        }

        

        public int[] shares { get; set; }

        public List<int> coefficients { get; set; }

        public List<int> Coef { get; set; }

        private void GenerateCoefficients()
        {
            Random rand = new Random();
            List<int> coef = new List<int>();

            for(int i = 0; i < _numberOfCoef; i++)
            {
                coef.Add(rand.Next(prime));
            }

            Coef = coef;
        }

        /**
         * Generates random numbers for to be used as coefficients.
         * Coefficients will always be less than prime numbers.
         * 
         */
        private int[] GenerateCooefficients()
        {
            int[] coeff = new int[_shareforRec];

            for (int j = 0; j < _shareforRec - 1; j++)
            {
                coeff[j] = new Random().Next(1, prime - 1);

                if (coeff[j] == _secret || coeff[j] == 0)
                {
                    coeff[j]++;
                }
            }

            return coeff;
        }

        /**
         * Generates shares of coefficients
         * 
         */ 
        private int[] SplitShares(int[] coeff)
        {
            int[] shares = new int[_numOfShare];

            for (int i = 0; i < _numOfShare; i++)
            {
                int share = _secret;

                for (int j = 0; j < coeff.Length; j++)
                {
                    share = (share + (coeff[j] * (int)Math.Pow(i + 1, j + 1))) % prime;

                    if (share == 0)
                    {
                        share += prime;
                    }
                }
                shares[i] = share;
            }

            return shares;
        }

        /**
         * Takes shares of coefficients as input.
         * Returns original coefficients.
         * 
         */ 
        private List<int> DecryptCoefficients(List<string[]> coefficientList)
        {
            List<int> co = new List<int>();
            string[] coef;

            for (int i = 0; i < coefficientList.Count; i++)
            {
                coef = coefficientList[i];

                List<int[]> list = new List<int[]>();
                list.Add(new int[] { 2, Convert.ToInt32(coef[1]) });
                list.Add(new int[] { 3, Convert.ToInt32(coef[2]) });
                list.Add(new int[] { 1, Convert.ToInt32(coef[0]) });

                //int[,] coefficients = new int[,]{ { 2, Convert.ToInt32(coef[1]) }, 
                //   { 3, Convert.ToInt32(coef[2]) }, { 1, Convert.ToInt32(coef[0]) } };

                co.Add(combineKeyShares(list, prime));
            }

            return co;
            /*
            List<int> coefficients = new List<int>(coefficientList.Count);
            string[] coefShares;
            List<int> shareNum;

            for(int i = 0; i < coefficientList.Count; i++)
            {
                coefShares = new string[_shareforRec];
                shareNum = new List<int>();

                for (int j = 0; j < _shareforRec; j++)
                {
                    coefShares[j] = coefficientList[i][j];
                    shareNum.Add(j);
                }

                string coef = ShareAssembler.TextReconstruction(coefShares, shareNum, _numOfShares, _shareforRec);

                if (coef != "")
                {
                    coefficients.Add((int) coef[0]);
                }
                
            }

            return coefficients; 
            */
        }

        /**
         * Helper method to decrypt coefficients
         * 
         */
        private int combineKeyShares(List<int[]> keyShares, int prime)
        {
            int secret = 0;

            for (int i = 0; i < keyShares.Count; i++)
            {
                int[] currentShare = keyShares[i];

                int numerator = 1;
                int denominator = 1;

                for (int j = 0; j < keyShares.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    int[] tempCurrent = keyShares[j];
                    int negate = -1;

                    numerator = (numerator * (tempCurrent[0] * negate)) % prime;
                    denominator = (denominator * (currentShare[0] - tempCurrent[0])) % prime;
                }

                secret += prime + (currentShare[1] * numerator * modInverse(denominator, prime));
            }

            return secret % prime;
        }

        /**
         * Performs modulo inverse
         * 
         */ 
        private int modInverse(int num, int prime)
        {
            if (num == -1)
            {
                return prime - 1;
            }

            BigInteger n = new BigInteger(num);
            BigInteger p = new BigInteger(prime);
            BigInteger result = n.modInverse(p);

            return result.IntValue();
        }
    }
}

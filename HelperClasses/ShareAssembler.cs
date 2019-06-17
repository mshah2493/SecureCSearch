using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using securePDFmerging;

namespace securePDFmergings
{
    public class ShareAssembler
    {
        /**
         * Private fields for Share Generation
         * TEXT_MOUDULUS: The number that will be used in mod arithmitic for text shares.
         * IMAGE_MODULUS: The number that will be used in mod arithmitic for image shares.
         * CHAR_OFFSET: Since some char values that will be produced by shares are unreadable in pdf files
         * We offset them in order to find a field of numbers in which all 127 characters can
         * be read again by the pdf files in file reconstruction.
        */
        private const int TEXT_MODULUS = 127;
        private const int IMAGE_MODULUS = 251;
        private const int CHAR_OFFSET = 33; 
        
        /**
         * A method for taking an array of share values and using them to reconstruct their secret based
         * on Shamirs (n,k) thereshold.
         * <param name="shares">The array of shares used to generate the original secret</param>
         * <param name="sharesToUse">The list of shares that we wish to use from our array.</param>
         * <param name="n">The number of shares created in the generation process.</param>
         * <param name="k">The number of shares required to reconstruct the secret.</param>
         * <returns></returns>
         */
        public static string TextReconstruction(string[] shares, List<int> shareNumbers, int n, int k)
        {
            if (shares[1] == "") return "";

            ShareGenerator.intializeReverseMap();

            //A dictionary datatype used to store sharenumbers and share values
            Dictionary<int, int> sharesUsed = new Dictionary<int, int>();

            //String that is being reconstructed
            StringBuilder reconstruction = new StringBuilder();  

            //Stores the integer secret value we reconstruct each itteration
            int secret = 0; 

            if (shareNumbers.Count < k)
            {
                return "To few shares used in reconstruction! Use " + k + " shares to reconstruct secret. Reconstruction failed.";
            }
            else if (shareNumbers.Count == k) //If the count in ShareNumbers matches k exactly, use all of the strings in shares[]
            {
                //loop through each character in the shares and peform the following:
                //we use shares[0].length here since each share value is the same length so it 
                //is an arbitrary choice.
                for (int i = 0; i < shares[0].Length; i++)
                {
                    //For each item in shareNumbers, use that value as the 'key' in our dictionary
                    //and then use that value as the index in our shares array to get the value
                    for (int j = 0; j < shareNumbers.Count; j++)
                    {
                        int a = (int)(shares[shareNumbers[j]][i]);
                        if (ShareGenerator.REVERSE.ContainsKey(a))
                        {
                            a = ShareGenerator.REVERSE[a];
                        }
                        else
                        {
                            Console.WriteLine(a);
                        }

                        sharesUsed.Add(shareNumbers[j] + 1, a - CHAR_OFFSET);
                    }

                    //Call lagrange polynomial on our dictionary to reconstruct secret                    
                    secret = LagrangePolynomial(sharesUsed, false);
                    
                    reconstruction.Append((char) secret); //add our secret value to our string reconstruction
                    
                    sharesUsed.Clear(); //Clear our dictionary object
                }
            }
            else
            {
                return "To many shares selected to be used. Select " + k + " shares to reconstruct secret.  Reconstruction failed.";
            }

            return reconstruction.ToString();
        }

        public static Bitmap ImageReconstruction(Bitmap[] shares, List<int> shareNumbers, int n, int k)
        {

            Dictionary<int, int> redShare = new Dictionary<int, int>();
            Dictionary<int, int> blueShare = new Dictionary<int, int>();
            Dictionary<int, int> greenShare = new Dictionary<int, int>();
            Bitmap reconstruction = new Bitmap(shares[0]); //Create our reconstructed image object, arbitratily pick one of the shares to use for sizing purposes
            Color reconColor;   //Our reconstruction pixel color
            int red, green, blue; //Reconstructed color components of a pixel

            //make sure the list of shares to use is the appropriate size
            if (shareNumbers.Count < k)
            {
                System.Console.WriteLine("To few shares used in reconstruction! Use " + k + " shares to reconstruct secret. Reconstruction failed.");
            }

            //Loop through each x pixel in the reconstruction
            for (int x = 0; x < reconstruction.Width; x++)
            {
                //loop through each y pixel in the reconstruction
                for (int y = 0; y < reconstruction.Height; y++)
                {
                    //For each item in shareNumbers, use that value as the 'key' in our dictionary
                    //and then use that value as the index in our shares array to get the value
                    for (int i = 0; i < shareNumbers.Count; i++)
                    {
                        redShare.Add(shareNumbers[i] + 1, shares[shareNumbers[i]].GetPixel(x, y).R);
                        greenShare.Add(shareNumbers[i] + 1, shares[shareNumbers[i]].GetPixel(x, y).G);
                        blueShare.Add(shareNumbers[i] + 1, shares[shareNumbers[i]].GetPixel(x, y).B);
                    }

                    //Reconstruct inndividual color components
                    red = LagrangePolynomial(redShare, true);
                    green = LagrangePolynomial(greenShare, true);
                    blue = LagrangePolynomial(blueShare, true);

                    reconColor = Color.FromArgb(red, green, blue);
                    reconstruction.SetPixel(x, y, reconColor);

                    //Clears out our color component dictionaries
                    redShare.Clear();
                    greenShare.Clear();
                    blueShare.Clear();

                }
            }

            return reconstruction;
        }

        public static Bitmap ImageReconstructionReduced(Bitmap[] shares, List<int> shareNumbers, int n, int k)
        {
            Bitmap reconImage = new Bitmap(shares[0].Width, shares[0].Height, shares[0].PixelFormat);
            int batchCount;
            int currPixelXVal;
            Color runColor = new Color();

            Dictionary<int, int> redShare = new Dictionary<int, int>();
            Dictionary<int, int> blueShare = new Dictionary<int, int>();
            Dictionary<int, int> greenShare = new Dictionary<int, int>();

            Color reconColor;   //Our reconstruction pixel color
            int reconXPos;
            int red, green, blue; //Reconstructed color components of a pixel

            for (int y = 0; y < shares[0].Height; y++)
            {
                reconXPos = 0;
                //determine how many pixels we actually need to process in the current row
                int numPixelsUsed = 0;  //number of pixels to process in this row

                for (int i = 0; i < shares[0].Width; i++)
                {
                    if (shares[0].GetPixel(i, y).R == 0 
                        && shares[0].GetPixel(i, y).G == 0 
                        && shares[0].GetPixel(i, y).B == 0)
                    {
                        break;
                    }
                    else
                    {
                        numPixelsUsed++;
                    }
                }

                for (int shareXPos = 0; shareXPos < numPixelsUsed;)
                {
                    //determine how many pixels we are able to process
                    if (numPixelsUsed - shareXPos >= 4)
                    {
                        batchCount = 3;
                    }
                    else if (numPixelsUsed - shareXPos >= 3)
                    {
                        batchCount = 2;
                    }
                    else
                    {
                        batchCount = 1;
                    }

                    if ((shareXPos + batchCount) < shares[0].Width)
                    {
                        //the pixel that contains 'count' information of the preceeding colors
                        runColor = shares[0].GetPixel(shareXPos + batchCount, y);   
                    }
                        
                    //loop through the pixels in the batch
                    for (int j = 0; j < batchCount; j++)
                    {
                        currPixelXVal = shareXPos + j;

                        //For each item in shareNumbers, use that value as the 'key' in our dictionary
                        //and then use that value as the index in our shares array to get the value
                        for (int i = 0; i < shareNumbers.Count; i++)
                        {
                            redShare.Add(shareNumbers[i] + 1, shares[shareNumbers[i]].GetPixel(currPixelXVal, y).R);
                            greenShare.Add(shareNumbers[i] + 1, shares[shareNumbers[i]].GetPixel(currPixelXVal, y).G);
                            blueShare.Add(shareNumbers[i] + 1, shares[shareNumbers[i]].GetPixel(currPixelXVal, y).B);
                        }

                        //Reconstruct individual color components
                        red = LagrangePolynomial(redShare, true);
                        green = LagrangePolynomial(greenShare, true);
                        blue = LagrangePolynomial(blueShare, true);

                        reconColor = Color.FromArgb(red, green, blue);
                        
                        //determine which R,G or B value to use in our Run Pixel adn create a 'run' of that many pixels in reconsturction image
                        if (j == 0)
                        {
                            for (int l = 0; l < runColor.R; l++)
                            {
                                if (reconXPos < shares[0].Width)
                                {
                                    reconImage.SetPixel(reconXPos, y, reconColor);
                                }
                                    
                                reconXPos++;
                            }
                        }
                        else if (j == 1)
                        {
                            for (int l = 0; l < runColor.G; l++)
                            {
                                if (reconXPos < shares[0].Width)
                                {
                                    reconImage.SetPixel(reconXPos, y, reconColor);
                                }
                                    
                                reconXPos++;
                            }
                        }
                        else
                        {
                            for (int l = 0; l < runColor.B; l++)
                            {
                                if (reconXPos < shares[0].Width)
                                {
                                    reconImage.SetPixel(reconXPos, y, reconColor);
                                }
                                    
                                reconXPos++;
                            }
                        }

                        //Clears out our color component dictionaries
                        redShare.Clear();
                        greenShare.Clear();
                        blueShare.Clear();
                    }

                    shareXPos += batchCount;

                    //skip over to the next pixel to be the first to be processed in the next batch
                    if ((shareXPos + 1) != reconImage.Width)
                    {
                        shareXPos++;
                    }
                }
            }

            return reconImage;
        }


        /// <summary>
        /// Calculates the lagrange polynomial on a set of share numbers and their value to reconstruct7
        /// an original secret.
        /// </summary>
        /// <param name="sharesUsed">The dictionary used to store share numbers and their values.</param>
        /// <returns>Returns the reconstructed secret integer value.</returns>
        private static int LagrangePolynomial(Dictionary<int, int> sharesUsed, bool isImage)
        {
            Dictionary<int, int>.KeyCollection temp = sharesUsed.Keys;
            List<int> shareNumbers = temp.ToList<int>();

            int result = 0;
            result = sharesUsed[shareNumbers[1]] + ((sharesUsed[shareNumbers[0]] - sharesUsed[shareNumbers[1]]) * shareNumbers[1]);

            if (result < 0)
            {
                if (!isImage)
                {
                    result = result + TEXT_MODULUS;
                }
                else
                {
                    result = result + IMAGE_MODULUS;
                }
            }

            if (!isImage)
            {
                return Mod(result, TEXT_MODULUS);
            }
            else
            {
                return Mod(result, IMAGE_MODULUS);
            }
                
            //int l1;
            //int l2;
            //int l3;
            ////Determine legrange polynomial 1
            //if (!isImage)
            //    l1 = Mod(shareNumbers[1] * shareNumbers[2], TEXT_MODULUS) * MultiplicatveInverse((shareNumbers[0] - shareNumbers[1]) * (shareNumbers[0] - shareNumbers[2]), TEXT_MODULUS);
            //else
            //    l1 = Mod(shareNumbers[1] * shareNumbers[2], IMAGE_MODULUS) * MultiplicatveInverse((shareNumbers[0] - shareNumbers[1]) * (shareNumbers[0] - shareNumbers[2]), IMAGE_MODULUS);

            ////for legrange polynomial 2 there is a chance the constant term is negative, so if it is we must
            ////account for this
            //if (!isImage)
            //{
            //    if ((shareNumbers[1] - shareNumbers[0]) * (shareNumbers[1] - shareNumbers[2]) < 0)
            //    {
            //        int num = Mod(shareNumbers[0] * shareNumbers[2], TEXT_MODULUS) * -1;
            //        int denom = (shareNumbers[1] - shareNumbers[0]) * (shareNumbers[1] - shareNumbers[2]);
            //        denom *= -1;
            //        l2 = num * MultiplicatveInverse(denom, TEXT_MODULUS);
            //    }
            //    //if its not negative do this
            //    else
            //        l2 = Mod(shareNumbers[0] * shareNumbers[2], TEXT_MODULUS) * MultiplicatveInverse((shareNumbers[1] - shareNumbers[0]) * (shareNumbers[1] - shareNumbers[2]), TEXT_MODULUS);
            //}
            //else
            //{
            //    if ((shareNumbers[1] - shareNumbers[0]) * (shareNumbers[1] - shareNumbers[2]) < 0)
            //    {
            //        int num = Mod(shareNumbers[0] * shareNumbers[2], IMAGE_MODULUS) * -1;
            //        int denom = (shareNumbers[1] - shareNumbers[0]) * (shareNumbers[1] - shareNumbers[2]);
            //        denom *= -1;
            //        l2 = num * MultiplicatveInverse(denom, IMAGE_MODULUS);
            //    }
            //    //if its not negative do this
            //    else
            //        l2 = Mod(shareNumbers[0] * shareNumbers[2], IMAGE_MODULUS) * MultiplicatveInverse((shareNumbers[1] - shareNumbers[0]) * (shareNumbers[1] - shareNumbers[2]), IMAGE_MODULUS);
            //}
            ////determine legrange polynomial 3
            //if (!isImage)
            //    l3 = Mod(shareNumbers[0] * shareNumbers[1], TEXT_MODULUS) * MultiplicatveInverse((shareNumbers[2] - shareNumbers[0]) * (shareNumbers[2] - shareNumbers[1]), TEXT_MODULUS);
            //else
            //    l3 = Mod(shareNumbers[0] * shareNumbers[1], IMAGE_MODULUS) * MultiplicatveInverse((shareNumbers[2] - shareNumbers[0]) * (shareNumbers[2] - shareNumbers[1]), IMAGE_MODULUS);

            //int result = 0;

            ////Sum the polynomials
            //result += l1 * sharesUsed[shareNumbers[0]];
            //result += l2 * sharesUsed[shareNumbers[1]];
            //result += l3 * sharesUsed[shareNumbers[2]];

            //if (!isImage)
            //    return Mod(result, TEXT_MODULUS);   //mod the final result
            //else
            //    return Mod(result, IMAGE_MODULUS);
        }

        /// <summary>
        /// A modulus function
        /// </summary>
        /// <param name="x">The x value in the case of x mod n</param>
        /// <param name="n">The n value in the case of x mond n</param>
        /// <returns></returns>
        private static int Mod(int x, int n)
        {
            return (Math.Abs(x * n) + x) % n;
        }

        /// <summary>
        /// A function to determine the Mulitplicative inverse of a number and a prime
        /// </summary>
        /// <param name="a">the number</param>
        /// <param name="mod">the prime number</param>
        /// <returns>The value of the multiplicative inverse</returns>
        private static int MultiplicatveInverse(int a, int mod)
        {
            int dividend = a % mod;
            int divsor = mod;

            int lastX = 1;
            int currX = 0;

            while (divsor > 0)
            {
                int quotient = dividend / divsor;
                int remainder = dividend % divsor;
                if (remainder <= 0)
                {
                    break;
                }
                    
                int nextX = lastX - currX * quotient;
                lastX = currX;
                currX = nextX;

                dividend = divsor;
                divsor = remainder;
            }
            

            if (divsor != 1)
            {
                throw new Exception("Numbers a and mod are not realitive primes!");
            }

            return (currX < 0 ? currX + mod : currX);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace securePDFmerging
{
    public class ShareGenerator
    {
        /*
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
        private static Random random = new Random(System.DateTime.Now.Millisecond);
        private static Bitmap secretImage;

        public static Dictionary<int, int> REVERSE = new Dictionary<int, int>();

        private static int[] EXTENDED = { 0x00C7, 0x00FC, 0x00E9, 0x00E2,
            0x00E4, 0x00E0, 0x00E5, 0x00E7, 0x00EA, 0x00EB, 0x00E8, 0x00EF,
            0x00EE, 0x00EC, 0x00C4, 0x00C5, 0x00C9, 0x00E6, 0x00C6, 0x00F4,
            0x00F6, 0x00F2, 0x00FB, 0x00F9, 0x00FF, 0x00D6, 0x00DC, 0x00A2,
            0x00A3, 0x00A5, 0x20A7, 0x0192, 0x00E1, 0x00ED, 0x00F3, 0x00FA,
            0x00F1, 0x00D1, 0x00AA, 0x00BA, 0x00BF, 0x2310, 0x00AC, 0x00BD,
            0x00BC, 0x00A1, 0x00AB, 0x00BB, 0x2591, 0x2592, 0x2593, 0x2502,
            0x2524, 0x2561, 0x2562, 0x2556, 0x2555, 0x2563, 0x2551, 0x2557,
            0x255D, 0x255C, 0x255B, 0x2510, 0x2514, 0x2534, 0x252C, 0x251C,
            0x2500, 0x253C, 0x255E, 0x255F, 0x255A, 0x2554, 0x2569, 0x2566,
            0x2560, 0x2550, 0x256C, 0x2567, 0x2568, 0x2564, 0x2565, 0x2559,
            0x2558, 0x2552, 0x2553, 0x256B, 0x256A, 0x2518, 0x250C, 0x2588,
            0x2584, 0x258C, 0x2590, 0x2580, 0x03B1, 0x00DF, 0x0393, 0x03C0,
            0x03A3, 0x03C3, 0x00B5, 0x03C4, 0x03A6, 0x0398, 0x03A9, 0x03B4,
            0x221E, 0x03C6, 0x03B5, 0x2229, 0x2261, 0x00B1, 0x2265, 0x2264,
            0x2320, 0x2321, 0x00F7, 0x2248, 0x00B0, 0x2219, 0x00B7, 0x221A,
            0x207F, 0x00B2, 0x25A0, 0x00A0 };

        public static char getExtendedAscii(int code)
        {
            if (code >= 0x80 && code <= 0xFF)
            {
                return (char)EXTENDED[code - 0x7F];
            }

            return (char)code;
        }

        public static void intializeReverseMap()
        {
            for (int i = 33; i <= 159; i++)
            {
                if (i == 157)
                {
                    int ex = getExtendedAscii(161);

                    if (!REVERSE.ContainsKey(ex))
                    {
                        REVERSE.Add(ex, i);
                    }
                }
                else if (i == 127)
                {
                    int ex = getExtendedAscii(160);

                    if (!REVERSE.ContainsKey(ex))
                    {
                        REVERSE.Add(ex, i);
                    }
                }
                else if (i > 126)
                {
                    int ex = getExtendedAscii(i);

                    if (!REVERSE.ContainsKey(ex))
                    {
                        REVERSE.Add(ex, i);
                    }
                }
                else
                {
                    if (!REVERSE.ContainsKey(i))
                    {
                        REVERSE.Add(i, i);
                    }
                }

            }
        }

        /**
         * Generates Shamir Secret shares for a given string of text, using the (n,k) scheme
         * where n shares are generated and k shares are required to reconstruct the secret.
         * <param name="secret">The text secret we wish to share.</param>
         * <param name="n">The number of shares to produce from our secret.</param>
         * <param name="k">The number of shares required to reproduce the secret.</param>
         * <returns>An array of share values generated from our secret of length n.</returns>
        */
        public static Share[] GenerateShares(string secret, int n, int k, int[] coefficients, SortedDictionary<int, int> avgFrequencyMap)
        {
            string[] shares = new string[n]; //The list that will eventually store our shares
            StringBuilder[] shareArray = new StringBuilder[n]; // temporarly stores share values

            for (int i = 0; i < shareArray.Length; i++)
            {
                shareArray[i] = new StringBuilder();
            }

            //Loop through each character in our secret string
            for (int i = 0; i < secret.Length; i++)
            {
                char[] shareChars = GenerateSecretChars(secret[i], n, k, coefficients, avgFrequencyMap);

                //now we append the corresponding character share values to our string share values
                for (int j = 0; j < shareArray.Length; j++)
                {
                    shareArray[j].Append(shareChars[j]);
                }
            }

            //Convert our share array of StringBuilder objects into a list of strings.
            for (int i = 0; i < shareArray.Length; i++)
            {
                shares[i] = shareArray[i].ToString();
            }

            Share[] shares2 = new Share[shares.Length];

            for (int i = 0; i < shares.Length; i++)
            {
                shares2[i] = new Share(i);
                shares2[i].Append(shares[i]);
            }

            return shares2;  //Return shares
        }

        /*
         * Generates Shamir Secret Shares for a given Bitmap image, using the (n,k) scheme
         * where n shares are generated and k shares are required to reconstruct the secret.
         * <param name="secret">The secret Bitmap Image we wish to share</param>
         * <param name="n">The number of shares to produce in the scheme</param>
         * <param name="k">The number of shares required to reconstruct the original secret.</param>
         * <returns>An array of share values generated from our secret image</returns>
        */
        public static Bitmap[] GenerateShares(Bitmap secret, int n, int k, SortedDictionary<int, int> avgFrequencyMap)
        {
            Bitmap[] shares = new Bitmap[n];    //our share images
            secretImage = secret;
            
            //Instantiate each share image to be the same size as the original
            for (int i = 0; i < shares.Length; i++)
            {
                shares[i] = new Bitmap(secret);
            }
                
            //shares[i] = new Bitmap(secretImage.Width, secretImage.Height,secret.PixelFormat);

            //loop through each pixel in the original to create shares
            for (int x = 0; x < secretImage.Width; x++)
            {
                for (int y = 0; y < secretImage.Height; y++)
                {
                    Color[] sharePixel = GenerateSecretPixel(secretImage.GetPixel(x, y), n, k);  //create an array of share pixels based on secret pixel

                    //Add the share pixels to the corresponding share images
                    for (int i = 0; i < shares.Length; i++)
                    {
                        shares[i].SetPixel(x, y, sharePixel[i]);
                    }
                }
            }

            return shares;  //return our share images
        }


        /**
         * Generate Secret Shares for a given Bitmap image using the (n,k) scheme.  Where n
         * shares are generated and k shares are requried to reconstruct the image.  Also size reduction
         * techniques are applied to ensure the shares are of smaller size than the origianl image
         * <param name="secret">The secret Bitmap Image we wish to share</param>
         * <param name="n">The number of shares to produce in the scheme</param>
         * <param name="k">The number of shares required to reconstruct the original secret.</param>
         * <param name="threshold">The threshold value used to determine similar pixels.</param>
         * <returns>An array of share values generated from our secret image</returns>
        */
        public static Bitmap[] GenerateReducedShares(Bitmap secret, int n, int k, int threshold)
        {
            Bitmap[] shares = new Bitmap[n];    //our share images
            secretImage = secret;
            
            //Make our share images all the same size as the original
            for (int i = 0; i < shares.Length; i++)
            {
                shares[i] = new Bitmap(secretImage.Width, secretImage.Height, secret.PixelFormat);
            }

            Color[] sharePixel;
            int setXValue;  //this keeps track of where we are placing the pixels in the share compared to what pixel in the secret is being processed(the x vaues aren't identical)
            int similarPixels;
            List<int> similarPixelVals = new List<int>();

            //Loop through each pixel in our secret, from a left to right fashion, line by line
            for (int currY = 0; currY < secretImage.Height;)
            {
                setXValue = 0;

                for (int currX = 0; currX < secretImage.Width;)
                {
                    //Create a Share from the first pixel
                    //creates an array of share pixels based on secret pixel
                    sharePixel = GenerateSecretPixel(secretImage.GetPixel(currX, currY), n, k);  

                    //Update our share images with the shared color pixel
                    for (int i = 0; i < shares.Length; i++)
                    {
                        if (setXValue < secretImage.Width)
                        {
                            shares[i].SetPixel(setXValue, currY, sharePixel[i]);
                        }
                    }

                    setXValue++;

                    //determine how many pixels are 'similar' to our current secret pixel
                    similarPixels = DetermineNumberOfSimilarPixels(currX, currY, threshold);

                    //add this value to our list of similar pixel values
                    similarPixelVals.Add(similarPixels);

                    //update our current pixel to the next 'non' similar pixel
                    currX = currX + similarPixels;  

                    //If we have 3 values in similarPixelVals, then create a new pixel in our share images
                    if (similarPixelVals.Count == 3)
                    {
                        //increment currX by 1 since we'll be adding a new pixel
                        //currX++;    

                        Color c = Color.FromArgb(similarPixelVals[0], similarPixelVals[1], similarPixelVals[2]);
                        
                        //sharePixel = GenerateSecretPixel(c, 5, 3);  //generate secret values of this pixel

                        //add the share pixels to our shares
                        for (int i = 0; i < shares.Length; i++)
                        {
                            if (setXValue < secretImage.Width)
                            {
                                shares[i].SetPixel(setXValue, currY, c);
                            }
                        }

                        setXValue++;

                        //empty our list so we can add more values again
                        similarPixelVals.Clear();   
                    }

                    //reset similar pixels to 0 for next pixel to be processed 
                    similarPixels = 0;                                         
                }

                //flush out ramining siimilarPixelValues
                if (similarPixelVals.Count == 1)
                {
                    //increment currX by 1 since we'll be adding a new pixel
                    //currX++;    

                    Color c = Color.FromArgb(similarPixelVals[0], 0, 0);
                    
                    //sharePixel = GenerateSecretPixel(c, 5, 3);  //generate secret values of this pixel

                    //add the share pixels to our shares
                    for (int i = 0; i < shares.Length; i++)
                    {
                        if (setXValue < secretImage.Width)
                        {
                            shares[i].SetPixel(setXValue, currY, c);
                        }
                    }

                    //empty our list so we can add more values again
                    similarPixelVals.Clear();   
                }
                else if (similarPixelVals.Count == 2)
                {
                    Color c = Color.FromArgb(similarPixelVals[0], similarPixelVals[1], 0);

                    //generate secret values of this pixel
                    //sharePixel = GenerateSecretPixel(c, 5, 3);  

                    //add the share pixels to our shares
                    for (int i = 0; i < shares.Length; i++)
                    {
                        if (setXValue < secretImage.Width)
                        {
                            shares[i].SetPixel(setXValue, currY, c);
                        }
                    }

                    //empty our list so we can add more values again
                    similarPixelVals.Clear();   
                }

                currY++;
            }

            return shares;
        }

        /**
         * Takes an individual pixel color and creates SSS shares for it based on the (n,k) scheme
         * <param name="color">The color we wish to share</param>
         * <param name="n">The number of shares to generate</param>
         * <param name="k">The number of shares required to reconstruct this color</param>
         * <returns>An array of colors that are the shares of the original color value</returns>
        */
        private static Color[] GenerateSecretPixel(Color color, int n, int k)
        {
            int[] coefficients = new int[k - 1];    //Random coeffiecent values for our polynomial
            Color[] shareColors = new Color[n];    //an array of colors which are the share values of our secret color

            //Generate random coefficents
            for (int i = 0; i < coefficients.Length; i++)
            {
                coefficients[i] = random.Next(IMAGE_MODULUS);
            }

            //Make sure any r,g,b values that are over 251 are turned to 250 to deal with the prime number of 251 issue
            if (color.R >= 251 || color.G >= 251 || color.B >= 251)
            {
                int red;
                int green;
                int blue;

                if (color.R >= 251)
                {
                    red = 250;
                }
                else
                {
                    red = color.R;
                }
                    

                if (color.G >= 251)
                {
                    green = 250;
                }
                else
                {
                    green = color.G;
                }

                if (color.B >= 251)
                {
                    blue = 250;
                }
                else
                {
                    blue = color.B;
                }

                color = Color.FromArgb(red, green, blue);
            }

            //Generate share values for each R,G,B component in the secret color
            int[] redShareValues = GenerateShareValues(color.R, n, k, coefficients, true);
            int[] greenShareValues = GenerateShareValues(color.G, n, k, coefficients, true);
            int[] blueShareValues = GenerateShareValues(color.B, n, k, coefficients, true);

            //Generate the share colors by creating them from the corresponding share color components
            for (int i = 0; i < shareColors.Length; i++)
            {
                shareColors[i] = Color.FromArgb(redShareValues[i], greenShareValues[i], blueShareValues[i]);
            }

            return shareColors; //return our share color array
        }

        /**
         * Takes a character and creates SSS shares for it based on the (n,k) scheme.
         * <param name="c">the char value to be shared.</param>
         * <param name="n">the number of shares to produce for the character.</param>
         * <param name="k">the number of shares reuired to reconstruct the character.</param>
         * <returns>an array of chars that are the shares of the original character value.</returns>
        */ 
        private static char[] GenerateSecretChars(char c, int n, int k, int[] coefficients, SortedDictionary<int, int> avgFrequencyMap)
        {
            if ((int)c == 8217)
            {
                c = '\'';
            }

            //An array of characters which are the share values of our secret char
            char[] shareChars = new char[n];

            //Gets the integer share values for our secret char
            int[] rawShareValues = GenerateShareValues((int)c, n, k, coefficients, false);

            //convert the int values into characters
            for (int i = 0; i < shareChars.Length; i++)
            {
                shareChars[i] = (char)(rawShareValues[i] + CHAR_OFFSET);

                if (avgFrequencyMap.ContainsKey(rawShareValues[i] + CHAR_OFFSET))
                {
                    avgFrequencyMap[rawShareValues[i] + CHAR_OFFSET] += 1;
                }
                else
                {
                    avgFrequencyMap[rawShareValues[i] + CHAR_OFFSET] = 1;
                }
            }

            for (int i = 0; i < shareChars.Length; i++)
            {
                if ((int)shareChars[i] == 157)
                {
                    shareChars[i] = getExtendedAscii(161);
                }
                else if ((int)shareChars[i] == 127)
                {
                    shareChars[i] = getExtendedAscii(160);
                }
                else
                {
                    shareChars[i] = getExtendedAscii(shareChars[i]);
                }
            }
            return shareChars;  //returns the character share values of the secret character
        }

        /**
         * Generates integer share values for a given integer secret.
         * <param name="secretVal">The integer we wish to share.</param>
         * <param name="n">Number of shares to generate.</param>
         * <param name="k">Subset of shares required to reconstruct secret</param>
         * <param name="coefficients">Random coefficent array used in share generation.</param>
         * <param name="image">Is this value being used for image or text SS.</param>
         * <returns>An integer array which is the share values of the secret.</returns>
        */
        private static int[] GenerateShareValues(int secretVal, int n, int k, int[] coefficients, bool image)
        {
            int[] rawShareValues = new int[n];

            //Create n polynomials based off the secret and coefficients
            for (int i = 0; i < n; i++)
            {
                int result = 0; //the result of the polynomial expression

                //Starting at the lowest degree of our polynomial (which is 1) and go up to the highest
                //(based on k - 1)
                for (int j = 1; j <= (k - 1); j++)
                {
                    //The value of our x value raised to its power
                    int degreeValue = (int)Math.Pow((double)(i + 1), (double)j);

                    //add this value to our result
                    result += coefficients[j - 1] * degreeValue; 
                }

                //Add the result of our secret now
                result += secretVal; 

                //We must mod the entire result now before storing its value
                if (image == false)
                {
                    result = Mod(result, TEXT_MODULUS);
                }
                else
                {
                    result = Mod(result, IMAGE_MODULUS);
                }

                //store the value as a character in our share char array
                rawShareValues[i] = result;   
            }

            return rawShareValues;
        }
        
        /**
         * Determine How many pixels after our current pixel are 'similar' based on a threshold, 
         * inorder to reduce the number of share values beeing generated
         * <param name="currX">The current x value of the pixel being shared</param>
         * <param name="currY">The current y value of the pixel being shared</param>
         * <returns>The number of pixels after the current pixel that are similar</returns>
        */
        private static int DetermineNumberOfSimilarPixels(int currX, int currY, int threshold)
        {
            //the number of similar successive pixels
            int similarPixels = 1;  

            //current pixel being shared
            Color currentPixel = secretImage.GetPixel(currX, currY);

            if (currX == (secretImage.Width - 1))
            {
                return 1;
            }

            //next pixel after the current pixel    
            Color nextPixel = secretImage.GetPixel(++currX, currY);     

            //loop through as many pixels in a row as possible comparing the 'similarity' of them to the current pixel
            while (true)
            {
                //Determine the similarity of the two succeeding pixels
                int test = currentPixel.R - nextPixel.R;

                if ((test >= threshold) || (test <= -1 * threshold))
                {
                    break;  //break out of the loop if difference of R value is greter than threshold
                }
                    
                test = currentPixel.G - nextPixel.G;

                if ((test >= threshold) || (test <= -1 * threshold))
                {
                    break;  //break out of the loop if difference of G value is greter than threshold
                }
                    
                test = currentPixel.B - nextPixel.B;

                if ((test >= threshold) || (test <= -1 * threshold))
                {
                    break;  //break out of the loop if difference of B value is greter than threshold
                }
                    

                //if the pixels are simiar, increment similarPixels by 1 and grab the next pixel
                similarPixels++;

                //move onto th next pixel in the row unless that was the last pixel in the row
                if (currX == (secretImage.Width - 1))
                {
                    break;  //break out of the loop if that was the last pixel in the row
                }
                else
                {
                    nextPixel = secretImage.GetPixel(++currX, currY);   //move onto the next pixel
                }
                    
                if (similarPixels == 255)
                {
                    return similarPixels;
                }
            }

            return similarPixels;
        }

        /**
         * A modulus function
         * </summary>
         * <param name="x">The x value in the case of x mod n</param>
         * <param name="n">The n value in the case of x mond n</param>
         * <returns></returns>
         */ 
        private static int Mod(int x, int n)
        {
            return (Math.Abs(x * n) + x) % n;
        }
    }
}

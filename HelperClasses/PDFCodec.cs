using Newtonsoft.Json;
using System.Linq;
using O2S.Components.PDF4NET;
using securePDFmerging.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace securePDFmerging
{
    class PDFCodec
    {
        private Dictionary<Object, Object> _words;
        private PDFDocument _encryptedPDF;
        private PDFDocument _decryptedPDF;
        private FileStream _encryptedFile;

        private KeyIV _keyIv;
        private int _keySize = 128;
        private int _blockSize = 128;

        public PDFCodec(Dictionary<Object, Object> words, Files Files)
        {
            _words = words;
            _encryptedPDF = Files.EncryptedPDF;
            _decryptedPDF = Files.DecryptedPDF;
            _encryptedFile = Files.EncryptedFile;

            setKeyIV();
        }

        /**
         * Sets an AES key and IV.
         * 
         */
        private void setKeyIV()
        {
            KeyGenerator keyGenerator = new KeyGenerator();
            _keyIv = keyGenerator.MakeKey(_keySize, _blockSize);
        }

        /**
         * Encrypts the PDF using AES word by word
         * 
         */
        public Dictionary<Object, Object> EncryptPDF()
        {
            using (RijndaelManaged aesEncryption = new RijndaelManaged())
            {
                string CipherText = "";
                aesEncryption.KeySize = _keySize;
                aesEncryption.BlockSize = _blockSize;
                aesEncryption.Mode = CipherMode.CBC;
                aesEncryption.Padding = PaddingMode.PKCS7;
                aesEncryption.Key = _keyIv.Key;
                aesEncryption.IV = _keyIv.IV;

                foreach (KeyValuePair<Object, Object> entry in _words)
                {
                    if (entry.Key.GetType() != typeof(string))
                    {
                        List<Words> Words = (List<Words>)entry.Value;
                        int pageIndex = (int)entry.Key;

                        for (int i = 0; i < Words.Count; i++)
                        {
                            Words word = Words[i];
                            byte[] plainText = ASCIIEncoding.UTF8.GetBytes(word.Word);

                            ICryptoTransform crypto = aesEncryption.CreateEncryptor();
                            byte[] EncryptedText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);

                            CipherText += Convert.ToBase64String(EncryptedText);

                            Words[i].Word = CipherText;
                        }
                    }
                }

                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                var fi = new FileInfo(@"C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/bin/Release/UploadedFiles/Encrypted_Letter.bin");

                using (var binaryFile = fi.Create())
                {
                    string output = JsonConvert.SerializeObject(_words);
                    binaryFormatter.Serialize(binaryFile, output);
                    binaryFile.Flush();
                }

                return _words;
            }
        }

        /**
         * Encrypts the PDF using AES block by block
         * 
         */
        public void GetEncryptedPDF()
        {
            using (RijndaelManaged aesEncryption = new RijndaelManaged())
            {
                aesEncryption.KeySize = _keySize;
                aesEncryption.BlockSize = _blockSize;
                aesEncryption.Mode = CipherMode.ECB;
                aesEncryption.Padding = PaddingMode.PKCS7;
                aesEncryption.Key = _keyIv.Key;
                //aesEncryption.Key = ASCIIEncoding.UTF8.GetBytes("BTikvHBatPdAtgT3317QIQqGFY25WpIz");
                aesEncryption.IV = _keyIv.IV;

                foreach (KeyValuePair<Object, Object> entry in _words)
                {
                    _encryptedPDF.AddPage();
                    if (entry.Key.GetType() != typeof(string))
                    {
                        List<Blocks> Words = (List<Blocks>) entry.Value;
                        int pageIndex = (int) entry.Key;

                        for (int i = 0; i < Words.Count; i++)
                        {
                            Blocks word = Words[i];
                            byte[] plainText = Encoding.ASCII.GetBytes(word.Block);

                            ICryptoTransform crypto = aesEncryption.CreateEncryptor();
                            byte[] EncryptedText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);

                            string key = Encoding.ASCII.GetString(aesEncryption.Key);
                            string CipherText = Convert.ToBase64String(EncryptedText);
                            string c = Encoding.ASCII.GetString(EncryptedText);
                            string cAn = ToBinary(ConvertToByteArray(word.Block, Encoding.ASCII));
                            string keyAnci = ToBinary(ConvertToByteArray(key, Encoding.ASCII));


                            if (word.Chunks.Count == 0)
                            {
                                _encryptedPDF.Pages[0].Canvas.DrawText(CipherText, word.FontBase, word.FontBrush, word.Left[0], word.Top[0]);
                            }
                            else
                            {
                                for(int j = 0; j <= word.Chunks.Count; j++)
                                {
                                    string text = GetString(CipherText, j, word.Chunks);
                                    _encryptedPDF.Pages[0].Canvas.DrawText(text, word.FontBase, word.FontBrush, word.Left[j], word.Top[j]);
                                }
                            }
                        }
                    }
                }
            }

            string encryptedFilePath = Path.GetFullPath("C:/Meet/STUDY/Cryptography/securePDFmerging/securePDFEncrytion/bin/Release/UploadedFiles/Encrypted_letter.pdf");
            _encryptedPDF.Save(encryptedFilePath);
        }

        /**
         * Processes chunks of string and returns desired string
         * 
         */
        private string GetString(string CipherText, int index, List<int> Chunks)
        {
            int length;
            string str;

            if (index == 0)
            {
                length = Chunks[index];
                str = CipherText.Substring(0, length);
            }
            else if (index + 1 > Chunks.Count)
            {
                length = CipherText.Length - Chunks[index - 1];
                str = CipherText.Substring(Chunks[index - 1], length);
            }
            else
            {
                length = CipherText.Length - Chunks[index] - Chunks[index - 1];
                str = CipherText.Substring(Chunks[index], length);
            }

            return str;
        }

        /**
         * Converts string to byte array
         * 
         */
        private byte[] ConvertToByteArray(string str, Encoding encoding)
        {
            return encoding.GetBytes(str);
        }

        /**
         * Converts byte array to binary string
         * 
         */
        private String ToBinary(Byte[] data)
        {
            return string.Join(" ", data.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
        }
    }
}







using securePDFmerging.Models;
using System.Security.Cryptography;

namespace securePDFmerging
{
    class KeyGenerator
    {
        /**
         * Genrates an AES key and IV
         * 
         */
        public KeyIV MakeKey(int KeySize, int BlockSize)
        {
            RijndaelManaged aesEncryption = new RijndaelManaged
            {
                KeySize = KeySize,
                BlockSize = BlockSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            aesEncryption.GenerateIV();
            aesEncryption.GenerateKey();

            KeyIV keyIv = new KeyIV
            {
                Key = aesEncryption.Key,
                IV = aesEncryption.IV
            };

            return keyIv;
        }
    }
}

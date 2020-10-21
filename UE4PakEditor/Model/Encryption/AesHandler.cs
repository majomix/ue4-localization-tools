using System.Collections.Generic;
using System.Security.Cryptography;

namespace UE4PakEditor.Model.Encryption
{
    public static class AesHandler
    {
        public const int BlockSize = 16 * 8;
        static readonly Rijndael Cipher;
        static readonly Dictionary<byte[], ICryptoTransform> CachedTransforms = new Dictionary<byte[], ICryptoTransform>();

        static AesHandler()
        {
            Cipher = Rijndael.Create();
            Cipher.Mode = CipherMode.ECB;
            Cipher.Padding = PaddingMode.Zeros;
            Cipher.BlockSize = BlockSize;
        }

        public static byte[] DecryptAes(byte[] data, byte[] key)
        {
            var decryptor = Cipher.CreateDecryptor(key, null);
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }
}

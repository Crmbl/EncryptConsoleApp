using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SilentCartographer
{
    public class EncryptionUtil
    {
        /// <summary>
        /// Encrypt the byte array.
        /// </summary>
        public static byte[] EncryptBytes(byte[] inputBytes, string passPhrase, string saltValue)
        {
            var cipher = new RijndaelManaged {Mode = CipherMode.CBC};
            var salt = Encoding.ASCII.GetBytes(saltValue);
            var password = new PasswordDeriveBytes(passPhrase, salt, "SHA1", 2);

            var encryptor = cipher.CreateEncryptor(password.GetBytes(32), password.GetBytes(16));
            byte[] cipherBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherBytes = memoryStream.ToArray();
                }
            }

            return cipherBytes;
        }

        /// <summary>
        /// Decrypt the byte array.
        /// </summary>
        public static byte[] DecryptBytes(byte[] encryptedBytes, string passPhrase, string saltValue)
        {
            var cipher = new RijndaelManaged {Mode = CipherMode.CBC};
            var salt = Encoding.ASCII.GetBytes(saltValue);
            var password = new PasswordDeriveBytes(passPhrase, salt, "SHA1", 2);

            var decryptor = cipher.CreateDecryptor(password.GetBytes(32), password.GetBytes(16));
            byte[] plainBytes;
            using (var memoryStream = new MemoryStream(encryptedBytes))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    plainBytes = new byte[encryptedBytes.Length];
                    int decryptedCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);
                }
            }

            return plainBytes;
        }
    }
}

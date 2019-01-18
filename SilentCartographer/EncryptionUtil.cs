using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SilentCartographer
{
    public class EncryptionUtil
    {
        public static byte[] EncryptBytes(string plainText, string passPhrase, string saltValue)
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
                    using (var streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                    cipherBytes = memoryStream.ToArray();
                }
            }

            return cipherBytes;
        }

        public static string DecryptBytes(byte[] encryptedBytes, string passPhrase, string saltValue)
        {
            var cipher = new RijndaelManaged {Mode = CipherMode.CBC};
            var salt = Encoding.ASCII.GetBytes(saltValue);
            var password = new PasswordDeriveBytes(passPhrase, salt, "SHA1", 2);

            var decryptor = cipher.CreateDecryptor(password.GetBytes(32), password.GetBytes(16));
            string plainBytes;
            using (var memoryStream = new MemoryStream(encryptedBytes))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (var streamReader = new StreamReader(cryptoStream))
                    {
                        plainBytes = streamReader.ReadToEnd();
                    }
                }
            }

            return plainBytes;
        }

        public static string Encipher(string input, int key)
        {
            var output = string.Empty;
            foreach (var ch in input)
            {
                if (!char.IsLetter(ch))
                    output += ch;

                var d = char.IsUpper(ch) ? 'A' : 'a';
                output += (char)((ch + key - d) % 26 + d);
            }

            if (output.Contains("\\"))
                output = output.Replace("\\", "--");

            return output;
        }

        /// <summary>
        /// Decipher !
        /// </summary>
        public static string Decipher(string input, int key)
        {
            if (input.Contains("--"))
                input = input.Replace("--", "\\");

            return Encipher(input, 26 - key);
        }
    }
}
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SilentCartographer
{
    public class EncryptionUtil
    {
        public static byte[] EncryptBytes(byte[] inputBytes, string passPhrase, string saltValue)
        {
            var cipher = new RijndaelManaged {Mode = CipherMode.CBC};
            var salt = Encoding.ASCII.GetBytes(saltValue);
            var password = new PasswordDeriveBytes(passPhrase, salt, "SHA1", 2);

            var encryptor = cipher.CreateEncryptor(password.GetBytes(32), password.GetBytes(16));
            var memoryStream = new MemoryStream();
            memoryStream.Write(BitConverter.GetBytes(inputBytes.Length), 0, 4);
            var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            cryptoStream.FlushFinalBlock();
            var cipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            return cipherBytes;
        }

        public static byte[] DecryptBytes(byte[] encryptedBytes, string passPhrase, string saltValue)
        {
            var cipher = new RijndaelManaged {Mode = CipherMode.CBC};
            var salt = Encoding.ASCII.GetBytes(saltValue);
            var password = new PasswordDeriveBytes(passPhrase, salt, "SHA1", 2);

            var decryptor = cipher.CreateDecryptor(password.GetBytes(32), password.GetBytes(16));
            var memoryStream = new MemoryStream(encryptedBytes);
            var tmpByte = new Byte[4];
            var memoryRead = memoryStream.Read(tmpByte, 0, 4);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainBytes = new byte[BitConverter.ToInt32(tmpByte, 0)];
            var DecryptedCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

            memoryStream.Close();
            cryptoStream.Close();

            return plainBytes;
        }

        public static string Encipher(string input, int key)
        {
            var output = string.Empty;
            foreach (var ch in input)
            {
                if (!char.IsLetter(ch))
                    output += ch;
                else
                {
                    var d = char.IsUpper(ch) ? 'A' : 'a';
                    output += (char)((ch + key - d) % 26 + d);
                }
            }

            if (output.Contains("\\"))
                output = output.Replace("\\", "--");

            return output;
        }

        public static string Decipher(string input, int key)
        {
            if (input.Contains("--"))
                input = input.Replace("--", "\\");

            return Encipher(input, 26 - key);
        }
    }
}
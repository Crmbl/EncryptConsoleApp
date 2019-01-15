using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using SilentCartographer.Objects;

namespace SilentCartographer
{
    class Program
    {
        static void Main(string[] args)
        {
            //EncryptTest();
            Mapping();
        }

        private static void EncryptTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var sample = @"C:\Users\axels\Downloads\sample.gif";
            var encrypted = @"C:\Users\axels\Downloads\ecrypted.gif";
            var decrypted = @"C:\Users\axels\Downloads\decrypted.gif";
            var fileBytes = File.ReadAllBytes(sample);

            var resultEncrypt = EncryptionUtil.EncryptBytes(fileBytes, "password", "salt");
            File.WriteAllBytes(encrypted, resultEncrypt);

            var resultDecrypt = EncryptionUtil.DecryptBytes(resultEncrypt, "password", "salt");
            File.WriteAllBytes(decrypted, resultDecrypt);

            var elapsedTime = timer.ElapsedMilliseconds;
            timer.Stop();
            Console.WriteLine($"ElapsedTime : {elapsedTime} ms");
            Console.ReadLine();
        }

        private static void Mapping()
        {
            var folderPath = new DirectoryInfo(@"C:\Users\SCHAEFAX\Documents\Perso\SilentCartographer");
            var mappedTree = new Folder();

            mappedTree.WalkDirectoryTree(folderPath);

            var json = JsonConvert.SerializeObject(mappedTree, Formatting.Indented);
            var jsonFile = $"{Environment.CurrentDirectory}\\mapping";
            File.WriteAllText(jsonFile, json);

            using (StreamReader reader = new StreamReader(jsonFile))
            {
                var line = "";
                while (line != null)
                {
                    line = reader.ReadLine();
                    Console.WriteLine(line);
                }
            }

            Console.ReadLine();
        }
    }
}

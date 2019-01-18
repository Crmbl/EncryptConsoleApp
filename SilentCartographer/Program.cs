using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SilentCartographer.Objects;

namespace SilentCartographer
{
    class Program
    {
        public static FolderObject MappedTree { get; set; }

        public static FolderObject FlattenedTree { get; set; }

        public static string Password { get; set; }

        public static string Salt { get; set; }

        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            #region Checking state

            if (!File.Exists($"{Environment.CurrentDirectory}\\poivre"))
                Console.WriteLine("> Error. Can't find password file. ABORT!");
            if (!File.Exists($"{Environment.CurrentDirectory}\\sel"))
                Console.WriteLine("> Error. Can't find salt file. ABORT!");

            Password = File.ReadAllText($"{Environment.CurrentDirectory}\\poivre");
            Salt = File.ReadAllText($"{Environment.CurrentDirectory}\\sel");

            if (!args.Any() || args.Length != 2)
            {
                if (!args.Any())
                    Console.WriteLine("> Error. No args provided, needed 2.");
                if (args.Length != 2)
                    Console.WriteLine($"> Error. No correct amount of args provided : {args.Length}, needed 2");

                Console.ReadLine();
                Environment.Exit(-1);
            }

            var originPath = args.First();
            var originExists = Directory.Exists(originPath);
            var destPath = args.Last();
            var destExists = Directory.Exists(destPath);

            if (!originExists || !destExists)
            {
                if (!originExists)
                    Console.WriteLine($"> Error. The origin directory does not exists : {originPath}.");
                if (!destExists)
                    Console.WriteLine($"> Error. The destination directory does not exists : {destPath}.");

                Console.ReadLine();
                Environment.Exit(-1);
            }

            Console.WriteLine("- [OK]");
            Console.WriteLine("> Checkup prerequisites done");
            Console.WriteLine($"- Origin directory path : {originPath}");
            Console.WriteLine($"- Destination directory path : {destPath}");
            Console.WriteLine("< Continue ?");
            Console.ReadLine();

            #endregion //Checking state

            #region //////////////// DECRYPT

            //TODO FIX not working
            var mappingEnciphered = EncryptionUtil.Encipher("mapping", 3);
            Console.WriteLine("- [OK]");
            Console.WriteLine($"> Begin decryption of files in {destPath}");
            var bytes = File.ReadAllBytes($"{destPath}\\{mappingEnciphered}");
            var resultDecrypt = EncryptionUtil.DecryptBytes(bytes, Password, Salt);
            File.WriteAllText($"{destPath}\\Decrypted\\mapping", resultDecrypt);
            var dir = new DirectoryInfo(destPath);
            foreach (var file in dir.GetFiles())
            {
                if (file.Name == mappingEnciphered) continue;
                var teubs = File.ReadAllBytes($"{destPath}\\{file.Name}");
                var decryptedFile = EncryptionUtil.DecryptBytes(teubs, Password, Salt);
                File.WriteAllText($"{destPath}\\Decrypted\\{EncryptionUtil.Decipher(file.Name, 3)}", decryptedFile);
            }
            Console.WriteLine("< Decryption done. Shutting down");
            stopWatch.Stop();
            Console.WriteLine($"\nExecuted in {stopWatch.ElapsedMilliseconds} ms");
            Console.ReadLine();
            Environment.Exit(0);

            #endregion ///////////// DECRYPT
            #region //////////////// ENCRYPT

            //TODO FIX not working
            Console.WriteLine("> Starting mapping generation");
            MappedTree = new FolderObject();
            MappedTree.WalkDirectoryTree(new DirectoryInfo(originPath));
            var nbFilesToTreat = CountFiles(MappedTree);
            Console.WriteLine("- [OK] Mapping generation done");
            Console.WriteLine("< Continue ?");
            Console.ReadLine();

            Console.WriteLine("> Begin files encryption from originPath");
            FlattenedTree = new FolderObject(MappedTree.Name, new List<FileObject>(MappedTree.Files), new List<FolderObject>(MappedTree.Folders));
            for (var i = 0; i < FlattenedTree.Folders.Count; i++)
                FlattenedTree.Folders.AddRange(FlattenedTree.Folders[i].Folders);
            EncryptFiles(new DirectoryInfo(originPath), new DirectoryInfo(destPath));
            var nbFilesTreated = new DirectoryInfo(destPath).GetFiles().Length;
            Console.WriteLine($"- {nbFilesTreated} files have been treated on {nbFilesToTreat}");
            if (nbFilesTreated == nbFilesToTreat)
            {
                Console.WriteLine("- [OK]");
                Console.WriteLine("> Begin mapping encryption");
                GenerateEncryptedJson(new DirectoryInfo(destPath));
                Console.WriteLine("- Mapping encryption done");
                Console.WriteLine($"- Output path : {destPath}\\mapping");
                Console.WriteLine("< All files have been treated. Shutting down");
            }
            else
            {
                Console.WriteLine("- [KO]");
                Console.WriteLine("- Not all files have been treated ...");
                Console.WriteLine("< Abort mapping encryption. Shutting down");
            }

            stopWatch.Stop();
            Console.WriteLine($"\nExecuted in {stopWatch.ElapsedMilliseconds} ms");
            Console.ReadLine();
            Environment.Exit(0);

            #endregion ///////////// ENCRYPT
        }

        private static void EncryptFiles(DirectoryInfo dir, DirectoryInfo destDir)
        {
            foreach (var file in dir.GetFiles())
            {
                var fileText = File.ReadAllText(file.FullName);
                var resultEncrypt = EncryptionUtil.EncryptBytes(fileText, Password, Salt);
                var ext = file.Extension;
                var fileName = file.Name;

                var i = 0;
                while (File.Exists($"{destDir}\\{EncryptionUtil.Encipher(fileName, 3)}"))
                    fileName = fileName.Replace($"{(i == 0 ? string.Empty : i.ToString())}{ext}", $"{++i}{ext}");

                var tmpFile = GetFile(file.Name, dir.Name);
                if (tmpFile == null) continue;

                tmpFile.UpdatedName = fileName;
                var encipheredName = EncryptionUtil.Encipher(fileName, 3);
                File.WriteAllBytes($"{destDir}\\{encipheredName}", resultEncrypt);
            }

            foreach (var subDir in dir.GetDirectories())
                EncryptFiles(subDir, destDir);
        }

        private static FileObject GetFile(string fName, string dName)
        {
            foreach (var sub in FlattenedTree.Folders)
                foreach (var file in sub.Files)
                    if (file.OriginName == fName && file.UpdatedName == null && sub.Name == dName)
                        return file;

            return null;
        }

        private static void GenerateEncryptedJson(DirectoryInfo destDir)
        {
            var json = JsonConvert.SerializeObject(MappedTree, Formatting.Indented);
            var jsonFile = $"{destDir.FullName}\\{EncryptionUtil.Encipher("mapping", 3)}";
            File.WriteAllText(jsonFile, json);

            var fileText = File.ReadAllText(jsonFile);
            var resultEncrypt = EncryptionUtil.EncryptBytes(fileText, Password, Salt);
            File.WriteAllBytes(jsonFile, resultEncrypt);
        }

        private static int CountFiles(FolderObject folderObject)
        {
            var amount = folderObject.Files?.Count ?? 0;
            foreach (var subFolder in folderObject.Folders)
                amount += CountFiles(subFolder);

            return amount;
        }
    }
}
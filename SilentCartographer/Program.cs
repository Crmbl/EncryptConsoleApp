using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SilentCartographer.Objects;

namespace SilentCartographer
{
    class Program
    {
        #region Properties

        public static FolderObject MappedTree { get; set; }

        public static FolderObject FlattenedTree { get; set; }

        public static string Password { get; set; }

        public static string Salt { get; set; }

        #endregion //Properties

        private const bool DoDecrypt = false;

        static void Main(string[] args)
        {
            #region Checking state

            var error = false;
            if (!File.Exists($"{Environment.CurrentDirectory}\\poivre"))
                { Console.WriteLine("> Error. Can't find password file. ABORT!"); error = true; }
            if (!File.Exists($"{Environment.CurrentDirectory}\\sel"))
                { Console.WriteLine("> Error. Can't find salt file. ABORT!"); error = true; }
            if (!File.Exists($"{Environment.CurrentDirectory}\\settings"))
                { Console.WriteLine("> Error. Can't find settings file. ABORT!"); error = true; }
            if (error) { Console.ReadLine(); Environment.Exit(-1); }

            Password = File.ReadAllText($"{Environment.CurrentDirectory}\\poivre");
            Salt = File.ReadAllText($"{Environment.CurrentDirectory}\\sel");
            var settings = File.ReadAllText($"{Environment.CurrentDirectory}\\settings").Split(';');

            var originPath = settings.First();
            var originExists = Directory.Exists(originPath);
            var destPath = settings.Last();
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
            Console.WriteLine($"- [Encrypt mode active?] : {!DoDecrypt}");
            Console.WriteLine("< Continue ?");
            Console.ReadLine();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            #endregion //Checking state

            #region //////////////// DECRYPT

            if (DoDecrypt)
            {
                Console.WriteLine("- [OK]");
                Console.WriteLine($"> Begin decryption of files in {destPath}");
                DecryptFiles(new DirectoryInfo(destPath));
                Console.WriteLine("< Decryption done. Shutting down");
                stopWatch.Stop();
                Console.WriteLine($"\nExecuted in {stopWatch.ElapsedMilliseconds} ms");
                Console.ReadLine();
                Environment.Exit(0);
            }

            #endregion ///////////// DECRYPT
            #region //////////////// ENCRYPT

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
                Console.WriteLine($"- Output path : {destPath}\\mapping.json");
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

        #region Methods

        private static void EncryptFiles(DirectoryInfo dir, DirectoryInfo destDir)
        {
            foreach (var file in dir.GetFiles())
            {
                var bytes = File.ReadAllBytes(file.FullName);
                var resultEncrypt = EncryptionUtil.EncryptBytes(bytes, Password, Salt);
                var ext = file.Extension;
                var fileName = file.Name;

                var i = 0;
                while (File.Exists($"{destDir}\\{EncryptionUtil.Encipher(fileName, 10)}"))
                    fileName = fileName.Replace($"{(i == 0 ? string.Empty : i.ToString())}{ext}", $"{++i}{ext}");

                var tmpFile = GetFile(file.Name, dir.Name);
                if (tmpFile == null) continue;

                tmpFile.UpdatedName = fileName;
                var encipheredName = EncryptionUtil.Encipher(fileName, 10);
                File.WriteAllBytes($"{destDir}\\{encipheredName}", resultEncrypt);
            }

            foreach (var subDir in dir.GetDirectories())
                EncryptFiles(subDir, destDir);
        }

        private static void DecryptFiles(DirectoryInfo dir)
        {
            foreach (var file in dir.GetFiles())
            {
                var bytes = File.ReadAllBytes($"{dir.FullName}\\{file.Name}");
                var decryptedFile = EncryptionUtil.DecryptBytes(bytes, Password, Salt);
                if (!Directory.Exists($"{dir.FullName}\\Decrypted"))
                    Directory.CreateDirectory($"{dir.FullName}\\Decrypted");
                File.WriteAllBytes($"{dir.FullName}\\Decrypted\\{EncryptionUtil.Decipher(file.Name, 10)}", decryptedFile);
            }
        }

        private static FileObject GetFile(string fName, string dName)
        {
            if (FlattenedTree.Files.Any())
            {
                foreach(var file in FlattenedTree.Files)
                if (file.OriginName == fName && file.UpdatedName == null && FlattenedTree.Name == dName)
                    return file;
            }

            foreach (var sub in FlattenedTree.Folders)
                foreach (var file in sub.Files)
                    if (file.OriginName == fName && file.UpdatedName == null && sub.Name == dName)
                        return file;

            return null;
        }

        private static void GenerateEncryptedJson(DirectoryInfo destDir)
        {
            var json = JsonConvert.SerializeObject(MappedTree, Formatting.Indented);
            var jsonFile = $"{destDir.FullName}\\{EncryptionUtil.Encipher("mapping.json", 10)}";
            File.WriteAllText(jsonFile, json);

            //var bytes = File.ReadAllBytes(jsonFile);
            var bytes = Encoding.UTF8.GetBytes(File.ReadAllText(jsonFile));
            var resultEncrypt = EncryptionUtil.EncryptBytes(bytes, Password, Salt);
            File.WriteAllBytes(jsonFile, resultEncrypt);
        }

        private static int CountFiles(FolderObject folderObject)
        {
            var amount = folderObject.Files?.Count ?? 0;
            foreach (var subFolder in folderObject.Folders)
                amount += CountFiles(subFolder);

            return amount;
        }

        #endregion //Methods
    }
}
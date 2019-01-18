using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SilentCartographer.Objects;

namespace SilentCartographer
{
    class Program
    {
        public static FolderObject MappedFolderObject { get; set; }

        public static string Password { get; set; }

        public static string Salt { get; set; }

        static void Main(string[] args)
        {
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

            Console.WriteLine("> Everything is fine :");
            Console.WriteLine($"- Origin directory path : {originPath}");
            Console.WriteLine($"- Destination directory path : {destPath}");
            Console.WriteLine("< Continue ?");
            Console.ReadLine();

            #endregion //Checking state

            ////////////////////// DECRYPT
            //var bytes = File.ReadAllBytes($"{destPath}\\mapping");
            //var resultDecrypt = EncryptionUtil.DecryptBytes(bytes, Password, Salt);
            //File.WriteAllBytes($"{destPath}\\mappingTest", resultDecrypt);
            //return;
            ////////////////////// DECRYPT

            Console.WriteLine("> Starting mapping generation");
            Console.WriteLine("- Processing ...");
            MappedFolderObject = new FolderObject();
            MappedFolderObject.WalkDirectoryTree(new DirectoryInfo(originPath));
            var nbFilesToTreat = Count(MappedFolderObject);
            Console.WriteLine("- Mapping generation done");
            Console.WriteLine("< Continue ?");
            Console.ReadLine();

            Console.WriteLine("> Begin files encryption from originPath");
            EncryptFiles(new DirectoryInfo(originPath), new DirectoryInfo(destPath));
            var nbFilesTreated = new DirectoryInfo(destPath).GetFiles().Length;
            Console.WriteLine($"- {nbFilesTreated} files have been treated on {nbFilesToTreat}");
            //if (nbFilesTreated == nbFilesToTreat)
            //{
            //    Console.WriteLine("> Begin mapping encryption");
                GenerateEncryptedJson(new DirectoryInfo(destPath));
            //    Console.WriteLine("- Mapping encryption done");
            //    Console.WriteLine($"- Output path : {destPath}\\mapping");
            //    Console.WriteLine("< Continue ?");
            //    Console.ReadLine();

            //    Console.WriteLine("< All files have been treated. Application shutting down");
            //    Console.ReadLine();
            //    Environment.Exit(0);
            //}
            //else
            //{
            //    Console.WriteLine("< Not all files have been treated ...");
            //    Console.WriteLine("- Abort mapping encryption. Application shutting down.");

            //    Console.ReadLine();
            //    Environment.Exit(0);
            //}
        }

        private static void EncryptFiles(DirectoryInfo dir, DirectoryInfo destDir)
        {
            foreach (var file in dir.GetFiles())
            {
                var fileBytes = File.ReadAllBytes(file.FullName);
                var resultEncrypt = EncryptionUtil.EncryptBytes(fileBytes, Password, Salt);
                var ext = file.Extension;
                var fileName = file.Name;

                var i = 0;
                while (File.Exists($"{destDir}\\{EncryptionUtil.Encipher(fileName, 10)}"))
                    fileName = fileName.Replace($"{(i == 0 ? string.Empty : i.ToString())}{ext}", $"{++i}{ext}");

                var tmpFolder = GetFolderObject(MappedFolderObject, dir.Name, file.Name);
                var tmpFile = tmpFolder?.Files.FirstOrDefault(x => x.OriginName == file.Name);
                if (tmpFile == null) continue;

                tmpFile.UpdatedName = fileName;
                var encipheredName = EncryptionUtil.Encipher(fileName, 10);
                File.WriteAllBytes($"{destDir}\\{encipheredName}", resultEncrypt);
            }

            foreach (var subDir in dir.GetDirectories())
                EncryptFiles(subDir, destDir);
        }

        private static void GenerateEncryptedJson(DirectoryInfo destDir)
        {
            var json = JsonConvert.SerializeObject(MappedFolderObject, Formatting.Indented);
            var jsonFile = $"{destDir.FullName}\\mapping";
            File.WriteAllText(jsonFile, json);

            //var fileBytes = File.ReadAllBytes(jsonFile);
            //var resultEncrypt = EncryptionUtil.EncryptBytes(fileBytes, Password, Salt);
            //File.WriteAllBytes(jsonFile, resultEncrypt);
        }

        private static int Count(FolderObject folderObject)
        {
            var amount = folderObject.Files?.Count ?? 0;
            if (folderObject.Folders != null)
                foreach (var subFolder in folderObject.Folders)
                    amount += Count(subFolder);

            return amount;
        }

        private static FolderObject GetFolderObject(FolderObject folder, string folderName, string fileName)
        {
            //TODO it won't go through every folder, stops at first Folders == null.
            if (folder.Name == folderName && folder.Files != null && folder.Files.Any(f => f.OriginName == fileName))
                return folder;

            if (folder.Folders != null)
                foreach (var subFolder in folder.Folders)
                    return GetFolderObject(subFolder, folderName, fileName);

            return null;
        }
    }
}

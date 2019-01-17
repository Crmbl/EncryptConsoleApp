using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SilentCartographer.Objects;

namespace SilentCartographer
{
    class Program
    {
        public static Folder MappedFolder { get; set; }

        public static string Password { get; set; }

        public static string Salt { get; set; }

        static void Main(string[] args)
        {
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

            Console.WriteLine("> Starting mapping generation");
            Console.WriteLine("- Processing ...");
            MappedFolder = new Folder();
            MappedFolder.WalkDirectoryTree(new DirectoryInfo(originPath));
            var nbFilesToTreat = Count(MappedFolder);
            Console.WriteLine("- Mapping generation done");
            Console.WriteLine("< Continue ?");
            Console.ReadLine();

            Console.WriteLine("> Begin files encryption from originPath");
            EncryptFiles(new DirectoryInfo(originPath), new DirectoryInfo(destPath));
            var nbFilesTreated = new DirectoryInfo(destPath).GetFiles().Length - 1;
            Console.WriteLine($"- {nbFilesTreated} files have been treated on {nbFilesToTreat}");
            if (nbFilesTreated == nbFilesToTreat)
            {
                Console.WriteLine("> Begin mapping encryption");
                GenerateEncryptedJson(new DirectoryInfo(destPath));
                Console.WriteLine("- Mapping encryption done");
                Console.WriteLine($"- Output path : {destPath}\\mapping");
                Console.WriteLine("< Continue ?");
                Console.ReadLine();

                Console.WriteLine("< All files have been treated. Application shutting down");
                Console.ReadLine();
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("< Not all files have been treated ...");
                Console.WriteLine("- Abort mapping encryption. Application shutting down.");

                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Encrypt all files in directory and sub dir till the end !
        /// </summary>
        /// <param name="dir">Origin directory.</param>
        /// <param name="destDir">Destination directory.</param>
        /// <returns>Returns the number of files processed.</returns>
        private static void EncryptFiles(DirectoryInfo dir, DirectoryInfo destDir)
        {
            foreach (var file in dir.GetFiles())
            {
                var fileBytes = File.ReadAllBytes(file.FullName);
                var resultEncrypt = EncryptionUtil.EncryptBytes(fileBytes, Password, Salt);
                
                //TODO maybe encrypt name too ?
                //var newPath = $"{destDir}\\{file.Name}";
                var ext = file.Extension;
                var fileName = file.Name;

                var i = 0;
                while (File.Exists($"{destDir}\\{fileName}"))
                    fileName = fileName.Replace($"{(i == 0 ? string.Empty : i.ToString())}{ext}", $"{++i}{ext}");




                File.WriteAllBytes($"{destDir}\\{fileName}", resultEncrypt);
            }

            foreach (var subDir in dir.GetDirectories())
                EncryptFiles(subDir, destDir);

            //var resultDecrypt = EncryptionUtil.DecryptBytes(resultEncrypt, "password", "salt");
            //File.WriteAllBytes(decrypted, resultDecrypt);
        }

        /// <summary>
        /// Generate mapping of directories to json file.
        /// </summary>
        /// <param name="dir">Start folder.</param>
        /// <param name="destDir">Destination to write the json file to.</param>
        /// <returns>Returns number of files to process later.</returns>
        private static int GenerateMapping(DirectoryInfo dir, DirectoryInfo destDir)
        {
            var mappedTree = new Folder();
            mappedTree.WalkDirectoryTree(dir);

            var json = JsonConvert.SerializeObject(mappedTree, Formatting.Indented);
            var jsonFile = $"{destDir.FullName}\\mapping";
            File.WriteAllText(jsonFile, json);

            MappedFolder = mappedTree;
            return Count(mappedTree);
        }

        private static void GenerateEncryptedJson(DirectoryInfo destDir)
        {
            var json = JsonConvert.SerializeObject(MappedFolder, Formatting.Indented);
            var jsonFile = $"{destDir.FullName}\\mapping";
            File.WriteAllText(jsonFile, json);

            var fileBytes = File.ReadAllBytes(jsonFile);
            var resultEncrypt = EncryptionUtil.EncryptBytes(fileBytes, Password, Salt);
            File.WriteAllBytes(jsonFile, resultEncrypt);
        }

        /// <summary>
        /// Count each files from origin directory.
        /// </summary>
        private static int Count(Folder folder)
        {
            var amount = folder.FileNames?.Count ?? 0;
            if (folder.Folders != null)
                foreach (var subFolder in folder.Folders)
                    amount += Count(subFolder);

            return amount;
        }
    }
}

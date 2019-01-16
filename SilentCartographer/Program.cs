using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SilentCartographer.Objects;

namespace SilentCartographer
{
    class Program
    {
        public static Folder Folder { get; set; }

        public static string Password { get; }

        public static string Salt { get; }

        //args[0] = @"C:\Users\SCHAEFAX\Documents\Perso\SilentCartographer";
        static void Main(string[] args)
        {
            // TODO get password and salt from file and check them.
            //Password = 
            //Salt = 

            if (!args.Any() || args.Length != 2)
            {
                if (!args.Any())
                    Console.WriteLine("► Error. No args provided, needed 2.");
                if (args.Length != 2)
                    Console.WriteLine($"► Error. No correct amount of args provided : {args.Length}, needed 2");

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
                    Console.WriteLine($"► Error. The origin directory does not exists : {originPath}.");
                if (!destExists)
                    Console.WriteLine($"► Error. The destination directory does not exists : {destPath}.");

                Console.ReadLine();
                Environment.Exit(-1);
            }

            Console.WriteLine("► Everything is fine :");
            Console.WriteLine($"→ Origin directory path : {originPath}");
            Console.WriteLine($"→ Destination directory path : {destPath}");
            Console.WriteLine("◄ Continue ?");
            Console.ReadLine();

            Console.WriteLine("► Starting mapping generation");
            Console.WriteLine("→ Processing ...");
            var nbFilesToTreat = GenerateMapping(new DirectoryInfo(originPath), new DirectoryInfo(destPath));
            Console.WriteLine("→ Mapping generation done");
            Console.WriteLine($"→ Output path : {destPath}\\mapping");
            Console.WriteLine("◄ Continue ?");
            Console.ReadLine();

            Console.WriteLine("► Begin files encryption from originPath");
            var nbFilesTreated = EncryptFiles(new DirectoryInfo(destPath));
            Console.WriteLine($"→ {nbFilesTreated} files have been treated on {nbFilesToTreat}");
            if (nbFilesTreated == nbFilesToTreat)
            {
                Console.WriteLine("◄ All files have been treated. Application shutting down");
                Console.ReadLine();
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("◄ Not all files have been treated ... Enjoy debugging :D");
            }
        }

        private static int EncryptFiles(DirectoryInfo destDir)
        {
            // TODO recursivity
            foreach (var file in Folder.FileNames)
            {
                var fileBytes = File.ReadAllBytes(file);
                var resultEncrypt = EncryptionUtil.EncryptBytes(fileBytes, Password, Salt);
                var name = file.Split('/').Last();
                //TODO maybe encrypt name too ?
                File.WriteAllBytes($"{destDir}\\{name}", resultEncrypt);
            }

            //var sample = @"C:\Users\axels\Downloads\sample.gif";
            //var encrypted = @"C:\Users\axels\Downloads\ecrypted.gif";
            //var decrypted = @"C:\Users\axels\Downloads\decrypted.gif";
            //var fileBytes = File.ReadAllBytes(sample);

            //var resultEncrypt = EncryptionUtil.EncryptBytes(fileBytes, "password", "salt");
            //File.WriteAllBytes(encrypted, resultEncrypt);

            //var resultDecrypt = EncryptionUtil.DecryptBytes(resultEncrypt, "password", "salt");
            //File.WriteAllBytes(decrypted, resultDecrypt);

            return destDir.GetFiles().Length - 1;
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

            Folder = mappedTree;
            return Count(Folder);
        }

        /// <summary>
        /// Count each files from origin directory.
        /// </summary>
        private static int Count(Folder folder)
        {
            var amount = folder.FileNames.Count;
            foreach (var subFolder in folder.Folders)
            {
                amount += subFolder.FileNames?.Count ?? 0;
                if (subFolder.Folders.Any())
                    amount += Count(subFolder);
            }

            return amount;
        }
    }
}

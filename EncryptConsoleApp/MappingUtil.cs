using System;
using System.Collections.Generic;
using System.IO;

namespace EncryptConsoleApp
{
    public class Folder
    {
        public string Name { get; set; }
        public IList<string> FileNames { get; set; }
        public IList<Folder> Folders { get; set; }

        public Folder()
        {
            FileNames = new List<string>();
            Folders = new List<Folder>();
        }
    }

    public static class MappingUtil
    {
        public static Folder WalkDirectoryTree(this Folder folder, DirectoryInfo root)
        {
            folder.Name = root.Name;
            FileInfo[] files = null;
            try
            {
                files = root.GetFiles("*.*");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files == null) return folder;
            foreach (FileInfo fi in files)
                folder.FileNames.Add(fi.Name);

            var subDirs = root.GetDirectories();
            foreach (DirectoryInfo dirInfo in subDirs)
                folder.Folders.Add(WalkDirectoryTree(new Folder(), dirInfo));

            return folder;
        }

        public static void WriteTree(this Folder folder)
        {
            Console.WriteLine("┌ " + folder.Name);
            foreach (var fileName in folder.FileNames)
            {
                Console.WriteLine("├ " + fileName);
            }

            foreach (var subFolder in folder.Folders)
            {
                subFolder.WriteTree();
            }
        }
    }
}

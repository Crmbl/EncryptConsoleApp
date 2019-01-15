using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SilentCartographer.Objects;

namespace SilentCartographer
{
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

            if (files != null && files.Any())
            {
                folder.FileNames = new List<string>();
                foreach (var fi in files)
                    folder.FileNames.Add(fi.Name);
            }

            var subDirs = root.GetDirectories();
            if (subDirs.Any())
            {
                folder.Folders = new List<Folder>();
                foreach (var dirInfo in subDirs)
                    folder.Folders.Add(WalkDirectoryTree(new Folder(), dirInfo));
            }

            return folder;
        }
    }
}

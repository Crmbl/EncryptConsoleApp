using System;
using System.IO;
using System.Linq;
using SilentCartographer.Objects;

namespace SilentCartographer
{
    public static class MappingUtil
    {
        public static FolderObject WalkDirectoryTree(this FolderObject folderObject, DirectoryInfo root)
        {
            folderObject.Name = root.Name;
            FileInfo[] files = null;
            try
            {
                files = root.GetFiles("*.*");
            }
            catch (UnauthorizedAccessException e) { Console.WriteLine(e.Message); }
            catch (DirectoryNotFoundException e) { Console.WriteLine(e.Message); }

            if (files != null && files.Any())
                foreach (var fi in files)
                    folderObject.Files.Add(new FileObject(fi.Name));

            var subDirs = root.GetDirectories();
            if (subDirs.Any())
                foreach (var dirInfo in subDirs)
                    folderObject.Folders.Add(WalkDirectoryTree(new FolderObject(), dirInfo));

            return folderObject;
        }
    }
}

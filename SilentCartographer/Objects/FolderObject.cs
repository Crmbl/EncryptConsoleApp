using System.Collections.Generic;

namespace SilentCartographer.Objects
{
    public class FolderObject
    {
        public string Name { get; set; }

        public string FullPath { get; set; }

        public List<FileObject> Files { get; set; }

        public List<FolderObject> Folders { get; set; }

        public FolderObject()
        {
            Files = new List<FileObject>();
            Folders = new List<FolderObject>();
        }

        public FolderObject(string name, string fullPath, List<FileObject> files, List<FolderObject> folders)
        {
            Name = name;
            FullPath = fullPath;
            Files = files;
            Folders = folders;
        }
    }
}

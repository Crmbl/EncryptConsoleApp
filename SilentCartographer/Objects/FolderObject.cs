using System.Collections.Generic;

namespace SilentCartographer.Objects
{
    public class FolderObject
    {
        public string Name { get; set; }
        public List<FileObject> Files { get; set; }
        public List<FolderObject> Folders { get; set; }

        public FolderObject()
        {
        }
    }
}

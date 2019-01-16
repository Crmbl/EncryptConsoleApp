using System.Collections.Generic;

namespace SilentCartographer.Objects
{
    public class Folder
    {
        public string Name { get; set; }
        public List<string> FileNames { get; set; }
        public List<Folder> Folders { get; set; }

        public Folder()
        {
        }
    }
}

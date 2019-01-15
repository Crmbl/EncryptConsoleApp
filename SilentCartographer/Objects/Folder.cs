using System.Collections.Generic;

namespace SilentCartographer.Objects
{
    public class Folder
    {
        public string Name { get; set; }
        public IList<string> FileNames { get; set; }
        public IList<Folder> Folders { get; set; }

        public Folder()
        {
        }
    }
}

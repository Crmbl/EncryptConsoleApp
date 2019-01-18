namespace SilentCartographer.Objects
{
    public class FileObject
    {
        public string OriginName { get; set; }
        public string UpdatedName { get; set; }

        public FileObject()
        {
        }

        public FileObject(string originName)
        {
            OriginName = originName;
        }
    }
}

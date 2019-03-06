using System;
using System.Drawing;
using System.IO;
using System.Linq;
using MediaToolkit;
using MediaToolkit.Model;
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
            {
                foreach (var fi in files)
                {
                    FileObject file;
                    try
                    {
                        using (Stream stream = File.OpenRead(fi.FullName))
                        {
                            using (var srcImg = Image.FromStream(stream, false, false))
                            {
                                file = new FileObject(fi.Name, srcImg.Width.ToString(), srcImg.Height.ToString());
                            }
                        }
                    }
                    catch (Exception) // it is a video !
                    {
                        var inputFile = new MediaFile { Filename = fi.FullName };
                        using (var engine = new Engine()) { engine.GetMetadata(inputFile); }

                        var size = inputFile.Metadata.VideoData.FrameSize.Split('x');
                        file = new FileObject(fi.Name, size.First(), size.Last());
                    }

                    folderObject.Files.Add(file);
                }
            }

            var subDirs = root.GetDirectories();
            if (subDirs.Any())
                foreach (var dirInfo in subDirs)
                    folderObject.Folders.Add(WalkDirectoryTree(new FolderObject(), dirInfo));

            return folderObject;
        }
    }
}

using TestProject.App_Start;

namespace TestProject.Models
{
    public class FileDetailViewModel
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string Path { get; set; }
        public string ReadableFileSize
        {
            get
            {
                return FileSize > 0 ? FileSize.GetFileSize() : string.Empty;
            }
        }
    }
}
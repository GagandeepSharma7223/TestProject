using System.Collections.Generic;
using TestProject.App_Start;

namespace TestProject.Models
{
    public class DirectoryDetailViewModel : BaseResponseViewModel
    {
        public int FileCount
        {
            get
            {
                return Files.Count;
            }
        }
        public long DirectorySize { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ReadableDirectorySize
        {
            get
            {
                return DirectorySize > 0 ? DirectorySize.GetFileSize() : string.Empty;
            }
        }
        public List<FileDetailViewModel> Files { get; set; } = new List<FileDetailViewModel>();
    }
}
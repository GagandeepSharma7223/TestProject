namespace TestProject.Models
{
    public class BaseResponseViewModel
    {
        public bool Success
        {
            get { return string.IsNullOrEmpty(ErrorMessage); }
        }
        public string ErrorMessage { get; set; }
    }
}
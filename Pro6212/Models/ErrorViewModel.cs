namespace Prog6212.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }
}

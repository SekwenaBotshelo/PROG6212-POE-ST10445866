namespace PROG6212_POE.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Additional error tracking
        public string? ErrorMessage { get; set; }
        public int? StatusCode { get; set; }
        public string? StackTrace { get; set; }
    }
}
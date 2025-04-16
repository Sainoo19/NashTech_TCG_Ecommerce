namespace NashTech_TCG_MVC.Models.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}

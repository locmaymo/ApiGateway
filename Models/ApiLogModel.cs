namespace ApiGateway.Models
{
    public class ApiLogModel
    {
        public DateTime Timestamp { get; set; }    // Thời gian
        public string Method { get; set; }         // GET, POST, etc.
        public string Endpoint { get; set; }       // API endpoint
        public int StatusCode { get; set; }        // Mã trạng thái HTTP
        public string LogLevel { get; set; }      // Mức độ log
        public object Request { get; set; }        // Nội dung Request (tuỳ chọn)
        public object Response { get; set; }       // Nội dung Response (tuỳ chọn)
    }
}

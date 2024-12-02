namespace ApiGateway.Models
{
    public class Filter
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }

    public class LogSearchRequest
    {
        public string SearchTerm { get; set; }
        public int From { get; set; } = 0;
        public int Size { get; set; } = 50;
        public List<Filter> Filters { get; set; } = new List<Filter>();
        // Add StartDate and EndDate properties
        public string StartDate { get; set; }  // Preferably use string if you're receiving date as string from frontend
        public string EndDate { get; set; }    // Alternatively, you can use DateTime?
    }
}
namespace StargateAPI.Business.Data
{
    public class ProcessLog
    {
        public int Id { get; set; }

        public DateTime TimestampUtc { get; set; }

        // "INFO" or "ERROR"
        public string Level { get; set; } = string.Empty;

        // e.g. "GetPersonByNameHandler"
        public string Source { get; set; } = string.Empty;

        // Short message: what happened
        public string Message { get; set; } = string.Empty;

        // Optional: stack trace, JSON of payload, etc.
        public string? Details { get; set; }
    }
}
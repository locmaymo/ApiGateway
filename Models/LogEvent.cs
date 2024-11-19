using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ApiGateway.Models
{
    public class LogEvent
    {
        [JsonPropertyName("@timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("messageTemplate")]
        public string MessageTemplate { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("fields")]
        public Dictionary<string, object> Fields { get; set; }
    }
}
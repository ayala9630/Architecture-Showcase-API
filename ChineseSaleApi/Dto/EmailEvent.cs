using System;

namespace ChineseSaleApi.Dto
{
    public class EmailEvent
    {
        public Guid EventId { get; set; }
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

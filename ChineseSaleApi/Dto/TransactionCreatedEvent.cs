using System;

namespace ChineseSaleApi.Dto
{
    public class TransactionCreatedEvent
    {
        public Guid TransactionId { get; set; }
        public int? UserId { get; set; }
        public string? CustomerEmail { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public object? Payload { get; set; }
    }
}

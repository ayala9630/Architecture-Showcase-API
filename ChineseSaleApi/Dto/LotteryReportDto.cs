using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Dto
{
    public class LotteryReportDto
    {
        [Required]
        public int LotteryId { get; set; }
        [Required]
        public string LotteryName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? TotalCards { get; set; }
        public int? TotalSalesRevenue { get; set; }
        public bool IsDone { get; set; }
        public List<GiftWinnerDto> GiftWinners { get; set; } = new List<GiftWinnerDto>();
    }

    public class GiftWinnerDto
    {
        public int GiftId { get; set; }
        public string GiftName { get; set; }
        public string? GiftDescription { get; set; }
        public int? GiftValue { get; set; }
        public string? DonorName { get; set; }
        public string? CategoryName { get; set; }
        public WinnerUserDto? Winner { get; set; }
        public int TotalTicketsSold { get; set; }
    }

    public class WinnerUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}

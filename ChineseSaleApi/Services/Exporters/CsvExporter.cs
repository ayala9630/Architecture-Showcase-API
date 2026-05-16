using System;
using System.Linq;
using System.Text;
using ChineseSaleApi.Dto;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services.Exporters
{
    public class CsvExporter
    {
        private readonly ILogger<CsvExporter> _logger;

        public CsvExporter(ILogger<CsvExporter> logger)
        {
            _logger = logger;
        }

        public byte[] ExportToCsv(LotteryReportDto report)
        {
            try
            {
                var csv = new StringBuilder();

                // Add lottery summary header
                csv.AppendLine("=== Lottery Report ===");
                csv.AppendLine($"Lottery ID,{report.LotteryId}");
                csv.AppendLine($"Lottery Name,\"{report.LotteryName}\"");
                csv.AppendLine($"Start Date,{report.StartDate:yyyy-MM-dd}");
                csv.AppendLine($"End Date,{report.EndDate:yyyy-MM-dd}");
                csv.AppendLine($"Total Cards Sold,{report.TotalCards ?? 0}");
                csv.AppendLine($"Total Sales Revenue,¤{report.TotalSalesRevenue ?? 0}");
                csv.AppendLine($"Status,{(report.IsDone ? "Completed" : "In Progress")}");
                csv.AppendLine();

                // Add gift winners table header
                csv.AppendLine("=== Gift Winners ===");
                csv.AppendLine("Gift ID,Gift Name,Gift Description,Gift Value,Donor Name,Category,Tickets Sold,Winner ID,Winner Name,Winner Email,Winner Phone");

                // Add gift winners data
                foreach (var gift in report.GiftWinners)
                {
                    var giftName = EscapeCsvField(gift.GiftName);
                    var giftDescription = EscapeCsvField(gift.GiftDescription ?? "");
                    var donorName = EscapeCsvField(gift.DonorName ?? "");
                    var categoryName = EscapeCsvField(gift.CategoryName ?? "");

                    string winnerInfo;
                    if (gift.Winner != null)
                    {
                        var winnerName = EscapeCsvField($"{gift.Winner.FirstName} {gift.Winner.LastName}");
                        var winnerEmail = EscapeCsvField(gift.Winner.Email ?? "");
                        var winnerPhone = EscapeCsvField(gift.Winner.Phone ?? "");
                        winnerInfo = $"{gift.Winner.UserId},{winnerName},{winnerEmail},{winnerPhone}";
                    }
                    else
                    {
                        winnerInfo = ",\"No Winner Yet\",,";
                    }

                    csv.AppendLine($"{gift.GiftId},{giftName},{giftDescription},¤{gift.GiftValue ?? 0},{donorName},{categoryName},{gift.TotalTicketsSold},{winnerInfo}");
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export report to CSV for lottery {LotteryId}.", report?.LotteryId);
                throw;
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            // If field contains comma, quote, or newline, wrap in quotes and escape quotes
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return $"\"{field}\"";
        }
    }
}

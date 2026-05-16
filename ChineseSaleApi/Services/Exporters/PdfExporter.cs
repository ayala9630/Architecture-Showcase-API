using System;
using System.Linq;
using System.Text;
using ChineseSaleApi.Dto;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services.Exporters
{
    public class PdfExporter
    {
        private readonly ILogger<PdfExporter> _logger;

        public PdfExporter(ILogger<PdfExporter> logger)
        {
            _logger = logger;
        }

        public byte[] ExportToPdf(LotteryReportDto report)
        {
            try
            {
                var html = GenerateHtmlReport(report);
                return Encoding.UTF8.GetBytes(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export report to PDF (HTML) for lottery {LotteryId}.", report?.LotteryId);
                throw;
            }
        }

        private string GenerateHtmlReport(LotteryReportDto report)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='he' dir='rtl'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>Lottery Report - {report.LotteryName}</title>");
            html.AppendLine("    <style>");
            html.AppendLine(@"
        @media print {
            body { margin: 0; }
            .no-print { display: none; }
            .page-break { page-break-after: always; }
        }
        body {
            font-family: Arial, Helvetica, sans-serif;
            margin: 20px;
            direction: rtl;
            text-align: right;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 10px;
            margin-bottom: 30px;
            text-align: center;
        }
        .header h1 {
            margin: 0 0 10px 0;
            font-size: 32px;
        }
        .header .subtitle {
            font-size: 16px;
            opacity: 0.9;
        }
        .summary {
            background: #f8f9fa;
            border: 2px solid #dee2e6;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 30px;
        }
        .summary h2 {
            margin-top: 0;
            color: #495057;
            border-bottom: 2px solid #6c757d;
            padding-bottom: 10px;
        }
        .summary-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-top: 15px;
        }
        .summary-item {
            background: white;
            padding: 15px;
            border-radius: 5px;
            border-left: 4px solid #667eea;
        }
        .summary-item .label {
            color: #6c757d;
            font-size: 14px;
            margin-bottom: 5px;
        }
        .summary-item .value {
            font-size: 24px;
            font-weight: bold;
            color: #212529;
        }
        .status {
            display: inline-block;
            padding: 5px 15px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: bold;
        }
        .status.completed {
            background: #d4edda;
            color: #155724;
        }
        .status.in-progress {
            background: #fff3cd;
            color: #856404;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            background: white;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        th {
            background: #495057;
            color: white;
            padding: 12px;
            text-align: right;
            font-weight: bold;
        }
        td {
            padding: 12px;
            border-bottom: 1px solid #dee2e6;
        }
        tr:hover {
            background: #f8f9fa;
        }
        .winner-info {
            background: #d4edda;
            padding: 5px 10px;
            border-radius: 4px;
            display: inline-block;
        }
        .no-winner {
            color: #856404;
            font-style: italic;
        }
        .print-button {
            position: fixed;
            top: 20px;
            left: 20px;
            background: #667eea;
            color: white;
            border: none;
            padding: 12px 24px;
            font-size: 16px;
            border-radius: 5px;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        }
        .print-button:hover {
            background: #5568d3;
        }
        .footer {
            margin-top: 30px;
            padding-top: 20px;
            border-top: 2px solid #dee2e6;
            text-align: center;
            color: #6c757d;
            font-size: 14px;
        }
    ");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Print button
            html.AppendLine("    <button class='print-button no-print' onclick='window.print()'>🖨️ הדפס דוח</button>");

            // Header
            html.AppendLine("    <div class='header'>");
            html.AppendLine($"        <h1>דוח הגרלה - {EscapeHtml(report.LotteryName)}</h1>");
            //html.AppendLine($"        <div class='subtitle'>מזהה הגרלה: {report.LotteryId}</div>");
            html.AppendLine("    </div>");

            // Summary section
            html.AppendLine("    <div class='summary'>");
            html.AppendLine("        <h2>סיכום ההגרלה</h2>");
            html.AppendLine("        <div class='summary-grid'>");
            html.AppendLine("            <div class='summary-item'>");
            html.AppendLine("                <div class='label'>תאריך התחלה</div>");
            html.AppendLine($"                <div class='value'>{report.StartDate:dd/MM/yyyy}</div>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class='summary-item'>");
            html.AppendLine("                <div class='label'>תאריך סיום</div>");
            html.AppendLine($"                <div class='value'>{report.EndDate:dd/MM/yyyy}</div>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class='summary-item'>");
            html.AppendLine("                <div class='label'>כרטיסים שנמכרו</div>");
            html.AppendLine($"                <div class='value'>{report.TotalCards ?? 0}</div>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class='summary-item'>");
            html.AppendLine("                <div class='label'>סה\"כ הכנסות</div>");
            html.AppendLine($"                <div class='value'>₪{report.TotalSalesRevenue ?? 0:N0}</div>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class='summary-item'>");
            html.AppendLine("                <div class='label'>סטטוס</div>");
            var statusClass = report.IsDone ? "completed" : "in-progress";
            var statusText = report.IsDone ? "הושלמה" : "בתהליך";
            html.AppendLine($"                <div class='value'><span class='status {statusClass}'>{statusText}</span></div>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");

            // Winners table
            html.AppendLine("    <div class='summary'>");
            html.AppendLine($"        <h2>זוכים במתנות ({report.GiftWinners.Count} מתנות)</h2>");
            html.AppendLine("        <table>");
            html.AppendLine("            <thead>");
            html.AppendLine("                <tr>");
            html.AppendLine("                    <th>#</th>");
            html.AppendLine("                    <th>שם המתנה</th>");
            html.AppendLine("                    <th>תיאור</th>");
            html.AppendLine("                    <th>שווי</th>");
            html.AppendLine("                    <th>תורם</th>");
            html.AppendLine("                    <th>קטגוריה</th>");
            html.AppendLine("                    <th>כרטיסים שנמכרו</th>");
            html.AppendLine("                    <th>זוכה</th>");
            html.AppendLine("                </tr>");
            html.AppendLine("            </thead>");
            html.AppendLine("            <tbody>");
            int id = 1;
            foreach (var gift in report.GiftWinners)
            {
                html.AppendLine("                <tr>");
                html.AppendLine($"                    <td>{id++}</td>");
                html.AppendLine($"                    <td><strong>{EscapeHtml(gift.GiftName)}</strong></td>");
                html.AppendLine($"                    <td>{EscapeHtml(gift.GiftDescription ?? "-")}</td>");
                html.AppendLine($"                    <td>₪{gift.GiftValue ?? 0:N0}</td>");
                html.AppendLine($"                    <td>{EscapeHtml(gift.DonorName ?? "-")}</td>");
                html.AppendLine($"                    <td>{EscapeHtml(gift.CategoryName ?? "-")}</td>");
                html.AppendLine($"                    <td>{gift.TotalTicketsSold}</td>");

                if (gift.Winner != null)
                {
                    var winnerName = $"{gift.Winner.FirstName} {gift.Winner.LastName}";
                    html.AppendLine($"                    <td><div class='winner-info'>🎉 {EscapeHtml(winnerName)}<br/><small>{EscapeHtml(gift.Winner.Email ?? "")}</small></div></td>");
                }
                else
                {
                    html.AppendLine("                    <td><span class='no-winner'>טרם הוגרל</span></td>");
                }

                html.AppendLine("                </tr>");
            }

            html.AppendLine("            </tbody>");
            html.AppendLine("        </table>");
            html.AppendLine("    </div>");

            // Footer
            html.AppendLine("    <div class='footer'>");
            html.AppendLine($"        <p>דוח נוצר בתאריך: {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine("        <p>© Chinese Sale System - כל הזכויות שמורות</p>");
            html.AppendLine("    </div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#39;");
        }
    }
}

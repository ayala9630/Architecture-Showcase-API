using System;
using ChineseSaleApi.Dto;
using ChineseSaleApi.ServiceInterfaces;
using ChineseSaleApi.Services.Exporters;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services
{
    public class FileExportService : IFileExportService
    {
        private readonly CsvExporter _csvExporter;
        private readonly JsonExporter _jsonExporter;
        private readonly PdfExporter _pdfExporter;
        private readonly ILogger<FileExportService> _logger;

        public FileExportService(
            CsvExporter csvExporter,
            JsonExporter jsonExporter,
            PdfExporter pdfExporter,
            ILogger<FileExportService> logger)
        {
            _csvExporter = csvExporter;
            _jsonExporter = jsonExporter;
            _pdfExporter = pdfExporter;
            _logger = logger;
        }

        public byte[] ExportReportToCsv(LotteryReportDto report)
        {
            return _csvExporter.ExportToCsv(report);
        }

        public byte[] ExportReportToJson(LotteryReportDto report)
        {
            return _jsonExporter.ExportToJson(report);
        }

        public byte[] ExportReportToPdf(LotteryReportDto report)
        {
            return _pdfExporter.ExportToPdf(report);
        }

        public string GetFileName(int lotteryId, string format)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"LotteryReport_{lotteryId}_{timestamp}.{format.ToLower()}";
        }

        public string GetContentType(string format)
        {
            return format.ToLower() switch
            {
                "csv" => "text/csv",
                "json" => "application/json",
                "pdf" => "text/html",
                "html" => "text/html",
                _ => "application/octet-stream"
            };
        }
    }
}

using System;
using System.Text;
using System.Text.Json;
using ChineseSaleApi.Dto;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services.Exporters
{
    public class JsonExporter
    {
        private readonly ILogger<JsonExporter> _logger;

        public JsonExporter(ILogger<JsonExporter> logger)
        {
            _logger = logger;
        }

        public byte[] ExportToJson(LotteryReportDto report)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(report, options);
                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export report to JSON for lottery {LotteryId}.", report?.LotteryId);
                throw;
            }
        }
    }
}

using ChineseSaleApi.Dto;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface IFileExportService
    {
        byte[] ExportReportToCsv(LotteryReportDto report);
        byte[] ExportReportToJson(LotteryReportDto report);
        byte[] ExportReportToPdf(LotteryReportDto report);
        string GetFileName(int lotteryId, string format);
        string GetContentType(string format);
    }
}

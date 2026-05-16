using ChineseSaleApi.Dto;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface ILotteryService
    {
        Task AddLottery(CreateLotteryDto lotteryDto);
        Task DeleteLottery(int id);
        Task<List<LotteryDto>> GetAllLotteries();
        Task<LotteryDto?> GetLotteryById(int id);
        Task<bool?> UpdateLottery(UpdateLotteryDto lotteryDto);
        Task<UserDto?> Lottery(int giftId);
        Task<LotteryReportDto?> GetLotteryReport(int lotteryId);
    }
}
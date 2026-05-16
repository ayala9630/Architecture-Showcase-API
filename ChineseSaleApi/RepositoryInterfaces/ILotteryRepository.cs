using ChineseSaleApi.Models;

namespace ChineseSaleApi.RepositoryInterfaces
{
    public interface ILotteryRepository
    {
        Task<int> AddLottery(Lottery lottery);
        Task DeleteLottery(int id);
        Task<IEnumerable<Lottery>> GetAllLotteries();
        Task<Lottery?> GetLotteryById(int id);
        Task UpdateLottery(Lottery lottery);
    }
}
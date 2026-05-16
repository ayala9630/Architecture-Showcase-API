using ChineseSaleApi.Data;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ChineseSaleApi.Repositories
{
    public class LotteryRepository : ILotteryRepository
    {
        private readonly ChineseSaleContext _context;
        public LotteryRepository(ChineseSaleContext context)
        {
            _context = context;
        }
        //create
        public async Task<int> AddLottery(Lottery lottery)
        {
            _context.Lotteries.Add(lottery);
            await _context.SaveChangesAsync();
            return lottery.Id;
        }
        //read
        public async Task<IEnumerable<Lottery>> GetAllLotteries()
        {
            return await _context.Lotteries.ToListAsync();
        }
        public async Task<Lottery?> GetLotteryById(int id)
        {
            return await _context.Lotteries.FirstOrDefaultAsync(x => x.Id == id);
        }
        //update
        public async Task UpdateLottery(Lottery lottery)
        {
            _context.Lotteries.Update(lottery);
            await _context.SaveChangesAsync();
        }
        //delete
        public async Task DeleteLottery(int id)
        {
            var lottery = await _context.Lotteries.FirstOrDefaultAsync(x => x.Id == id);
            if (lottery != null)
            {
                _context.Lotteries.Remove(lottery);
                await _context.SaveChangesAsync();
            }
        }

        //public async Task<> Lottery(int lotteryId)
        //{
        //    var lottery = await _context.Lotteries.FirstOrDefaultAsync(x => x.Id == lotteryId);
        //    if (lottery == null)
        //    {
        //        return null;
        //    }
        //    _context.Lotteries.Update(lottery);
        //    await _context.SaveChangesAsync();
        //    return lottery.Id;
        //}
    }
}

using ChineseSaleApi.Data;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using ChineseSaleApi.Dto;

namespace ChineseSaleApi.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly ChineseSaleContext _context;
        public CardRepository(ChineseSaleContext context)
        {
            _context = context;
        }
        //create
        public async Task<int> AddCard(Card card)
        {
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card.Id;
        }
        //read
        public async Task<IEnumerable<Card>> GetAllCards(int lotteryId)
        {
            return await _context.Cards.Include(p => p.Gift).Include(x=>x.User).Where(x=>x.Gift.LotteryId==lotteryId).ToListAsync();
        }
        public async Task<List<Card?>> GetCardByGiftId(int id)
        {
            return await _context.Cards.Include(g=>g.Gift).Include(u=>u.User).Where(x => x.GiftId == id).ToListAsync();
        }

        public async Task<(IEnumerable<Card> items, int totalCount)> GetCardsWithPagination(int lotteryId, int pageNumber, int pageSize)
        {
            var query = _context.Cards.Include(p => p.Gift).Where(x => x.Gift.LotteryId == lotteryId).AsQueryable();
            var totalCount = await query.CountAsync();
            var cards = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (cards, totalCount);
        }

        public async Task<IEnumerable<Card>> GetWinnerCards(int lotteryId)
        {
            return await _context.Cards.Include(p => p.Gift)
                                       .Include(u => u.User)
                                       .Where(x => x.Gift.LotteryId == lotteryId && x.IsWin == true)
                                       .ToListAsync();
        }


        //public async Task<(IEnumerable<Card> items, int totalCount)> GetCardsWithPagination
        //
        //ByValue(int lotteryId, int pageNumber, int pageSize, bool ascending)
        //{
        //    var query = ascending? _context.Cards.Include(p => p.Gift).OrderBy(x => x.Gift.GiftValue).Where(x => x.Gift.LotteryId == lotteryId).AsQueryable():
        //    _context.Cards.Include(p => p.Gift).OrderByDescending(x => x.Gift.GiftValue).Where(x => x.Gift.LotteryId == lotteryId).AsQueryable();
        //    var totalCount = await query.CountAsync();
        //    var cards = await query
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();
        //    return (cards, totalCount);
        //}

        // public async Task<(IEnumerable<Card> items, int totalCount)> GetCardsWithPaginationSortByPurchases(int lotteryId, int pageNumber, int pageSize, bool ascending)
        // {
        //     var query = _context.Cards.Include(p => p.Gift).Where(x => x.Gift.LotteryId == lotteryId).AsQueryable()
        //     var totalCount = await query.CountAsync();
        //     var cards = await query
        //         .Skip((pageNumber - 1) * pageSize)
        //         .Take(pageSize)
        //         .ToListAsync();
        //     return (cards, totalCount);
        // }
        //update
        public async Task UpdateCardToWin(Card card)
        {
            _context.Cards.Update(card);
            await _context.SaveChangesAsync();
        }
        
        public async Task<bool> ResetWinnersByLotteryId(int lotteryId)
        {
            var winners = await _context.Cards.Include(g=>g.Gift)
                                              .Where(c=>c.Gift.LotteryId==lotteryId)
                                              .Where(c => c.IsWin == true)
                                              .ToListAsync();
            if (winners.Count == 0)
            {
                return false;
            }
            foreach (var winner in winners)
            {
                winner.IsWin = false;
            }
            _context.Cards.UpdateRange(winners);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
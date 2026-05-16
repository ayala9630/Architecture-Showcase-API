using ChineseSaleApi.Data;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ChineseSaleApi.Repositories
{
    public class CardCartRepository : ICardCartRepository
    {
        private readonly ChineseSaleContext _context;
        public CardCartRepository(ChineseSaleContext context)
        {
            _context = context;
        }
        //create
        public async Task<int> AddCardCart(CardCart cardCart)
        {
            _context.CardCarts.Add(cardCart);
            await _context.SaveChangesAsync();
            return cardCart.Id;
        }
        //read
        public async Task<IEnumerable<CardCart>> GetAllCardCarts()
        {
            return await _context.CardCarts.ToListAsync();
        }
        public async Task<CardCart?> GetCardCartById(int id)
        {
            return await _context.CardCarts.FirstOrDefaultAsync(x => x.Id == id);
        }
        //by user id
        public async Task<IEnumerable<CardCart>> GetCardCartsByUserId(int userId)
        {
            return await _context.CardCarts
                .Where(cc => cc.UserId == userId)
                .ToListAsync();
        }
        //update
        public async Task UpdateCardCart(CardCart cardCart)
        {
            _context.CardCarts.Update(cardCart);
            await _context.SaveChangesAsync();
        }
        //delete
        public async Task DeleteCardCart(int id)
        {
            var cardCart = await _context.CardCarts.FirstOrDefaultAsync(x => x.Id == id);
            if (cardCart != null)
            {
                _context.CardCarts.Remove(cardCart);
                await _context.SaveChangesAsync();
            }
        }

    }
}
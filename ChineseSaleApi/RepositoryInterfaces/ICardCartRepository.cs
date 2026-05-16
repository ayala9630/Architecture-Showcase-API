using ChineseSaleApi.Models;

namespace ChineseSaleApi.RepositoryInterfaces
{
    public interface ICardCartRepository
    {
        Task<int> AddCardCart(CardCart cardCart);
        Task DeleteCardCart(int id);
        Task<IEnumerable<CardCart>> GetAllCardCarts();
        Task<IEnumerable<CardCart>> GetCardCartsByUserId(int userId);
        Task<CardCart?> GetCardCartById(int id);
        Task UpdateCardCart(CardCart cardCart);
    }
}
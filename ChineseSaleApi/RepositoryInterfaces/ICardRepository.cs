using ChineseSaleApi.Models;

namespace ChineseSaleApi.RepositoryInterfaces
{
    public interface ICardRepository
    {
        Task<int> AddCard(Card card);
        Task<IEnumerable<Card>> GetAllCards(int lotteryId);
        Task<List<Card?>> GetCardByGiftId(int id);
         Task<(IEnumerable<Card> items, int totalCount)> GetCardsWithPagination(int lotteryId, int pageNumber, int pageSize);
        Task UpdateCardToWin(Card card);
        Task<bool> ResetWinnersByLotteryId(int lotteryId);
        Task<IEnumerable<Card>> GetWinnerCards(int lotteryId);
         //Task<(IEnumerable<Card>, int totalCount)> GetCardsWithPaginationSortByValue(int lotteryId, int pageNumber, int pageSize, bool ascending);
    }
}
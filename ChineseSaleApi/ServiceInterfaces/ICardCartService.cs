using ChineseSaleApi.Dto;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface ICardCartService
    {
        Task<int> CreateCardCar(CreateCardCartDto cardCartDto);
        Task<IEnumerable<CardCartDto>> GetCardCartsByUserId(int userId);
        Task DeleteCardCart(int id);
        Task<bool?> UpdateCardCart(UpdateQuantityDto cardCartDto);
    }
}
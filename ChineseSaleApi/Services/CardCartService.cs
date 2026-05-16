using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services
{
    public class CardCartService : ICardCartService
    {
        private readonly ICardCartRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CardCartService> _logger;

        public CardCartService(ICardCartRepository repository, IMapper mapper, ILogger<CardCartService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        //create
        public async Task<int> CreateCardCar(CreateCardCartDto cardCartDto)
        {
            if (cardCartDto == null) throw new ArgumentNullException(nameof(cardCartDto));
            if (cardCartDto.UserId <= 0) throw new ArgumentException("UserId must be greater than zero.", nameof(cardCartDto.UserId));
            if (cardCartDto.GiftId <= 0) throw new ArgumentException("GiftId must be greater than zero.", nameof(cardCartDto.GiftId));
            if (cardCartDto.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(cardCartDto.Quantity));

            try
            {
                CardCart cardCart = _mapper.Map<CardCart>(cardCartDto);
                return await _repository.AddCardCart(cardCart);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "CreateCardCar received a null argument: {@CardCartDto}", cardCartDto);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "CreateCardCar received invalid argument: {@CardCartDto}", cardCartDto);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a card cart for user {UserId}.", cardCartDto?.UserId);
                throw;
            }
        }

        //read
        public async Task<IEnumerable<CardCartDto>> GetCardCartsByUserId(int userId)
        {
            if (userId <= 0) throw new ArgumentException("UserId must be greater than zero.", nameof(userId));

            try
            {
                var cardCarts = await _repository.GetCardCartsByUserId(userId);
                return cardCarts.Select(cc => _mapper.Map<CardCartDto>(cc));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "GetCardCartsByUserId received invalid userId: {UserId}.", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get card carts for user {UserId}.", userId);
                throw;
            }
        }

        //update
        public async Task<bool?> UpdateCardCart(UpdateQuantityDto cardCartDto)
        {
            if (cardCartDto == null) throw new ArgumentNullException(nameof(cardCartDto));
            if (cardCartDto.Id <= 0) throw new ArgumentException("CardCart Id must be greater than zero.", nameof(cardCartDto.Id));
            if (cardCartDto.Quantity < 0) throw new ArgumentException("Quantity cannot be negative.", nameof(cardCartDto.Quantity));

            try
            {
                CardCart? cardCart = await _repository.GetCardCartById(cardCartDto.Id);
                if (cardCart != null)
                {
                    cardCart.Quantity = cardCartDto.Quantity;
                    await _repository.UpdateCardCart(cardCart);
                    return true;
                }
                return null;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "UpdateCardCart received a null argument: {@CardCartDto}", cardCartDto);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "UpdateCardCart received invalid argument: {@CardCartDto}", cardCartDto);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update card cart {CardCartId}.", cardCartDto?.Id);
                throw;
            }
        }

        //delete
        public async Task DeleteCardCart(int id)
        {
            if (id <= 0) throw new ArgumentException("CardCart Id must be greater than zero.", nameof(id));

            try
            {
                await _repository.DeleteCardCart(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "DeleteCardCart received invalid id: {CardCartId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete card cart {CardCartId}.", id);
                throw;
            }
        }
    }
}
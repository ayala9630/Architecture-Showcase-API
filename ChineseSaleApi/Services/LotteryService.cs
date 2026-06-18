using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChineseSaleApi.Services
{
    public class LotteryService : ILotteryService
    {
        private readonly ILotteryRepository _repository;
        private readonly ICardRepository _cardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGiftRepository _giftRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LotteryService> _logger;
        private readonly IKafkaProducerService? _kafkaProducer;
        private readonly RedisCacheService _cache;
        private readonly CacheSettings _cacheSettings;

        public LotteryService
            (
            ILotteryRepository repository,
            ICardRepository cardRepository,
            IUserRepository userRepository,
            IGiftRepository giftRepository,
            IMapper mapper,
            ILogger<LotteryService> logger,
            RedisCacheService cache,
            IOptions<CacheSettings> cacheSettings
            , IKafkaProducerService? kafkaProducer = null
            )
        {
            _repository = repository;
            _cardRepository = cardRepository;
            _userRepository = userRepository;
            _giftRepository = giftRepository;
            _mapper = mapper;
            _logger = logger;
            _kafkaProducer = kafkaProducer;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
        }
        //create
        public async Task AddLottery(CreateLotteryDto lotteryDto)
        {
            try
            {
                if (lotteryDto.EndDate <= lotteryDto.StartDate)
                {
                    throw new ArgumentException("Lottery end date must be after start date.", nameof(lotteryDto.EndDate));
                }

                List<LotteryDto> lotteries = await GetAllLotteries();
                if (lotteries.Count > 0)
                {
                    var prevLottery = lotteries.OrderByDescending(l => l.Id).First();
                    if (prevLottery != null && prevLottery.EndDate <= lotteryDto.StartDate)
                    {
                        throw new ArgumentException("New lottery's start date must be after the previous lottery's end date.", nameof(lotteryDto.StartDate));
                    }
                }
                Lottery lottery = _mapper.Map<Lottery>(lotteryDto);
                await _repository.AddLottery(lottery);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while adding lottery.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add lottery.");
                throw;
            }
        }
        //read
        public async Task<List<LotteryDto>> GetAllLotteries()
        {
            try
            {
                const string cacheKey = "lotteries";
                var cachedLotteries = await _cache.GetAsync<List<LotteryDto>>(cacheKey);

                if (cachedLotteries != null)
                {
                    return cachedLotteries;
                }

                var lotteries = await _repository.GetAllLotteries();
                var lotteryDtos = lotteries.Select(lottery => _mapper.Map<LotteryDto>(lottery)).ToList();

                await _cache.SetAsync(cacheKey, lotteryDtos, TimeSpan.FromHours(_cacheSettings.Lotteries.GetAllTtlHours));

                return lotteryDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all lotteries.");
                throw;
            }
        }
        public async Task<LotteryDto?> GetLotteryById(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));

            try
            {
                var cacheKey = $"lotteries:id:{id}";
                var cachedLottery = await _cache.GetAsync<LotteryDto>(cacheKey);

                if (cachedLottery != null)
                {
                    return cachedLottery;
                }

                var lottery = await _repository.GetLotteryById(id);
                if (lottery == null)
                {
                    return null;
                }

                var lotteryDto = _mapper.Map<LotteryDto>(lottery);
                await _cache.SetAsync(cacheKey, lotteryDto, TimeSpan.FromHours(_cacheSettings.Lotteries.GetByIdTtlHours));

                return lotteryDto;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for GetLotteryById: {LotteryId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get lottery by id {LotteryId}.", id);
                throw;
            }
        }

        public async Task<LotteryReportDto?> GetLotteryReport(int lotteryId)
        {
            try
            {
                var lottery = await _repository.GetLotteryById(lotteryId);
                if (lottery == null)
                {
                    return null;
                }

                var allGifts = await _giftRepository.GetAllGifts(lotteryId);
                var winnerCards = await _cardRepository.GetWinnerCards(lotteryId);
                var allCards = await _cardRepository.GetAllCards(lotteryId);

                var giftWinners = new List<GiftWinnerDto>();

                foreach (var gift in allGifts)
                {
                    var winningCard = winnerCards.FirstOrDefault(c => c.GiftId == gift.Id);
                    var ticketsSold = allCards.Count(c => c.GiftId == gift.Id);

                    var giftWinner = new GiftWinnerDto
                    {
                        GiftId = gift.Id,
                        GiftName = gift.Name,
                        GiftDescription = gift.Description,
                        GiftValue = gift.GiftValue,
                        DonorName = gift.Donor != null 
                            ? $"{gift.Donor.CompanyName} ({gift.Donor.FirstName} {gift.Donor.LastName})".Trim()
                            : null,
                        CategoryName = gift.Category?.Name,
                        TotalTicketsSold = ticketsSold,
                        Winner = winningCard?.User != null ? _mapper.Map<WinnerUserDto>(winningCard.User) : null
                    };

                    giftWinners.Add(giftWinner);
                }

                return new LotteryReportDto
                {
                    LotteryId = lottery.Id,
                    LotteryName = lottery.Name,
                    StartDate = lottery.StartDate,
                    EndDate = lottery.EndDate,
                    TotalCards = lottery.TotalCards,
                    TotalSalesRevenue = lottery.TotalSum,
                    IsDone = lottery.IsDone,
                    GiftWinners = giftWinners
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate lottery report for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        //update
        public async Task<bool?> UpdateLottery(UpdateLotteryDto lotteryDto)
        {
            try
            {
                if (lotteryDto.EndDate <= lotteryDto.StartDate)
                {
                    throw new ArgumentException("Lottery end date must be after start date.", nameof(lotteryDto.EndDate));
                }
                var lottery = await _repository.GetLotteryById(lotteryDto.Id);
                if (lottery == null)
                {
                    return null;
                }
                if (lottery.StartDate <= DateTime.Now)
                {
                    throw new ArgumentException("Cannot update a lottery that already started.", nameof(lotteryDto.Id));
                }

                _mapper.Map(lotteryDto, lottery);
                await _repository.UpdateLottery(lottery);
                return true;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed updating lottery {LotteryId}.", lotteryDto?.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update lottery {LotteryId}.", lotteryDto?.Id);
                throw;
            }
        }

        //delete
        public async Task DeleteLottery(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));

            try
            {
                var lottery = await _repository.GetLotteryById(id);
                if (lottery == null)
                {
                    throw new KeyNotFoundException("Lottery not found.");
                }
                if (lottery.StartDate <= DateTime.Now)
                {
                    throw new ArgumentException("Cannot delete a lottery that already started.", nameof(id));
                }
                await _repository.DeleteLottery(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Lottery not found: {LotteryId}.", id);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting lottery {LotteryId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete lottery {LotteryId}.", id);
                throw;
            }   
        }
            
        public async Task<bool> UpdateWin(Card card)
        {
            try
            {
                card.IsWin = true;
                await _cardRepository.UpdateCardToWin(card);
                if (_kafkaProducer != null)
                {
                    var evt = new TransactionCreatedEvent
                    {
                        TransactionId = Guid.NewGuid(),
                        UserId = card.UserId,
                        CustomerEmail = null,
                        Amount = 0,
                        CreatedAt = DateTime.UtcNow,
                        TransactionType = "LotteryWin",
                        Payload = new { CardId = card.Id, GiftId = card.GiftId }
                    };
                    _ = _kafkaProducer.PublishAsync(System.Text.Json.JsonSerializer.Serialize(evt));
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update win for card {CardId}.", card?.Id);
                throw;
            }
        }
        
        public async Task<UserDto?> Lottery(int giftId)
        {
            if (giftId <= 0) throw new ArgumentException("GiftId must be greater than zero.", nameof(giftId));

            try
            {
                List<Card?> cardsList = await _cardRepository.GetCardByGiftId(giftId);
                if (cardsList == null || !cardsList.Any())
                {
                    throw new KeyNotFoundException("No cards available for the specified gift.");
                }
                int winnerCardNumber = new Random().Next(1, cardsList.Count() + 1);
                User? winnerUser = await _userRepository.GetUserById(cardsList[winnerCardNumber - 1].UserId);
                if (winnerUser == null)
                {
                    throw new KeyNotFoundException("Winner user not found.");
                }
                await UpdateWin(cardsList[winnerCardNumber - 1]);
                return _mapper.Map<UserDto>(winnerUser);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Lottery selection failed for gift {GiftId}: {Message}", giftId, ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in Lottery for gift {GiftId}.", giftId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run lottery for gift {GiftId}.", giftId);
                throw;
            }
        }
    }
}
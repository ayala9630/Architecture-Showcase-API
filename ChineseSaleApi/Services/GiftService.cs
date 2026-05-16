using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ChineseSaleApi.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _repository;
        private readonly ICardRepository _cardRepository;
        private readonly ILotteryRepository _lotteryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GiftService> _logger;
        private readonly RedisCacheService _cache;
        private readonly CacheSettings _cacheSettings;

        public GiftService(
            ICardRepository cardRepository,
            IGiftRepository repository,
            ILotteryRepository lotteryRepository,
            IMapper mapper,
            ILogger<GiftService> logger,
            RedisCacheService cache,
            IOptions<CacheSettings> cacheSettings)
        {
            _repository = repository;
            _cardRepository = cardRepository;
            _lotteryRepository = lotteryRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
        }
        //create
        public async Task<int> AddGift(CreateGiftDto createGiftDto)
        {
            try
            {
                Lottery? lottery = await _lotteryRepository.GetLotteryById(createGiftDto.LotteryId);
                if (lottery?.StartDate < DateTime.Now)
                    throw new ArgumentException("Gifts cannot be added after the raffle has started.", nameof(createGiftDto.LotteryId));
                Gift gift = _mapper.Map<Gift>(createGiftDto);
                return await _repository.AddGift(gift);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "AddGift received a null argument: {@CreateGiftDto}", createGiftDto);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while adding gift for lottery {LotteryId}: {Message}", createGiftDto?.LotteryId, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding a gift for lottery {LotteryId}.", createGiftDto?.LotteryId);
                throw;
            }
        }
        //read
        public async Task<GiftDto?> GetGiftById(int id)
        {
            try
            {
                var cacheKey = $"gifts:id:{id}";
                var cachedGift = await _cache.GetAsync<GiftDto>(cacheKey);

                if (cachedGift != null)
                {
                    return cachedGift;
                }

                var gift = await _repository.GetGiftById(id);
                if (gift == null)
                {
                    return null;
                }

                var tmp = await _cardRepository.GetWinnerCards(gift.LotteryId);
                var winner = tmp.FirstOrDefault(x => x.GiftId == gift.Id);
                var giftDto = _mapper.Map<GiftDto>(gift);
                giftDto.winner = winner?.User?.FirstName + " " + winner?.User?.LastName;

                await _cache.SetAsync(cacheKey, giftDto, TimeSpan.FromHours(_cacheSettings.Gifts.GetByIdTtlHours));

                return giftDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gift by id {GiftId}.", id);
                throw;
            }
        }

        public async Task<UpdateGiftDto> GetGiftsByIdUpdate(int id)
        {
            try
            {
                var gift = await _repository.GetGiftById(id);
                return _mapper.Map<UpdateGiftDto>(gift);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gifts by lottery id {LotteryId}.", id);
                throw;
            }
        }
        
        public async Task<IEnumerable<GiftWithOldPurchaseDto>> GetAllGifts(int lotteryId, int userId)
        {
            try
            {
                var gifts = await _repository.GetAllGifts(lotteryId);
                return gifts.Select(gift =>
                {
                    var dto = _mapper.Map<GiftWithOldPurchaseDto>(gift);
                    dto.OldPurchaseCount = gift.Cards?.Count(x => x.UserId == userId) ?? 0;
                    return dto;
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all gifts for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        public async Task<PaginatedResultDto<GiftWithOldPurchaseDto>> GetGiftsByUserWithPagination(int lotteryId, int? userId, PaginationParamsDto paginationParams,dynamic request)
        {
            try
            {
                var (gifts, totalCount) = await _repository.GetGiftsByUserWithPagination(lotteryId, paginationParams.PageNumber, paginationParams.PageSize);
                var baseUrl = $"{request.Scheme}://{request.Host}"; 
                var giftDto = gifts.Select(gift =>
                {
                    var dto = _mapper.Map<GiftWithOldPurchaseDto>(gift);
                    dto.ImageUrl = $"{baseUrl}/images/{gift.ImageUrl}";
                    dto.OldPurchaseCount = userId != null ? gift.Cards.Count(x => x.UserId == userId) : 0;
                    return dto;
                }).ToList();
                return new PaginatedResultDto<GiftWithOldPurchaseDto>
                {
                    Items = giftDto,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gifts by user with pagination for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }


        public async Task<PaginatedResultDto<GiftWithOldPurchaseDto>> GetGiftsSearchPagination(int lotteryId, int? userId, PaginationParamsDto paginationParams, string? textSearch, string? type, string? sortType,bool? ascendingOrder,int? categoryId)
        {
            try
            {
                var winners = await _cardRepository.GetWinnerCards(lotteryId);
                var (gifts, totalCount) = await _repository.GetGiftsSearchPagination(lotteryId, paginationParams.PageNumber, paginationParams.PageSize, textSearch, type,sortType,ascendingOrder,categoryId);
                var giftsWithWinners = gifts.GroupJoin(
                    winners,
                    gift => gift.Id,
                    winner => winner.GiftId,
                    (gift, winner) => new { gift, winner }
                ).SelectMany(x => x.winner.DefaultIfEmpty(),
                (x, winner) => new
                {
                    gift = x.gift,
                    winner = winner != null ? winner : null
                });
                var giftDto = giftsWithWinners.Select(x =>
                {
                    var dto = _mapper.Map<GiftWithOldPurchaseDto>(x.gift);
                    dto.countCards = x.gift.Cards?.Count() ?? 0;
                    dto.OldPurchaseCount = userId != null ? x.gift.Cards.Count(y => y.UserId == userId) : 0;
                    dto.winner = x.winner?.User?.FirstName + " " + x.winner?.User?.LastName;
                    return dto;
                }).ToList();
                Console.WriteLine(giftDto);
                return new PaginatedResultDto<GiftWithOldPurchaseDto>
                {
                    Items = giftDto,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search gifts for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        public async Task<int> GetGiftCountByLotteryId(int lotteryId)
        {
            try
            {
                var cacheKey = $"gifts:count:{lotteryId}";
                var cachedCount = await _cache.GetAsync<int>(cacheKey);

                if (cachedCount != default(int))
                {
                    return cachedCount;
                }

                var count = await _repository.GetGiftCountByLotteryId(lotteryId);
                await _cache.SetAsync(cacheKey, count, TimeSpan.FromHours(_cacheSettings.Gifts.GetCountByLotteryTtlHours));

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gift count for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }


        //update
        public async Task<bool?> UpdateGift(UpdateGiftDto updateGiftDto)
        {
            try
            {
                if (updateGiftDto.LotteryId != 0)
                {
                    Lottery? lottery = await _lotteryRepository.GetLotteryById(updateGiftDto.LotteryId);
                    if (lottery?.StartDate < DateTime.Now)
                        throw new ArgumentException("Gifts cannot be updated after the raffle has started.", nameof(updateGiftDto.LotteryId));
                }
                var gift = await _repository.GetGiftById(updateGiftDto.Id);
                if (gift != null)
                {

                    _mapper.Map(updateGiftDto, gift);
                    await _repository.UpdateGift(gift);
                    return true;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating gift {GiftId}.", updateGiftDto?.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update gift {GiftId}.", updateGiftDto?.Id);
                throw;
            }
        }
        //delete
        public async Task DeleteGift(int id)
        {
            try
            {
                Gift? gift = await _repository.GetGiftById(id);
                Lottery? lottery = await _lotteryRepository.GetLotteryById(gift?.LotteryId ?? 0);
                if (lottery?.StartDate < DateTime.Now)
                    throw new ArgumentException("Gifts cannot be deleted after the raffle has started.", nameof(id));
                await _repository.DeleteGift(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting gift {GiftId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete gift {GiftId}.", id);
                throw;
            }
        }
    }

}

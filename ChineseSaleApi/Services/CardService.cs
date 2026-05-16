using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services
{
    public class CardService : ICardService
    {
        private readonly IGiftRepository _giftRepository;
        private readonly ICardRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CardService> _logger;
        public CardService(ICardRepository repository, ILogger<CardService> logger, IGiftRepository giftRepository, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _giftRepository = giftRepository;
            _mapper = mapper;
        }
        //create
        public async Task<int> AddCard(CreateCardDto createCardDto)
        {
            try
            {
                Card card = new Card
                {
                    UserId = createCardDto.UserId,
                    GiftId = createCardDto.GiftId,
                };
                return await _repository.AddCard(card);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "AddCard received a null argument: {@CreateCardDto}", createCardDto);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding a card.");
                throw;
            }
        }
        //read
        public async Task<List<ListCardDto>> GetAllCarsds(int lotteryId)
        {
            try
            {
                var cards = await _repository.GetAllCards(lotteryId);
                var winnerByGiftId = cards
                    .Where(x => x.IsWin == true && x.User != null)
                    .GroupBy(x => x.GiftId)
                    .ToDictionary(g => g.Key, g => MapUser(g.First().User!));
                return cards.GroupBy(x => new { x.Gift.Id, x.Gift.Name, x.Gift.ImageUrl })
                            .Select(g => new ListCardDto
                            {
                                GiftId = g.Key.Id,
                                GiftName = g.Key.Name,
                                ImageUrl = g.Key?.ImageUrl??"",
                                Quantity = g.Count(),
                                WinUser = winnerByGiftId.TryGetValue(g.Key.Id, out var winnerUser)
                                    ? winnerUser
                                    : null
                            }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all cards for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        public async Task<PaginatedResultDto<ListCardDto>> GetCardsWithPagination(int lotteryId, PaginationParamsDto paginationParams)
        {
            try
            {
                int pageNumber = paginationParams.PageNumber > 0 ? paginationParams.PageNumber : 1;
                int pageSize = paginationParams.PageSize > 0 ? paginationParams.PageSize : 10;
                var items = await _repository.GetAllCards(lotteryId);
                int totalCount = items.GroupBy(x => x.Gift.Id).Count();
                var winnerByGiftId = items
                    .Where(x => x.IsWin == true && x.User != null)
                    .GroupBy(x => x.GiftId)
                    .ToDictionary(g => g.Key, g => MapUser(g.First().User!));

                var cardDtos = items.GroupBy(x => new { x.Gift.Id, x.Gift.Name, x.Gift.ImageUrl })
                            .Select(g => new ListCardDto
                            {
                                GiftId = g.Key.Id,
                                GiftName = g.Key.Name,
                                ImageUrl = g.Key?.ImageUrl ?? "",
                                Quantity = g.Count(),
                                WinUser = winnerByGiftId.TryGetValue(g.Key.Id, out var winnerUser)
                                    ? winnerUser
                                    : null
                            })
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

                return new PaginatedResultDto<ListCardDto>
                {
                    Items = cardDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paginated cards for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        public async Task<PaginatedResultDto<ListCardDto>> GetCardsWithPaginationSorted(int lotteryId, PaginationParamsDto paginationParams,string? sortType , bool ascending)
        {
            try
            {
                int pageNumber = paginationParams.PageNumber > 0 ? paginationParams.PageNumber : 1;
                int pageSize = paginationParams.PageSize > 0 ? paginationParams.PageSize : 10;
                var items = await _repository.GetAllCards(lotteryId);
                var winnerByGiftId = items
                    .Where(x => x.IsWin == true && x.User != null)
                    .GroupBy(x => x.GiftId)
                    .ToDictionary(g => g.Key, g => MapUser(g.First().User!));
                //int totalCount = items.GroupBy(x => x.Gift.Id).Count();
                //var cardDtos = items.GroupBy(x => new { x.Gift.Id, x.Gift.Name,x.Gift.GiftValue, x.Gift.ImageUrl })
                //            .Select(g => new ListCardDto
                //            {
                //                GiftId = g.Key.Id,
                //                GiftName = g.Key.Name,
                //                ImageUrl = g.Key?.ImageUrl ?? "",
                //                GiftValue = g.Key?.GiftValue,
                //                Quantity = g.Count()
                //            })
                //            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                //            .Take(paginationParams.PageSize);


                var groupedCards = items.GroupBy(x => new { x.GiftId, x.Gift.Id, x.Gift.Name, x.Gift.GiftValue, x.Gift.ImageUrl }).ToList();

                var allGift = await _giftRepository.GetAllGifts(lotteryId);
                var giftsWithCards = allGift.GroupJoin(
                    groupedCards,
                    gift => gift.Id,
                    card => card.Key.GiftId,
                    (gift, card) => new { gift, card }
                    ).SelectMany(x => x.card.DefaultIfEmpty(),
                    (x, card) => new
                    {
                        card = card,
                        gift = x != null ? x : null
                    }).ToList();

                var cardsDto = giftsWithCards
                    .Where(g => g.gift != null)
                    .Select(g => new ListCardDto
                    {
                        GiftId = g.gift.gift.Id,
                        GiftName = g.gift.gift.Name,
                        ImageUrl = g.gift.gift?.ImageUrl ?? "",
                        GiftValue = g.gift.gift?.GiftValue,
                        Quantity = g.card != null ? g.card.Count() : 0,
                        WinUser = winnerByGiftId.TryGetValue(g.gift.gift.Id, out var winnerUser)
                            ? winnerUser
                            : null
                    })
                    .ToList();
                    

                if (sortType == "value")
                    cardsDto = ascending ? cardsDto.OrderBy(x => x.GiftValue).ToList() : cardsDto.OrderByDescending(x => x.GiftValue).ToList();
                else if (sortType == "purchases")
                    cardsDto = ascending ? cardsDto.OrderBy(x => x.Quantity).ToList() : cardsDto.OrderByDescending(x => x.Quantity).ToList();

                int totalCount = cardsDto.Count;
                var pagedCards = cardsDto
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                return new PaginatedResultDto<ListCardDto>
                    {
                        Items = pagedCards,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paginated cards sorted by value for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        //public async Task<PaginatedResultDto<ListCardDto>> GetCardsWithPaginationSortByPurchases(int lotteryId, PaginationParamsDto paginationParams, bool ascending)
        //{
        //    try
        //    {
        //        var items = await _repository.GetAllCards(lotteryId);
        //        int totalCount = items.GroupBy(x => x.Gift.Id).Count();
        //        var cardDtos = items.GroupBy(x => new { x.Gift.Id, x.Gift.Name, x.Gift.GiftValue, x.Gift.ImageUrl })
        //                    .Select(g => new ListCardDto
        //                    {
        //                        GiftId = g.Key.Id,
        //                        GiftName = g.Key.Name,
        //                        ImageUrl = g.Key?.ImageUrl ?? "",
        //                        GiftValue = g.Key?.GiftValue,
        //                        Quantity = g.Count()
        //                    })
        //                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
        //                    .Take(paginationParams.PageSize);


        //        return new PaginatedResultDto<ListCardDto>
        //        {
        //            Items = cardDtos,
        //            TotalCount = totalCount
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to get paginated cards sorted by purchases for lottery {LotteryId}.", lotteryId);
        //        throw;
        //    }
        //}

        public async Task<CardDto?> GetCardByGiftId(int id)
        {
            try
            {
                var cards = await _repository.GetCardByGiftId(id);
                if (cards == null) return null;
                var winnerCard = cards.FirstOrDefault(x => x.IsWin == true);
                string? winnerName = winnerCard?.User != null
                    ? $"{winnerCard.User.FirstName} {winnerCard.User.LastName}"
                    : null;
                var groupCards = cards.GroupBy(x => new { x.UserId, x.GiftId, x.Gift?.Name, x.User?.FirstName, x.User?.LastName })
                                .Select(g => new
                                {
                                    UserFirstName = g.Key.FirstName,
                                    UserLastName = g.Key.LastName,
                                    GiftId = g.Key.GiftId,
                                    GiftName = g.Key.Name,
                                    Count = g.Count()
                                }).ToList();
                Dictionary<string, int> dict = new();

                foreach (var item in groupCards)
                {
                    dict.Add(item.UserFirstName + " " + item.UserLastName, item.Count);
                }
                return groupCards.Select(x => new CardDto
                {
                    GiftId = x.GiftId,
                    GiftName = x.GiftName,
                    CardPurchases = dict,
                    WinnerName = winnerName
                }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get card by gift id {GiftId}.", id);
                throw;
            }
        }

        public async Task<bool> ResetWinnersByLotteryId(int lotteryId)
        {
            try
            {
                return await _repository.ResetWinnersByLotteryId(lotteryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset winners for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        private UserDto MapUser(User user)
        {
            return _mapper.Map<UserDto>(user);
        }
    }
}
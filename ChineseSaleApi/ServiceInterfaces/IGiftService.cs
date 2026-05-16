using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface IGiftService
    {
        Task<int> AddGift(CreateGiftDto createGiftDto);
        Task DeleteGift(int id);
        Task<GiftDto?> GetGiftById(int id);
        //Task<PaginatedResultDto<GiftDto>> GetGiftsWithPagination(int lotteryId, PaginationParamsDto paginationParams);
        Task<IEnumerable<GiftWithOldPurchaseDto>> GetAllGifts(int lotteryId, int userId);
        Task<bool?> UpdateGift(UpdateGiftDto updateGiftDto);
        Task<PaginatedResultDto<GiftWithOldPurchaseDto>> GetGiftsByUserWithPagination(int lotteryId, int? userId, PaginationParamsDto paginationParams,dynamic request);
        Task<PaginatedResultDto<GiftWithOldPurchaseDto>> GetGiftsSearchPagination(int lotteryId, int? userId, PaginationParamsDto paginationParams, string? textSearch, string? type, string? sortType, bool? ascendingOrder,int? categoryId);
        Task<UpdateGiftDto> GetGiftsByIdUpdate(int id);
        Task<int> GetGiftCountByLotteryId(int lotteryId);
    }
}
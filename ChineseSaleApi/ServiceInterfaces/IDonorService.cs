using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface IDonorService
    {
        Task<int> AddDonor(CreateDonorDto donorDto);
        Task<IEnumerable<DonorDto>> GetAllDonors();
        Task<PaginatedResultDto<DonorDto>> GetDonorsWithPagination(int lottery, PaginationParamsDto paginationParams);
        Task<IEnumerable<DonorDto?>> GetDonorByLotteryId(int lottery);
        Task<SingelDonorDto?> GetDonorById(int id, int lottery, PaginationParamsDto paginationParamsdto);
        Task<DonorDto?> GetDonorByIdSimple(int id, int lotteryId);
        Task<PaginatedResultDto<DonorDto>> GetDonorsNameSearchedPagination(int lottery, PaginationParamsDto paginationParams, string textSearch);
        Task<PaginatedResultDto<DonorDto>> GetDonorsEmailSearchedPagination(int lottery, PaginationParamsDto paginationParams, string textSearch);
        Task<int> GetDonorCountByLotteryId(int lotteryId);

        Task<bool?> AddLotteryToDonor(int donorId, int lotteryId);
        Task<bool?> UpdateDonor(UpdateDonorDto donor);
        Task<bool?> DeleteDonor(int id, int lotteryId);
    }
}
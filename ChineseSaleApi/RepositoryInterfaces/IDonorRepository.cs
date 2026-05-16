using ChineseSaleApi.Models;

namespace ChineseSaleApi.RepositoryInterfaces
{
    public interface IDonorRepository
    {
        Task<int> AddDonor(Donor donor);
        Task<IEnumerable<Donor>> GetAllDonors();
        Task<(IEnumerable<Donor> items, int totalcount)> GetDonorsWithPagination(int lottery, int pageNumber, int pageSize);
        Task<(IEnumerable<Donor> items, int totalcount)> GetDonorsNameSearchedPagination(int lottery, int pageNumber, int pageSize, string textSearch);
        Task<(IEnumerable<Donor> items, int totalcount)> GetDonorsEmailSearchedPagination(int lottery, int pageNumber, int pageSize, string textSearch);
        Task<Donor?> GetDonorById(int id);
        Task UpdateDonor(Donor donor);
        Task<bool?> UpdateLotteryDonor(int donorId, int lotteryId);
        Task<IEnumerable<Donor>> GetDonorByLotteryId(int lottery);
        Task<int> GetDonorCountByLotteryId(int lotteryId);
        Task<bool?> DeleteDonor(int id);
        Task<bool?> DeleteLotteryDonor(int donorId, int lotteryId);
    }
}
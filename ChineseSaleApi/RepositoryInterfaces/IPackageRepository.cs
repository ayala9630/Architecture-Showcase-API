using ChineseSaleApi.Models;

namespace ChineseSaleApi.RepositoryInterfaces
{
    public interface IPackageRepository
    {
        Task<int> AddPackage(Package package);
        Task DeletePackage(int id);
        Task<IEnumerable<Package>> GetAllPackages(int lotteryId);
        Task<Package?> GetPackageById(int id);
        Task UpdatePackage(Package package);
    }
}
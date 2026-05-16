using ChineseSaleApi.Models;

namespace ChineseSaleApi.RepositoryInterfaces
{
    public interface IPackageCartRepository
    {
        Task<int> AddPackageCart(PackageCart packageCart);
        Task DeletePackageCart(int id);
        Task<IEnumerable<PackageCart>> GetAllPackageCarts();
        Task<PackageCart?> GetPackageCartById(int id);
        Task<IEnumerable<PackageCart>> GetPackagesByUserId(int userId);
        Task UpdatePackageCart(PackageCart packageCart);
    }
}
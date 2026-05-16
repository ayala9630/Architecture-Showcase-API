using ChineseSaleApi.Dto;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface IPackageCartService
    {
        Task<int> CreatePackageCart(CreatePackageCartDto packageCartDto);
        Task<IEnumerable<PackageCartDto>> GetPackagesByUserId(int userId);
        Task<bool?> UpdatePackageCart(PackageCartDto packageCartDto);
        Task DeletePackageCart(int id);
    }
}
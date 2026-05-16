using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface IPackageService
    {
        Task<int> AddPackage(CreatePackageDto createPackageDto);
        Task DeletePackage(int id);
        Task<List<PackageDto>> GetAllPackages(int lotteryId);
        Task<PackageDto?> GetPackageById(int id);
        Task<bool?> UpdatePackage(UpdatePackageDto packageDto);
        Task<UpdatePackageDto> GetPackageByIdUpdate(int id);
    }

}
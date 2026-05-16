using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;

namespace ChineseSaleApi.Mappings
{
    public class PackageCartProfile : Profile
    {
        public PackageCartProfile()
        {
            CreateMap<PackageCart, PackageCartDto>();
            CreateMap<CreatePackageCartDto, PackageCart>();
        }
    }
}

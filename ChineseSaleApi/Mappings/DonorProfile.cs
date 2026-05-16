using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;

namespace ChineseSaleApi.Mappings
{
    public class DonorProfile : Profile
    {
        public DonorProfile()
        {
            CreateMap<Donor, DonorDto>();
            CreateMap<Donor, SingelDonorDto>()
                .ForMember(dest => dest.Gifts, opt => opt.Ignore());

            CreateMap<CreateDonorDto, Donor>()
                .ForMember(dest => dest.CompanyAddressId, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyAddress, opt => opt.Ignore());

            CreateMap<UpdateDonorDto, Donor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyAddress, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

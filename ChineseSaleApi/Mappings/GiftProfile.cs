using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;

namespace ChineseSaleApi.Mappings
{
    public class GiftProfile : Profile
    {
        public GiftProfile()
        {
            CreateMap<Gift, GiftDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Donor != null ? src.Donor.CompanyName : string.Empty))
                .ForMember(dest => dest.CompanyLogoUrl, opt => opt.MapFrom(src => src.Donor != null ? src.Donor.CompanyIcon : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.winner, opt => opt.Ignore());

            CreateMap<Gift, GiftWithOldPurchaseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.OldPurchaseCount, opt => opt.Ignore())
                .ForMember(dest => dest.winner, opt => opt.Ignore());

            CreateMap<Gift, UpdateGiftDto>();
            CreateMap<CreateGiftDto, Gift>();
            CreateMap<UpdateGiftDto, Gift>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LotteryId, opt => opt.Condition(src => src.LotteryId != 0))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

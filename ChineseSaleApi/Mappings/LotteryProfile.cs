using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;

namespace ChineseSaleApi.Mappings
{
    public class LotteryProfile : Profile
    {
        public LotteryProfile()
        {
            CreateMap<Lottery, LotteryDto>();
            CreateMap<CreateLotteryDto, Lottery>();
            CreateMap<UpdateLotteryDto, Lottery>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

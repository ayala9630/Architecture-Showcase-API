using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;

namespace ChineseSaleApi.Mappings
{
    public class CardCartProfile : Profile
    {
        public CardCartProfile()
        {
            CreateMap<CardCart, CardCartDto>();
            CreateMap<CreateCardCartDto, CardCart>();
        }
    }
}

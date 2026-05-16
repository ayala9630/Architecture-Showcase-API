using ChineseSaleApi.Dto;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface IAddressService
    {
        Task<int> AddAddressForDonor(CreateAddressForDonorDto address);
        Task<int> AddAddressForUser(CreateAddressForUserDto address);
        Task<AddressDto?> GetAddressById(int id);
        Task<bool?> UpdateAddress(AddressDto addressDto);
    }
}
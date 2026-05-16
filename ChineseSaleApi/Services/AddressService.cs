using System;
using System.Threading.Tasks;
using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddressService> _logger;

        public AddressService(IAddressRepository repository, IMapper mapper, ILogger<AddressService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        //create
        public async Task<int> AddAddressForUser(CreateAddressForUserDto address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            try
            {
                Address addrress2 = _mapper.Map<Address>(address);

                return await _repository.AddAddress(addrress2);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "AddAddressForUser received a null argument.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding an address for user.");
                throw;
            }
        }

        public async Task<int> AddAddressForDonor(CreateAddressForDonorDto address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            try
            {
                Address addrress2 = _mapper.Map<Address>(address);

                return await _repository.AddAddress(addrress2);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "AddAddressForDonor received a null argument.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding an address for donor.");
                throw;
            }
        }

        //read
        public async Task<AddressDto?> GetAddressById(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));

            try
            {
                var address = await _repository.GetAddress(id);
                if (address == null)
                {
                    return null;
                }

                return _mapper.Map<AddressDto>(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get address by id {AddressId}.", id);
                throw;
            }
        }

        //update
        public async Task<bool?> UpdateAddress(AddressDto addressDto)
        {
            if (addressDto == null) throw new ArgumentNullException(nameof(addressDto));

            try
            {
                var address = await _repository.GetAddress(addressDto.Id);
                if (address != null)
                {
                    _mapper.Map(addressDto, address);
                    await _repository.UpdateAddress(address);
                    return true;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update address {AddressId}.", addressDto.Id);
                throw;
            }
        }
    }
}

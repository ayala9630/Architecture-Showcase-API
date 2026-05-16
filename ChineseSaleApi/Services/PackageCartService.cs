using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services
{
    public class PackageCartService : IPackageCartService
    {
        private readonly IPackageCartRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<PackageCartService> _logger;

        public PackageCartService(IPackageCartRepository repository, IMapper mapper, ILogger<PackageCartService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        //create
        public async Task<int> CreatePackageCart(CreatePackageCartDto packageCartDto)
        {
            try
            {
                PackageCart packageCart = _mapper.Map<PackageCart>(packageCartDto);
                return await _repository.AddPackageCart(packageCart);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "CreatePackageCart received a null argument: {@PackageCartDto}", packageCartDto);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a package cart for user {UserId}.", packageCartDto?.UserId);
                throw;
            }
        }

        //read
        public async Task<IEnumerable<PackageCartDto>> GetPackagesByUserId(int userId)
        {
            try
            {
                var packageCarts = await _repository.GetPackagesByUserId(userId);
                return packageCarts.Select(pc => _mapper.Map<PackageCartDto>(pc));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get package carts for user {UserId}.", userId);
                throw;
            }
        }

        //update
        public async Task<bool?> UpdatePackageCart(PackageCartDto packageCartDto)
        {
            try
            {
                PackageCart? packageCart = await _repository.GetPackageCartById(packageCartDto.Id);
                if (packageCart != null)
                {
                    packageCart.Quantity = packageCartDto.Quantity;
                    await _repository.UpdatePackageCart(packageCart);
                    return true;
                }
                return null;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "UpdatePackageCart received a null argument: {@PackageCartDto}", packageCartDto);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update package cart {PackageCartId}.", packageCartDto?.Id);
                throw;
            }
        }

        //delete
        public async Task DeletePackageCart(int id)
        {
            try
            {
                await _repository.DeletePackageCart(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete package cart {PackageCartId}.", id);
                throw;
            }
        }
    }
}
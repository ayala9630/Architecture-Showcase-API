using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services
{
    public class DonorService : IDonorService
    {
        private readonly IDonorRepository _repository;
        private readonly IAddressService _addressService;
        private readonly ICardRepository _cardRepository;
        private readonly ICardService _cardService;
        private readonly IMapper _mapper;
        private readonly ILogger<DonorService> _logger;

        public DonorService(IDonorRepository repository, IAddressService addressService, ICardRepository cardRepository, ICardService cardService, IMapper mapper, ILogger<DonorService> logger)
        {
            _repository = repository;
            _addressService = addressService;
            _cardRepository = cardRepository;
            _cardService = cardService;
            _mapper = mapper;
            _logger = logger;
        }

        //create
        public async Task<int> AddDonor(CreateDonorDto donorDto)
        {
            if (donorDto == null) throw new ArgumentNullException(nameof(donorDto));
            if (string.IsNullOrWhiteSpace(donorDto.CompanyName)) throw new ArgumentException("CompanyName is required.", nameof(donorDto.CompanyName));
            if (string.IsNullOrWhiteSpace(donorDto.CompanyEmail)) throw new ArgumentException("CompanyEmail is required.", nameof(donorDto.CompanyEmail));

            try
            {
                var idAddress = await _addressService.AddAddressForDonor(donorDto.CompanyAddress);
                Donor donor = _mapper.Map<Donor>(donorDto);
                donor.CompanyAddressId = idAddress;
                return await _repository.AddDonor(donor);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "AddDonor received a null argument: {@DonorDto}", donorDto);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while adding donor: {@DonorDto}", donorDto);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding a donor.");
                throw;
            }
        }

        //read
        public async Task<SingelDonorDto?> GetDonorById(int id, int lotteryId, PaginationParamsDto paginationParamsdto)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));
            if (lotteryId <= 0) throw new ArgumentException("lotteryId must be greater than zero.", nameof(lotteryId));

            try
            {
                var donor = await _repository.GetDonorById(id);
                if (donor == null)
                {
                    return null;
                }

                var cards = await GetAllCards_Fallback(lotteryId);

                Dictionary<string, int> dict = new();

                var donorGifts = donor.Gifts?.Where(g => g.DonorId == id && g.LotteryId == lotteryId) ?? Enumerable.Empty<Gift>();

                foreach (var gift in donorGifts)
                {
                    var count = cards.Count(c => c.GiftId == gift.Id);
                    dict[gift.Name ?? string.Empty] = count;
                }

                var dto = _mapper.Map<SingelDonorDto>(donor);
                dto.Gifts = dict;
                return dto;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while getting donor by id {DonorId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donor by id {DonorId} for lottery {LotteryId}.", id, lotteryId);
                throw;
            }
        }
        public async Task<DonorDto?> GetDonorByIdSimple(int id, int lotteryId)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));
            if (lotteryId <= 0) throw new ArgumentException("lotteryId must be greater than zero.", nameof(lotteryId));

            try
            {
                var donor = await _repository.GetDonorById(id);
                if (donor == null)
                {
                    return null;
                }
                return _mapper.Map<DonorDto>(donor);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for GetDonorByIdSimple: {DonorId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donor by id {DonorId} for lottery {LotteryId}.", id, lotteryId);
                throw;
            }
        }
        private async Task<IEnumerable<Models.Card>> GetAllCards_Fallback(int lotteryId)
        {
            try
            {
                return await _cardRepository.GetAllCards(lotteryId);
            }
            catch
            {
                return Enumerable.Empty<Models.Card>();
            }
        }

        //by lotterry id
        public async Task<IEnumerable<DonorDto?>> GetDonorByLotteryId(int lottery)
        {
            if (lottery <= 0) throw new ArgumentException("Lottery id must be greater than zero.", nameof(lottery));

            try
            {
                var donors = await _repository.GetDonorByLotteryId(lottery);
                return donors.Select(donor => _mapper.Map<DonorDto>(donor));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for GetDonorByLotteryId: {LotteryId}.", lottery);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donors by lottery id {LotteryId}.", lottery);
                throw;
            }
        }

        public async Task<int> GetDonorCountByLotteryId(int lotteryId)
        {
            try
            {
                return await _repository.GetDonorCountByLotteryId(lotteryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donor count for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        //all
        public async Task<IEnumerable<DonorDto>> GetAllDonors()
        {
            try
            {
                var donors = await _repository.GetAllDonors();
                return donors.Select(donor => _mapper.Map<DonorDto>(donor));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all donors.");
                throw;
            }
        }

        //with pagination
        public async Task<PaginatedResultDto<DonorDto>> GetDonorsWithPagination(int lottery, PaginationParamsDto paginationParams)
        {
            if (paginationParams == null) throw new ArgumentNullException(nameof(paginationParams));
            if (lottery <= 0) throw new ArgumentException("Lottery id must be greater than zero.", nameof(lottery));

            try
            {
                var (donors, totalCount) = await _repository.GetDonorsWithPagination(lottery, paginationParams.PageNumber, paginationParams.PageSize);
                var donorDtos = donors.Select(donor => _mapper.Map<DonorDto>(donor));
                return new PaginatedResultDto<DonorDto>
                {
                    Items = donorDtos,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Pagination parameters null for lottery {LotteryId}.", lottery);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donors with pagination for lottery {LotteryId}.", lottery);
                throw;
            }
        }

        public async Task<PaginatedResultDto<DonorDto>> GetDonorsNameSearchedPagination(int lottery, PaginationParamsDto paginationParams, string textSearch)
        {
            if (paginationParams == null) throw new ArgumentNullException(nameof(paginationParams));
            if (lottery <= 0) throw new ArgumentException("Lottery id must be greater than zero.", nameof(lottery));

            try
            {
                var (donors, totalCount) = await _repository.GetDonorsNameSearchedPagination(lottery, paginationParams.PageNumber, paginationParams.PageSize, textSearch);
                var donorDtos = donors.Select(donor => _mapper.Map<DonorDto>(donor));
                return new PaginatedResultDto<DonorDto>
                {
                    Items = donorDtos,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Pagination parameters null for lottery {LotteryId}.", lottery);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donors by name search for lottery {LotteryId}.", lottery);
                throw;
            }
        }
        public async Task<PaginatedResultDto<DonorDto>> GetDonorsEmailSearchedPagination(int lottery, PaginationParamsDto paginationParams, string textSearch)
        {
            if (paginationParams == null) throw new ArgumentNullException(nameof(paginationParams));
            if (lottery <= 0) throw new ArgumentException("Lottery id must be greater than zero.", nameof(lottery));

            try
            {
                var (donors, totalCount) = await _repository.GetDonorsEmailSearchedPagination(lottery, paginationParams.PageNumber, paginationParams.PageSize, textSearch);
                var donorDtos = donors.Select(donor => new DonorDto
                {
                    Id = donor.Id,
                    FirstName = donor.FirstName,
                    LastName = donor.LastName,
                    CompanyName = donor.CompanyName,
                    CompanyEmail = donor.CompanyEmail,
                    CompanyPhone = donor.CompanyPhone,
                    CompanyIcon = donor.CompanyIcon,
                    CompanyAddressId = donor.CompanyAddressId
                });
                return new PaginatedResultDto<DonorDto>
                {
                    Items = donorDtos,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Pagination parameters null for lottery {LotteryId}.", lottery);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donors by email search for lottery {LotteryId}.", lottery);
                throw;
            }
        }

        //update
        public async Task<bool?> UpdateDonor(UpdateDonorDto donor)
        {
            if (donor == null) throw new ArgumentNullException(nameof(donor));
            if (donor.Id <= 0) throw new ArgumentException("Donor Id must be greater than zero.", nameof(donor.Id));

            try
            {
                Donor? donor1 = await _repository.GetDonorById(donor.Id);
                if (donor1 == null)
                {
                    return null;
                }
                _mapper.Map(donor, donor1);
                await _repository.UpdateDonor(donor1);
                return true;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "UpdateDonor called with null payload.");
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed updating donor {DonorId}.", donor?.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update donor {DonorId}.", donor?.Id);
                throw;
            }
        }
        //add lottery to donor
        public async Task<bool?> AddLotteryToDonor(int donorId, int lotteryId)
        {
            if (donorId <= 0) throw new ArgumentException("DonorId must be greater than zero.", nameof(donorId));
            if (lotteryId <= 0) throw new ArgumentException("LotteryId must be greater than zero.", nameof(lotteryId));

            try
            {
                return await _repository.UpdateLotteryDonor(donorId, lotteryId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while adding lottery {LotteryId} to donor {DonorId}.", lotteryId, donorId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add lottery {LotteryId} to donor {DonorId}.", lotteryId, donorId);
                throw;
            }
        }
        //delete
        public async Task<bool?> DeleteDonor(int id, int lotteryId)
        {
            if (id <= 0) throw new ArgumentException("Donor Id must be greater than zero.", nameof(id));
            if (lotteryId <= 0) throw new ArgumentException("LotteryId must be greater than zero.", nameof(lotteryId));

            try
            {
                Donor? donor = await _repository.GetDonorById(id);
                if (donor == null)
                {
                    return null;
                }
                if (donor.Lotteries.Count() > 1)
                    return await _repository.DeleteLotteryDonor(id, lotteryId);
                return await _repository.DeleteDonor(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while deleting donor {DonorId} from lottery {LotteryId}.", id, lotteryId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete donor {DonorId} from lottery {LotteryId}.", id, lotteryId);
                throw;
            }
        }
    }
}

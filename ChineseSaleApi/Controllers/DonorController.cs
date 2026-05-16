using ChineseSaleApi.Attributes;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using ChineseSaleApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Admin]
    public class DonorController : ControllerBase
    {
        private readonly IDonorService _service;
        private readonly ILotteryService _lotteryService;
        private readonly ILogger<DonorController> _logger;

        public DonorController(IDonorService service, ILotteryService lotteryService, ILogger<DonorController> logger)
        {
            _service = service;
            _lotteryService = lotteryService;
            _logger = logger;
        }

        //read
        [HttpGet]
        public async Task<IActionResult> GetAllDonors()
        {
            try
            {
                var donors = await _service.GetAllDonors();
                return Ok(donors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all donors.");
                return StatusCode(500, "An unexpected error occurred while retrieving donors.");
            }
        }

        [HttpGet("{lotteryId}/{id}")]
        public async Task<IActionResult> GetDonorById(int id, int lotteryId, [FromQuery] PaginationParamsDto paginationParamsdto)
        {
            try
            {
                var donor = await _service.GetDonorById(id, lotteryId, paginationParamsdto);
                if (donor == null)
                {
                    return NotFound();
                }
                return Ok(donor);
            }
            catch (Exception ex)    
            {
                _logger.LogError(ex, "Failed to get donor {DonorId} for lottery {LotteryId}.", id, lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving donor details.");
            }
        }

        [HttpGet("lottery:{lotteryId}/{id}")]
        public async Task<IActionResult> GetDonorByIdSimple(int id, int lotteryId)
        {
            try
            {
                var donor = await _service.GetDonorByIdSimple(id, lotteryId);
                if (donor == null)
                {
                    return NotFound();
                }
                return Ok(donor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donor {DonorId} for lottery {LotteryId}.", id, lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving donor details.");
            }
        }
            [HttpGet("{lotteryId}")]
        public async Task<IActionResult> GetDonorByLotteryId(int lotteryId)
        {
            try
            {
                if (await _lotteryService.GetLotteryById(lotteryId) == null)
                    return NotFound();
                var donors = await _service.GetDonorByLotteryId(lotteryId);
                return Ok(donors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donors by lottery id {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving donors.");
            }
        }

        [HttpGet("lottery/{lotteryId}/pagination")]
        public async Task<IActionResult> GetDonorsWithPagination(int lotteryId, [FromQuery] PaginationParamsDto paginationParamsDto)
        {
            try
            {
                var pagedDonors = await _service.GetDonorsWithPagination(lotteryId, paginationParamsDto);
                return Ok(pagedDonors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paginated donors for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving paginated donors.");
            }
        }
        [AllowAnonymous]
        [HttpGet("lottery/{lotteryId}/count")]
        public async Task<IActionResult> GetDonorCountByLotteryId(int lotteryId)
        {
            try
            {
                if (await _lotteryService.GetLotteryById(lotteryId) == null)
                    return NotFound();
                var count = await _service.GetDonorCountByLotteryId(lotteryId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get donor count for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving donor count.");
            }
        }

        [HttpGet("lottery/{lotteryId}/search")]
        public async Task<IActionResult> GetDonorSearchedPagination(int lotteryId, [FromQuery] PaginationParamsDto paginationParams, [FromQuery] string? name, [FromQuery] string? email)
        {
            try
            {
                if (name != null)
                {
                    var pagedDonorsName = await _service.GetDonorsNameSearchedPagination(lotteryId, paginationParams, name);
                    return Ok(pagedDonorsName);
                }
                else if (email != null)
                {
                    var pagedDonorsEmail = await _service.GetDonorsEmailSearchedPagination(lotteryId, paginationParams, email);
                    return Ok(pagedDonorsEmail);
                }
                var pagedDonors = await _service.GetDonorsWithPagination(lotteryId, paginationParams);
                return Ok(pagedDonors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search donors for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while searching donors.");
            }
        }

        //create
        [HttpPost]
        public async Task<IActionResult> AddDonor([FromBody] CreateDonorDto donorDto)
        {
            try
            {
                var donorId = await _service.AddDonor(donorDto);
                return Ok(donorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add donor.");
                return StatusCode(500, "An unexpected error occurred while adding the donor.");
            }
        }

        //update
        [HttpPut]
        public async Task<IActionResult> UpdateDonor([FromBody] UpdateDonorDto donor)
        {
            try
            {
                bool? success = await _service.UpdateDonor(donor);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update donor {DonorId}.", donor?.Id);
                return StatusCode(500, "An unexpected error occurred while updating the donor.");
            }
        }

        //update lottery donor
        [HttpPut("lottery/{lotteryId}/id/{donorId}")]
        public async Task<IActionResult> AddLotteryToDonor(int donorId, int lotteryId)
        {
            try
            {
                bool? success = await _service.AddLotteryToDonor(donorId, lotteryId);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add lottery {LotteryId} to donor {DonorId}.", lotteryId, donorId);
                return StatusCode(500, "An unexpected error occurred while updating donor lotteries.");
            }
        }

        //delete
        [HttpDelete("lottery/{lotteryId}/id/{id}")]
        public async Task<IActionResult> DeleteDonor(int id, int lotteryId)
        {
            try
            {
                if (await _service.DeleteDonor(id, lotteryId) == null)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete donor {DonorId} from lottery {LotteryId}.", id, lotteryId);
                return StatusCode(500, "An unexpected error occurred while deleting the donor.");
            }
        }
    }
}
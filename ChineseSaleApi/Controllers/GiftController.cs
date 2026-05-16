using Microsoft.AspNetCore.Mvc;
using ChineseSaleApi.Dto;
using ChineseSaleApi.ServiceInterfaces;
using ChineseSaleApi.Models;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Authorization;
using ChineseSaleApi.Attributes;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class GiftController : ControllerBase
    {
        private readonly IGiftService _service;
        private readonly ILogger<GiftController> _logger;

        public GiftController(IGiftService service, ILogger<GiftController> logger)
        {
            _service = service;
            _logger = logger;
        }

        //read
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGiftById(int id)
        {
            try
            {
                var gift = await _service.GetGiftById(id);
                if (gift == null)
                {
                    return NotFound();
                }
                return Ok(gift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gift by id {GiftId}.", id);
                throw;
            }
        }
        [Authorize]
        [Admin]
        [HttpGet("id/{id}/update")]
        public async Task<IActionResult> GetGiftsByIdUpdate(int id)
        {
            try
            {
                var gifts = await _service.GetGiftsByIdUpdate(id);
                return Ok(gifts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gifts for update by lottery {LotteryId}.", id);
                return StatusCode(500, "An unexpected error occurred while retrieving the gifts for update.");
            }
        }
        [HttpGet("user/{userId}/lottery/{lotteryId}")]
        public async Task<IActionResult> GetAllGifts(int lotteryId, int userId)
        {
            try
            {
                var gifts = await _service.GetAllGifts(lotteryId, userId);
                return Ok(gifts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gifts for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        [HttpGet("lottery/{lotteryId}/pagination")]
        public async Task<IActionResult> GetGiftsByUserWithPagination(int lotteryId, [FromQuery] int? userId, [FromQuery] PaginationParamsDto paginationParams)
        {
            try
            {
                var pagedGifts = await _service.GetGiftsByUserWithPagination(lotteryId, userId, paginationParams,Request);
                return Ok(pagedGifts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paginated gifts for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }
        [HttpGet("lottery/{lotteryId}/count")]
        public async Task<IActionResult> GetGiftCountByLotteryId(int lotteryId)
        {
            try
            {
                var count = await _service.GetGiftCountByLotteryId(lotteryId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gift count for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving gift count.");
            }
        }

        [HttpGet("lottery/{lotteryId}/search-pagination")]
        public async Task<IActionResult> GetGiftsSearchPagination(int lotteryId, [FromQuery] int? userId, [FromQuery] PaginationParamsDto? paginationParams, [FromQuery] string? name, [FromQuery] string? donor, [FromQuery]  string? sortType, [FromQuery] bool? ascendingOrder, [FromQuery] int? categoryId)
        {
            try
            {
                if(paginationParams == null)
                {
                    var defaultPagination = new PaginationParamsDto
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                    paginationParams = defaultPagination;
                }
                if (name != null)
                {
                    var pagedGiftsName = await _service.GetGiftsSearchPagination(lotteryId, userId, paginationParams, name, "name",sortType,ascendingOrder,categoryId);
                    return Ok(pagedGiftsName);
                }
                else if (donor != null)
                {
                    var pagedGiftsDonor = await _service.GetGiftsSearchPagination(lotteryId, userId, paginationParams, donor, "donor",sortType,ascendingOrder,categoryId);
                    return Ok(pagedGiftsDonor);
                }
                else
                {
                    var pagedGifts = await _service.GetGiftsSearchPagination(lotteryId, userId, paginationParams, null, null,sortType,ascendingOrder,categoryId);
                    return Ok(pagedGifts);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search gifts for lottery {LotteryId}.", lotteryId);
                throw;
            }
        }

        //create
        [Authorize]
        [Admin]
        [HttpPost]
        public async Task<IActionResult> AddGift([FromBody] CreateGiftDto gift)
        {
            try
            {
                var createdGiftId = await _service.AddGift(gift);
                return CreatedAtAction(nameof(GetGiftById), new { id = createdGiftId }, gift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add gift for lottery {LotteryId}.", gift?.LotteryId);
                throw;
            }
        }

        //update
        [Authorize]
        [Admin]
        [HttpPut]
        public async Task<IActionResult> UpdateGift([FromBody] UpdateGiftDto gift)
        {
            try
            {
                var success = await _service.UpdateGift(gift);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update gift {GiftId}.", gift?.Id);
                throw;
            }
        }

        //delete
        [Authorize]
        [Admin]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGift(int id)
        {
            try
            {
                await _service.DeleteGift(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete gift {GiftId}.", id);
                throw;
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using ChineseSaleApi.ServiceInterfaces;
using ChineseSaleApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using ChineseSaleApi.Attributes;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Admin]

    public class CardController : ControllerBase
    {
        private readonly ICardService _service;
        private readonly ILogger<CardController> _logger;

        public CardController(ICardService service, ILogger<CardController> logger)
        {
            _service = service;
            _logger = logger;
        }

        //read
        [HttpGet("lottery/{lotteryId}")]
        public async Task<IActionResult> Get(int lotteryId)
        {
            try
            {
                var cards = await _service.GetAllCarsds(lotteryId);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cards for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving cards.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCardByGiftId(int id)
        {
            try
            {
                var card = await _service.GetCardByGiftId(id);
                if (card == null)
                {
                    return NotFound();
                }
                return Ok(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get card by gift id {GiftId}.", id);
                return StatusCode(500, "An unexpected error occurred while retrieving the card.");
            }
        }

        [HttpGet("pagination/{lotteryId}")]
        public async Task<IActionResult> GetCardsWithPagination(int lotteryId, [FromQuery] PaginationParamsDto paginationParams)
        {
            try
            {
                var cards = await _service.GetCardsWithPagination(lotteryId, paginationParams);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paginated cards for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving paginated cards.");
            }
        }

        [HttpGet("pagination/sorted/{lotteryId}")]
        public async Task<IActionResult> GetCardsWithPaginationSorted(int lotteryId, [FromQuery] PaginationParamsDto paginationParams,[FromQuery]string? sortType, [FromQuery] bool ascending)
        {
            try
            {
                var cards = await _service.GetCardsWithPaginationSorted(lotteryId, paginationParams,sortType, ascending);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paginated cards sorted by value for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while retrieving sorted cards.");
            }
        }

        //[HttpGet("pagination/sortByPurchases/{lotteryId}")]
        //public async Task<IActionResult> GetCardsWithPaginationSortByPurchases(int lotteryId, [FromQuery] PaginationParamsDto paginationParams, [FromQuery] bool ascending)
        //{
        //    try
        //    {
        //        var cards = await _service.GetCardsWithPaginationSortByPurchases(lotteryId, paginationParams, ascending);
        //        return Ok(cards);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to get paginated cards sorted by purchases for lottery {LotteryId}.", lotteryId);
        //        return StatusCode(500, "An unexpected error occurred while retrieving sorted cards.");
        //    }
        //}

        //create
        [HttpPost]
        public async Task<IActionResult> CreateCard([FromBody] CreateCardDto createCardDto)
        {
            try
            {
                var id = await _service.AddCard(createCardDto);
                return CreatedAtAction(nameof(CreateCard), new { id = id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create card for gift {GiftId}.", createCardDto?.GiftId);
                return StatusCode(500, ex+"\nAn unexpected error occurred while creating the card.");
            }
        }

        [HttpPut("resetWinners")]
        public async Task<IActionResult> ResetWinnersByLotteryId(int lotteryId)
        {
            try
            {
                await _service.ResetWinnersByLotteryId(lotteryId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset winners for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, "An unexpected error occurred while resetting winners.");
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using ChineseSaleApi.Dto;
using ChineseSaleApi.ServiceInterfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardCartController : ControllerBase
    {
        private readonly ICardCartService _service;
        private readonly ILogger<CardCartController> _logger;

        public CardCartController(ICardCartService service, ILogger<CardCartController> logger)
        {
            _service = service;
            _logger = logger;
        }

        //read
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCardCartsByUserId([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest("userId must be greater than zero.");

            try
            {
                var cards = await _service.GetCardCartsByUserId(userId);
                // Policy A: return 200 with empty array when no items
                return Ok(cards ?? Enumerable.Empty<CardCartDto>());
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while getting card carts for user {UserId}.", userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get card carts for user {UserId}.", userId);
                throw;
            }
        }

        //create
        [HttpPost]
        public async Task<IActionResult> CreateCardCart([FromBody] CreateCardCartDto cardCartDto)
        {
            if (cardCartDto == null)
                return BadRequest("Card cart data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _service.CreateCardCar(cardCartDto);
                return CreatedAtAction(nameof(GetCardCartsByUserId), new { userId = cardCartDto.UserId }, id);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "CreateCardCart called with null payload.");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while creating card cart: {@CardCartDto}.", cardCartDto);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create card cart for user {UserId}.", cardCartDto?.UserId);
                throw;
            }
        }

        //update
        [HttpPut]
        public async Task<IActionResult> UpdateCardCart([FromBody] UpdateQuantityDto cardCartDto)
        {
            if (cardCartDto == null)
                return BadRequest("Card cart data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _service.UpdateCardCart(cardCartDto);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "UpdateCardCart called with null payload.");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while updating card cart: {@CardCartDto}.", cardCartDto);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update card cart {CardCartId}.", cardCartDto?.Id);
                throw;
            }
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCardCart(int id)
        {
            if (id <= 0)
                return BadRequest("id must be greater than zero.");

            try
            {
                await _service.DeleteCardCart(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid id provided for DeleteCardCart: {CardCartId}.", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete card cart {CardCartId}.", id);
                throw;
            }
        }
    }
}
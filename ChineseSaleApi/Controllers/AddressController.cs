using Microsoft.AspNetCore.Mvc;
using ChineseSaleApi.ServiceInterfaces;
using ChineseSaleApi.Dto;
using Microsoft.Extensions.Logging;
using System;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AddressController : ControllerBase
    {
        private readonly IAddressService _service;
        private readonly ILogger<AddressController> _logger;

        public AddressController(IAddressService service, ILogger<AddressController> logger)
        {
            _service = service;
            _logger = logger;
        }

        //read
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            if (id <= 0) return BadRequest("id must be greater than zero.");

            try
            {
                var address = await _service.GetAddressById(id);
                if (address == null)
                    return NotFound();
                return Ok(address);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when getting address by id {AddressId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get address by id {AddressId}.", id);
                throw;
            }
        }

        //create
        [HttpPost("user")]
        public async Task<IActionResult> AddAddressForUser([FromBody] CreateAddressForUserDto addressDto)
        {
            if (addressDto == null)
                return BadRequest("Address data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var addressId = await _service.AddAddressForUser(addressDto);
                return CreatedAtAction(nameof(GetAddressById), new { id = addressId }, addressId);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "AddAddressForUser called with null payload.");
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in AddAddressForUser.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add address for user.");
                throw;
            }
        }

        [HttpPost("donor")]
        public async Task<IActionResult> AddAddressForDonor([FromBody] CreateAddressForDonorDto addressDto)
        {
            if (addressDto == null)
                return BadRequest("Address data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var addressId = await _service.AddAddressForDonor(addressDto);
                return CreatedAtAction(nameof(GetAddressById), new { id = addressId }, addressId);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "AddAddressForDonor called with null payload.");
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in AddAddressForDonor.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add address for donor.");
                throw;
            }
        }

        //update
        [HttpPut]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressDto addressDto)
        {
            if (addressDto == null)
                return BadRequest("Address data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                bool? success = await _service.UpdateAddress(addressDto);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "UpdateAddress called with null payload.");
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in UpdateAddress.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update address {AddressId}.", addressDto.Id);
                throw;
            }
        }
    }
}

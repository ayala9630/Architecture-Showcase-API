using Microsoft.AspNetCore.Mvc;
using ChineseSaleApi.Dto;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackageCartController : ControllerBase
    {
        private readonly IPackageCartService _service;
        private readonly ILogger<PackageCartController> _logger;

        public PackageCartController(IPackageCartService service, ILogger<PackageCartController> logger)
        {
            _service = service;
            _logger = logger;
        }

        //read
        [HttpGet]
        public async Task<IActionResult> GetPackageCartByUserId(int userId)
        {
            try
            {
                var packages = await _service.GetPackagesByUserId(userId);
                if (packages == null)
                    return NotFound();
                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get package carts for user {UserId}.", userId);
                return StatusCode(500, "An unexpected error occurred while retrieving package carts.");
            }
        }

        //create
        [HttpPost]
        public async Task<IActionResult> CreatePackageCart([FromBody] CreatePackageCartDto packageCartDto)
        {
            try
            {
                var id = await _service.CreatePackageCart(packageCartDto);
                return CreatedAtAction(nameof(CreatePackageCart), new { id = id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create package cart for user {UserId}.", packageCartDto?.UserId);
                return StatusCode(500, "An unexpected error occurred while creating the package cart.");
            }
        }

        //update
        [HttpPut]
        public async Task<IActionResult> UpdatePackageCart([FromBody] PackageCartDto packageCartDto)
        {
            try
            {
                var success = await _service.UpdatePackageCart(packageCartDto);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update package cart {PackageCartId}.", packageCartDto?.Id);
                return StatusCode(500, "An unexpected error occurred while updating the package cart.");
            }
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackageCart(int id)
        {
            try
            {
                await _service.DeletePackageCart(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete package cart {PackageCartId}.", id);
                return StatusCode(500, "An unexpected error occurred while deleting the package cart.");
            }
        }
    }
}
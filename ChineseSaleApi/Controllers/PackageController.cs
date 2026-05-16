using ChineseSaleApi.Attributes;
using ChineseSaleApi.Dto;
using ChineseSaleApi.ServiceInterfaces;
using ChineseSaleApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _service;
        private readonly ILogger<PackageController> _logger;

        public PackageController(IPackageService service, ILogger<PackageController> logger)
        {
            _service = service;
            _logger = logger;
        }

        //read
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPackageById(int id)
        {
            try
            {
                var package = await _service.GetPackageById(id);
                if (package == null)
                {
                    return NotFound();
                }
                return Ok(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get package by id {PackageId}.", id);
                return StatusCode(500, "An unexpected error occurred while retrieving the package.");
            }
        }

        [HttpGet("lottery/{lotteryId}")]
        public async Task<IActionResult> GetAllPackages(int lotteryId)
        {
            try
            {
                var packages = await _service.GetAllPackages(lotteryId);
                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get packages for lottery {LotteryId}.", lotteryId);
                return StatusCode(500, ex + "An unexpected error occurred while retrieving packages.");
            }
        }
        [Authorize]
        [Admin]
        [HttpGet("id/{id}/update")]
        public async Task<IActionResult> GetPackagesByIdUpdate(int id)
        {
            try
            {
                var packages = await _service.GetPackageByIdUpdate(id);
                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get packages by ids {PackageId}.", string.Join(", ", id));
                return StatusCode(500, "An unexpected error occurred while retrieving the packages.");
            }
        }
        //create
        [Authorize]
        [Admin]
        [HttpPost]
        public async Task<IActionResult> AddPackage([FromBody] CreatePackageDto createPackageDto)
        {
            try
            {
                var id = await _service.AddPackage(createPackageDto);
                return CreatedAtAction(nameof(GetPackageById), new { id = id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add package for lottery {LotteryId}.", createPackageDto?.LotteryId);
                return StatusCode(500, "An unexpected error occurred while adding the package.");
            }
        }

        //update
        [Authorize]
        [Admin]
        [HttpPut]
        public async Task<IActionResult> UpdatePackage([FromBody] UpdatePackageDto packageDto)
        {
            try
            {
                var success = await _service.UpdatePackage(packageDto);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update package {PackageId}.", packageDto?.Id);
                return StatusCode(500, "An unexpected error occurred while updating the package.");
            }
        }

        //delete
        [Authorize]
        [Admin]
        [HttpDelete]
        public async Task<IActionResult> DeletePackage(int id)
        {
            try
            {
                await _service.DeletePackage(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete package {PackageId}.", id);
                return StatusCode(500, "An unexpected error occurred while deleting the package.");
            }
        }
    }
}
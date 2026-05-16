using ChineseSaleApi.Attributes;
using ChineseSaleApi.Dto;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService service, ILogger<CategoryController> logger)
        {
            _service = service;
            _logger = logger;
        }

        //read
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _service.GetAllCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all categories.");
                return StatusCode(500, "An unexpected error occurred while retrieving categories.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _service.GetCategoryById(id);
                if (category == null)
                {
                    return NotFound();
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get category by id {CategoryId}.", id);
                return StatusCode(500, "An unexpected error occurred while retrieving the category.");
            }
        }

        //create
        [Authorize]
        [Admin]
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryDto category)
        {
            try
            {
                await _service.AddCategory(category);
                return Created(nameof(GetCategoryById), category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add category {Name}.", category?.Name);
                return StatusCode(500, "An unexpected error occurred while adding the category.");
            }
        }

        //delete
        [Authorize]
        [Admin]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _service.DeleteCategory(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category {CategoryId}.", id);
                return StatusCode(500, "An unexpected error occurred while deleting the category.");
            }
        }
    }
}
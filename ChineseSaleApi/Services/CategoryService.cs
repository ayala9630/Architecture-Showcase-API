using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChineseSaleApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;
        private readonly RedisCacheService _cache;
        private readonly CacheSettings _cacheSettings;

        public CategoryService(
            ICategoryRepository repository,
            IMapper mapper,
            ILogger<CategoryService> logger,
            RedisCacheService cache,
            IOptions<CacheSettings> cacheSettings)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
        }

        //create
        public async Task AddCategory(CreateCategoryDto categoryDto)
        {
            try
            {
                Category category = _mapper.Map<Category>(categoryDto);
                await _repository.AddCategory(category);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "AddCategory received a null argument: {@CategoryDto}", categoryDto);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding a category.");
                throw;
            }
        }

        //read
        public async Task<CategoryDto?> GetCategoryById(int id)
        {
            try
            {
                var cacheKey = $"categories:id:{id}";
                var cachedCategory = await _cache.GetAsync<CategoryDto>(cacheKey);

                if (cachedCategory != null)
                {
                    return cachedCategory;
                }

                var category = await _repository.GetCategory(id);
                if (category == null)
                {
                    return null;
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                await _cache.SetAsync(cacheKey, categoryDto, TimeSpan.FromHours(_cacheSettings.Categories.GetByIdTtlHours));

                return categoryDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get category by id {CategoryId}.", id);
                throw;
            }
        }

        public async Task<List<CategoryDto>> GetAllCategories()
        {
            try
            {
                const string cacheKey = "categories";
                var cachedCategories = await _cache.GetAsync<List<CategoryDto>>(cacheKey);

                if (cachedCategories != null)
                {
                    return cachedCategories;
                }

                var categories = await _repository.GetAllCategories();
                var categoryDtos = categories.Select(category => _mapper.Map<CategoryDto>(category)).ToList();

                await _cache.SetAsync(cacheKey, categoryDtos, TimeSpan.FromHours(_cacheSettings.Categories.GetAllTtlHours));

                return categoryDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all categories.");
                throw;
            }
        }

        //delete
        public async Task DeleteCategory(int id)
        {
            try
            {
                await _repository.DeleteCategory(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category {CategoryId}.", id);
                throw;
            }
        }
    }
}

using ChineseSaleApi.Dto;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface ICategoryService
    {
        Task AddCategory(CreateCategoryDto categoryDto);
        Task DeleteCategory(int id);
        Task<List<CategoryDto>> GetAllCategories();
        Task<CategoryDto?> GetCategoryById(int id);
    }
}
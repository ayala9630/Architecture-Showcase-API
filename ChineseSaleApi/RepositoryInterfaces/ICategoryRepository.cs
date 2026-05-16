using ChineseSaleApi.Models;

namespace ChineseSaleApi.RepositoryInterfaces
{
    public interface ICategoryRepository
    {
        Task<int> AddCategory(Category category);
        Task DeleteCategory(int id);
        Task<IEnumerable<Category>> GetAllCategories();
        Task<Category?> GetCategory(int id);
        Task UpdateCategory(Category category);
    }
}
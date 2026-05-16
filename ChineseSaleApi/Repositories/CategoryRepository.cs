using ChineseSaleApi.Data;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ChineseSaleApi.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ChineseSaleContext _context;
        public CategoryRepository(ChineseSaleContext context)
        {
            _context = context;
        }
        //create
        public async Task<int> AddCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category.Id;
        }
        //read
        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            return await _context.Categories.ToListAsync();
        }
        public async Task<Category?> GetCategory(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
        }
        //update
        public async Task UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        //delete
        public async Task DeleteCategory(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}

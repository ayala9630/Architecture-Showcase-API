using ChineseSaleApi.Data;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ChineseSaleApi.Repositories
{
    public class PackageCartRepository : IPackageCartRepository
    {
        private readonly ChineseSaleContext _context;
        public PackageCartRepository(ChineseSaleContext context)
        {
            _context = context;
        }
        //create
        public async Task<int> AddPackageCart(PackageCart packageCart)
        {
            _context.PackageCarts.Add(packageCart);
            await _context.SaveChangesAsync();
            return packageCart.Id;
        }
        //read
        public async Task<IEnumerable<PackageCart>> GetAllPackageCarts()
        {
            return await _context.PackageCarts.ToListAsync();
        }
        public async Task<IEnumerable<PackageCart>> GetPackagesByUserId(int userId)
        {
            return await _context.PackageCarts
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }
        public async Task<PackageCart?> GetPackageCartById(int id)
        {
            return await _context.PackageCarts.FirstOrDefaultAsync(x => x.Id == id);
        }
        //update
        public async Task UpdatePackageCart(PackageCart packageCart)
        {
            _context.PackageCarts.Update(packageCart);
            await _context.SaveChangesAsync();
        }
        //delete
        public async Task DeletePackageCart(int id)
        {
            var packageCart = await _context.PackageCarts.FirstOrDefaultAsync(x => x.Id == id);
            if (packageCart != null)
            {
                _context.PackageCarts.Remove(packageCart);
                await _context.SaveChangesAsync();
            }
        }
    }
}
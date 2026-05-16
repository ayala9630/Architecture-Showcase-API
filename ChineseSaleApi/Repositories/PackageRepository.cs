using ChineseSaleApi.Data;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ChineseSaleApi.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly ChineseSaleContext _context;
        public PackageRepository(ChineseSaleContext context)
        {
            _context = context;
        }
        //create
        public async Task<int> AddPackage(Package package)
        {
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();
            return package.Id;
        }
        //read
        public async Task<IEnumerable<Package>> GetAllPackages(int lotteryId)
        {
            return await _context.Packages.Where(l=>l.LotteryId==lotteryId).ToListAsync();
        }
        public async Task<Package?> GetPackageById(int id)
        {
            return await _context.Packages.FirstOrDefaultAsync(x => x.Id == id);
        }
        //update
        public async Task UpdatePackage(Package package)
        {
            _context.Packages.Update(package);
            await _context.SaveChangesAsync();
        }
        //delete
        public async Task DeletePackage(int id)
        {
            var package = await _context.Packages.FirstOrDefaultAsync(x => x.Id == id);
            if (package != null)
            {
                _context.Packages.Remove(package);
                await _context.SaveChangesAsync();
            }
        }
    }

}
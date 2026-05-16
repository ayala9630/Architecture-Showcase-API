using ChineseSaleApi.Data;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ChineseSaleApi.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly ChineseSaleContext _context;
        public DonorRepository(ChineseSaleContext context)
        {
            _context = context;
        }
        //create
        public async Task<int> AddDonor(Donor donor)
        {
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();
            return donor.Id;
        }
        //read
        public async Task<IEnumerable<Donor>> GetAllDonors()
        {
            return await _context.Donors.ToListAsync();
        }
        public async Task<IEnumerable<Donor>> GetDonorByLotteryId(int lottery)
        {
            return await _context.Donors
                .Where(d => d.Lotteries.Any(l => l.Id == lottery))
                .ToListAsync();
        }
        public async Task<int> GetDonorCountByLotteryId(int lotteryId)
        {
            return await _context.Donors
                .Where(d => d.Lotteries.Any(l => l.Id == lotteryId))
                .CountAsync();
        }
        public async Task<Donor?> GetDonorById(int id)
        {
            return await _context.Donors.Include(g => g.Gifts).Include(d=>d.Lotteries)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<(IEnumerable<Donor> items, int totalcount)> GetDonorsWithPagination(int lottery, int pageNumber, int pageSize)
        {
            var query = _context.Donors.Where(d => d.Lotteries.Any(l => l.Id == lottery)).AsQueryable();
            var totalCount = await query.CountAsync();
            var donors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (donors, totalCount);
        }
        public async Task<(IEnumerable<Donor> items, int totalcount)> GetDonorsNameSearchedPagination(int lottery, int pageNumber, int pageSize, string textSearch)
        {
            var query = _context.Donors.Where(d => d.Lotteries.Any(l => l.Id == lottery)).Where(t => (t.FirstName + " " + t.LastName).Contains(textSearch)).AsQueryable();

            var totalCount = await query.CountAsync();
            var donors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (donors, totalCount);
        }

        public async Task<(IEnumerable<Donor> items, int totalcount)> GetDonorsEmailSearchedPagination(int lottery, int pageNumber, int pageSize, string textSearch)
        {
            var query = _context.Donors.Where(d => d.Lotteries.Any(l => l.Id == lottery)).Where(t => t.CompanyEmail.Contains(textSearch)).AsQueryable();
            var totalCount = await query.CountAsync();
            var donors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (donors, totalCount);
        }

        //update
        public async Task UpdateDonor(Donor donor)
        {
            _context.Donors.Update(donor);
            await _context.SaveChangesAsync();
        }
        //update lottery donor
        public async Task<bool?> UpdateLotteryDonor(int donorId, int lotteryId)
        {
            var donor = await _context.Donors.Include(d => d.Lotteries)
                .FirstOrDefaultAsync(d => d.Id == donorId);
            var lottery = await _context.Lotteries
                .FirstOrDefaultAsync(l => l.Id == lotteryId);
            if (donor == null || lottery == null)
            {
                return null;
            }
            if (!donor.Lotteries.Any(l => l.Id == lotteryId))
            {
                donor.Lotteries.Add(lottery);
                await _context.SaveChangesAsync();
            }

            return true;
        }
        //delete
        public async Task<bool?> DeleteDonor(int id)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(x => x.Id == id);
            if (donor != null)
            {
                _context.Donors.Remove(donor);
                await _context.SaveChangesAsync();
                return true;
            }
            return null;
        }
        //delete lottery donor
        public async Task<bool?> DeleteLotteryDonor(int donorId, int lotteryId)
        {
            var donor = await _context.Donors
                .Include(d => d.Lotteries)
                .FirstOrDefaultAsync(d => d.Id == donorId);

            var lottery = await _context.Lotteries
                .FirstOrDefaultAsync(l => l.Id == lotteryId);

            if (donor == null || lottery == null)
            {
                return null;
            }
            if (donor.Lotteries.Any(l => l.Id == lotteryId))
            {
                donor.Lotteries.Remove(lottery);
                await _context.SaveChangesAsync();
            }
            return true;
        }

    }
}

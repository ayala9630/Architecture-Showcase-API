
using ChineseSaleApi.Data;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ChineseSaleApi.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ChineseSaleContext _context;
        public AddressRepository(ChineseSaleContext context)
        {
            _context = context;
        }

        //create
        public async Task<int> AddAddress(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address.Id;
        }

        //read
        public async Task<Address?> GetAddress(int id)
        {
            return await _context.Addresses.FirstOrDefaultAsync(x => x.Id == id);

        }
        //update
        public async Task UpdateAddress(Address address)
        {
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
        }
    }
}

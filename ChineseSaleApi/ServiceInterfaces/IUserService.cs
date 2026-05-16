using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using StoreApi.DTOs;

namespace ChineseSaleApi.Services
{
    public interface IUserService
    {
        Task AddUser(CreateUserDto createUserDto);
        Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto loginRequest);
        Task<UserDto?> GetUserById(int id);
        Task<List<UserDto>> GetAllUsers();
        Task<int> GetUserCount();
        Task<PaginatedResultDto<UserDto>> GetUserWithPagination(PaginationParamsDto paginationParams);
        Task<bool?> UpdateUser(UpdateUserDto userDto);
        Task<bool> IsEmailExists(string email);
        Task<bool> IsUserNameExists(string userName);
    }
}
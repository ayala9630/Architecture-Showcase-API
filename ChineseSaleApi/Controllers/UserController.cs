using ChineseSaleApi.Attributes;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.ServiceInterfaces;
using ChineseSaleApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreApi.DTOs;
using System;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService service, ILogger<UserController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                var user = await _service.AuthenticateAsync(loginDto);
                if (user == null)
                {
                    return Unauthorized();
                }
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid login request for user {Username}.", loginDto?.UserName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user {Username}.", loginDto?.UserName);
                throw;
            }
        }

        //read
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _service.GetUserById(id);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for GetUserById: {UserId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user by id {UserId}.", id);
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _service.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all users.");
                throw;
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetUserCount()
        {
            try
            {
                var count = await _service.GetUserCount();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user count.");
                return StatusCode(500, "An unexpected error occurred while retrieving user count.");
            }
        }
        [Authorize]
        [Admin]
        [HttpGet("pagination")]
        public async Task<IActionResult> GetUsersWithPagination([FromQuery] PaginationParamsDto paginationParamsDto)
        {
            try
            {
                var pagedUsers = await _service.GetUserWithPagination(paginationParamsDto);
                return Ok(pagedUsers);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid pagination parameters.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get users with pagination.");
                throw;
            }
        }

        //create
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                await _service.AddUser(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { Id = createUserDto.Username }, createUserDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed registering user {Username}.", createUserDto?.Username);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register user {Username}.", createUserDto?.Username);
                throw;
            }
        }
        [HttpGet("userName/{userName}")]
        public async Task<IActionResult> IsUserNameExists(string userName)
        {
            try
            {
                var exists = await _service.IsUserNameExists(userName);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if username exists: {UserName}", userName);
                throw;
            }
        }
        [HttpGet("email/{email}")]
        public async Task<IActionResult> IsEmailExists(string email)
        {
            try
            {
                var exists = await _service.IsEmailExists(email);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if email exists: {Email}", email);
                throw;
            }
        }
        //update
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto userDto)
        {
            try
            {
                var success = await _service.UpdateUser(userDto);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed updating user {UserId}.", userDto?.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}.", userDto?.Id);
                throw;
            }
        }
    }
}
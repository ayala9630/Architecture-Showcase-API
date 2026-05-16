using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ChineseSaleApi.Dto;
using ChineseSaleApi.Models;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoreApi.DTOs;

namespace ChineseSaleApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IAddressService _addressService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IEmailService emailService,
            IUserRepository repository,
            IAddressService addressService,
            ITokenService tokenService,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<UserService> logger
        )
        {
            _tokenService = tokenService;
            _configuration = configuration;
            _addressService = addressService;
            _repository = repository;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        // create
        public async Task AddUser(CreateUserDto createUserDto)
        {
            try
            {
                if (await _repository.IsUserNameExists(createUserDto.Username))
                {
                    throw new ArgumentException("Username already exists", nameof(createUserDto.Username));
                }

                int idAddress = await _addressService.AddAddressForUser(createUserDto.Address);
                User user = _mapper.Map<User>(createUserDto);
                user.Password = HashPassword(createUserDto.Password);
                user.AddressId = idAddress;

                //send welcome email(synchronous method in your EmailService)
                   _emailService.SendEmail(new EmailRequestDto()
                {
                    To = createUserDto.Email,
                    Subject = "ברוכים הבאים ל‑Chinese Sale — הרשמתך להגרלה",
                    Body = BuildWelcomeHtml(createUserDto.FirstName)
                });

                await _repository.AddUser(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while adding user {Username}.", createUserDto?.Username);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user {Username}.", createUserDto?.Username);
                throw;
            }
        }

        // read one
        public async Task<UserDto?> GetUserById(int id)
        {
            try
            {
                var user = await _repository.GetUserById(id);
                if (user == null)
                {
                    return null;
                }

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user by id {UserId}.", id);
                throw;
            }
        }

        // read all
        public async Task<List<UserDto>> GetAllUsers()
        {
            try
            {
                var users = await _repository.GetAllUsers();
                return users.Select(user => _mapper.Map<UserDto>(user)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all users.");
                throw;
            }
        }

          public async Task<int> GetUserCount()
          {
            try
            {
              return await _repository.GetUserCount();
            }
            catch (Exception ex)
            {
              _logger.LogError(ex, "Failed to get user count.");
              throw;
            }
          }

        public async Task<bool> IsUserNameExists(string userName)
        {
            return await _repository.IsUserNameExists(userName);
        }

        public async Task<bool> IsEmailExists(string email)
        {
            return await _repository.IsEmailExists(email);
        }


        // pagination
        public async Task<PaginatedResultDto<UserDto>> GetUserWithPagination(PaginationParamsDto paginationParams)
        {
            try
            {
                var (items, totalCount) = await _repository.GetUsersWithPagination(paginationParams.PageNumber, paginationParams.PageSize);

                List<UserDto> userDtos = items.Select(user => _mapper.Map<UserDto>(user)).ToList();

                return new PaginatedResultDto<UserDto>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get users with pagination.");
                throw;
            }
        }

        // update
        public async Task<bool?> UpdateUser(UpdateUserDto userDto)
        {
            try
            {
                var user = await _repository.GetUserById(userDto.Id);
                if (user == null)
                {
                    return null;
                }

                if (userDto.Address != null)
                {
                    await _addressService.UpdateAddress(userDto.Address);
                }

                if (!string.IsNullOrWhiteSpace(userDto.Email) && userDto.Email != user.Email)
                {
                    var allUsers = await _repository.GetAllUsers();
                    if (allUsers.Any(u => u.Email == userDto.Email))
                    {
                        throw new ArgumentException("Email already exists", nameof(userDto.Email));
                    }
                }

                _mapper.Map(userDto, user);

                await _repository.UpdateUser(user);
                return true;
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

        // authenticate
        public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var user = await _repository.GetUserByUserName(loginRequest.UserName);
                if (user == null)
                {
                    return null;
                }

                var hashedPassword = HashPassword(loginRequest.Password);
                if (user.Password != hashedPassword)
                {
                    return null;
                }

                var token = _tokenService.GenerateToken(user.Id, user.Email, user.FirstName, user.LastName, user.IsAdmin);
                var expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes", 60);

                // send login notification
                _emailService.SendEmail(new EmailRequestDto()
                {
                    To = user.Email,
                    Subject = "התראת כניסה — Chinese Sale",
                    Body = BuildLoginNotificationHtml(user.FirstName, DateTime.UtcNow)
                });

                _logger.LogInformation($"User {user.UserName} logged in successfully.");

                return new LoginResponseDto
                {
                  Token = token,
                  TokenType = "Bearer",
                  ExpiresIn = expiryMinutes * 60,
                  User = _mapper.Map<UserDto>(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user {Username}.", loginRequest?.UserName);
                throw;
            }
        }

        private static string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private static string BuildLoginNotificationHtml(string firstName, DateTime loginTimeUtc)
        {
            var loginTimeLocal = loginTimeUtc.ToLocalTime().ToString("f");
            return $@"<!doctype html>
<html lang=""he"" dir=""rtl"">
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>התראת כניסה — Chinese Sale</title>
  </head>
  <body style=""margin:0;padding:0;background:#fbfaf8;font-family:Arial, Helvetica, sans-serif;color:#1b1b1b;direction:rtl;text-align:right;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:720px;margin:36px auto;background:#ffffff;border-radius:10px;overflow:hidden;border:1px solid #e9e6e2;box-shadow:0 8px 28px rgba(20,20,20,0.06);"">
      <tr>
        <td style=""background:#0f2230;padding:26px 30px;color:#f3e9db;text-align:right;"">
          <h1 style=""margin:0;font-size:22px;font-weight:700;"">Chinese Sale — עדכון חשבון</h1>
          <p style=""margin:6px 0 0;font-size:13px;opacity:0.92;color:#d8cdb6;"">הודעה בטוחה ומכובדת</p>
        </td>
      </tr>
      <tr>
        <td style=""padding:28px 30px;"">
          <p style=""margin:0 0 12px;font-size:16px;color:#111;"">שלום {firstName},</p>
          <p style=""margin:0 0 16px;font-size:14px;color:#333;line-height:1.6;"">זוהתה כניסה חדשה לחשבונך בתאריך: <strong>{loginTimeLocal}</strong>.</p>

          <p style=""margin:0 0 18px;font-size:14px;color:#333;"">אם זו פעולה שלא בוצעה על ידך — אנא שנה את הסיסמה באופן מיידי ופנה לתמיכה.</p>

          <p style=""margin:0 0 18px;"">
            <a href=""#"" style=""display:inline-block;padding:12px 20px;background:#b8873e;color:#fff;border-radius:6px;text-decoration:none;font-weight:700;font-size:14px;"">אבטח את החשבון</a>
          </p>

          <p style=""margin:18px 0 0;font-size:13px;color:#6b655f;"">בברכה,<br/><strong>צוות Chinese Sale</strong></p>
        </td>
      </tr>
      <tr>
        <td style=""background:#fbf8f6;padding:14px 18px;font-size:12px;color:#8b7a62;text-align:center;"">
          הודעה אוטומטית — במידה ויש צורך בעזרה יש להשיב להודעה זו או לגשת למרכז העזרה שלנו.
        </td>
      </tr>
    </table>
  </body>
</html>";
        }

        private static string BuildWelcomeHtml(string firstName)
        {
            return $@"<!doctype html>
<html lang=""he"" dir=""rtl"">
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>ברוכים הבאים — Chinese Sale</title>
  </head>
  <body style=""margin:0;padding:0;background:#fbfaf8;font-family:Arial, Helvetica, sans-serif;color:#1b1b1b;direction:rtl;text-align:right;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:720px;margin:36px auto;background:#ffffff;border-radius:10px;overflow:hidden;border:1px solid #ece6df;box-shadow:0 10px 36px rgba(12,18,23,0.06);"">
      <tr>
        <td style=""background:#071926;padding:28px 34px;color:#f6ead0;text-align:right;"">
          <h1 style=""margin:0;font-size:24px;font-weight:700;"">ברוכים הבאים ל‑Chinese Sale</h1>
          <p style=""margin:6px 0 0;font-size:13px;opacity:0.9;color:#d9cfa6;"">הצטרפתך מאפשרת השתתפות בהגרלות סיניות מובחרות</p>
        </td>
      </tr>
      <tr>
        <td style=""padding:30px 34px;"">
          <p style=""margin:0 0 14px;font-size:16px;color:#111;"">שלום {firstName},</p>

          <p style=""margin:0 0 16px;font-size:14px;color:#333;line-height:1.6;"">
            תודה על הרשמתך. חשבונך מוכן להשתתפות בהגרלות הסיניות היוקרתיות שאנו מארגנים — הזדמנויות לזכייה במבחר פרסים נבחרים.
          </p>

          <p style=""margin:0 0 18px;font-size:14px;color:#333;"">
            תוכלו להתחיל בכמה לחיצות: בדוק/י את דף ההגרלות, רכש/י כרטיס והמתן להודעה על מועד ההגרלה.
          </p>

          <p style=""margin:0 0 18px;"">
            <a href=""#"" style=""display:inline-block;padding:12px 20px;background:#c59d5f;color:#fff;border-radius:6px;text-decoration:none;font-weight:700;font-size:14px;"">עבור להגרלות</a>
            <a href=""#"" style=""display:inline-block;margin-right:10px;padding:12px 20px;background:#f3efe2;color:#3b2f20;border-radius:6px;text-decoration:none;font-weight:700;font-size:14px;border:1px solid #ead6a7;"">החשבון שלי</a>
          </p>

          <p style=""margin:22px 0 0;font-size:13px;color:#6b6156;"">אנו מאחלים בהצלחה ובהנאה — צוות Chinese Sale</p>
        </td>
      </tr>
      <tr>
        <td style=""background:#fbf8f6;padding:14px 18px;font-size:12px;color:#8b7a62;text-align:center;"">
          קיבלת הודעה זו כי נרשמת ל‑Chinese Sale. נהל/י העדפות בתיבת ההגדרות של החשבון.
        </td>
      </tr>
    </table>
  </body>
</html>";
        }
    }
}

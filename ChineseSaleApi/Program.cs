using AutoMapper;
using ChineseSaleApi.Repositories;
using ChineseSaleApi.RepositoryInterfaces;
using ChineseSaleApi.ServiceInterfaces;
using ChineseSaleApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;
using StoreApi.Services;
using System.Text;
using System.Threading.RateLimiting;
using ChineseSaleApi.Dto;
using MailKit;
using ChineseSaleApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Configure Serilog
 Log.Logger = new LoggerConfiguration()
     .ReadFrom.Configuration(new ConfigurationBuilder()
         .AddJsonFile("appsettings.json" +
         "")
         .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
         .Build())
     .Enrich.FromLogContext()
//     .WriteTo.Console()
//     .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
     .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<ChineseSaleApi.Data.ChineseSaleContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Repository Injections
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICardCartRepository, CardCartRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IDonorRepository, DonorRepository>();
builder.Services.AddScoped<IGiftRepository, GiftRepository>();
builder.Services.AddScoped<ILotteryRepository, LotteryRepository>();
builder.Services.AddScoped<IPackageCartRepository, PackageCartRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
//Service Injections
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILotteryService, LotteryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IGiftService, GiftService>();
builder.Services.AddScoped<ICardCartService, CardCartService>();
builder.Services.AddScoped<IPackageCartService, PackageCartService>();
builder.Services.AddScoped<IDonorService, DonorService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Export Services
builder.Services.AddScoped<ChineseSaleApi.Services.Exporters.CsvExporter>();
builder.Services.AddScoped<ChineseSaleApi.Services.Exporters.JsonExporter>();
builder.Services.AddScoped<ChineseSaleApi.Services.Exporters.PdfExporter>();
builder.Services.AddScoped<IFileExportService, FileExportService>();

// Redis Configuration
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>();
    options.Configuration = redisSettings?.GetConnectionString();
});

builder.Services.AddScoped<RedisCacheService>();

builder.Services.Configure<EmailSettingsDto>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService,EmailService>();
//builder.Services.AddTransient<EmailService>(); // Register your EmailService for injection


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Log.Debug("JWT token validated for user {UserId}", userId);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            return RateLimitPartition.GetNoLimiter("no-limit");
        }

        var windowSeconds = builder.Configuration.GetValue<int>("RateLimiting:WindowSeconds");
        var permitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit");
        var segmentsPerWindow = builder.Configuration.GetValue<int>("RateLimiting:SegmentsPerWindow");

        return RateLimitPartition.GetSlidingWindowLimiter("global", _ => new SlidingWindowRateLimiterOptions
        {
            Window = TimeSpan.FromSeconds(windowSeconds),
            SegmentsPerWindow = segmentsPerWindow,
            PermitLimit = permitLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

// Use custom error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// ����� CORS
app.UseCors("AllowAllOrigins");

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();


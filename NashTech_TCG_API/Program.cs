// Updated Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NashTech_TCG_API.Common;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_API.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict();
});


// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cập nhật cấu hình OpenIddict 
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<AppDbContext>();
    })
    .AddServer(options =>
    {
        // Cấu hình hiện tại của bạn
        options.SetTokenEndpointUris("/api/Auth/token");
        options.AllowClientCredentialsFlow();
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();
        options.AcceptAnonymousClients();
        //options.RegisterScopes("api", "roles", "offline_access");
        options.RegisterScopes("api", "roles", "email", "offline_access");
        // Cấu hình thời gian sống token
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .DisableTransportSecurityRequirement();
    })
    .AddValidation(options => // Cấu hình validation cho API endpoints
    {
        // Sử dụng local server
        options.UseLocalServer();

        // Sử dụng ASP.NET Core integration
        options.UseAspNetCore();

        // Tùy chọn thêm: Cấu hình chi tiết
        options.Configure(validation =>
        {
            validation.TokenValidationParameters.ValidateAudience = false;
            validation.TokenValidationParameters.ValidateIssuer = false;
            validation.TokenValidationParameters.RequireExpirationTime = true;
            validation.TokenValidationParameters.RequireSignedTokens = true;
        });
    });

// Cấu hình authentication mặc định
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

// Register repositories and services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IRarityRepository, RarityRepository>();
builder.Services.AddScoped<IProductRatingRepository, ProductRatingRepository>();
builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
builder.Services.AddScoped<IdGenerator>();


// Register the DataSeeder service
builder.Services.AddScoped<DataSeeder>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins(
                    "https://localhost:7075", // Razor Pages app
                    "https://localhost:3000",  // React admin app with HTTPS
                    "http://localhost:3000"    // React admin app with HTTP
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // Important for cookies
        });
});
builder.Services.AddHttpContextAccessor();


// Trong Program.cs, cập nhật phần AddSwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NashTech TCG API", Version = "v1" });

    // Add JWT authentication documentation
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seed the database
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
}
app.UseGlobalExceptionMiddleware();
app.UseCors("AllowSpecificOrigins");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();


// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

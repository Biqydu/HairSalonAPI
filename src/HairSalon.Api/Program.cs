using HairSalon.Api.Data;
using HairSalon.Api.Data.Entities;
using HairSalon.Api.Features.Auth;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddUserManager<AppUserManager>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/api/docs", options =>
    {
        options.Theme = ScalarTheme.DeepSpace;
    }); 
}

app.MapGroup("/api")
    .MapAuthEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        IdentitySeeder.SeedRolesAsync(services).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the application roles.");
    }
}

app.Run();
using FiapCloudGames.Users.Api.BackgroundServices;
using FiapCloudGames.Users.Api.Extensions;
using FiapCloudGames.Users.Api.Middlewares;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Infrastructure.Data;
using FiapCloudGames.Users.Infrastructure.Repositories;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddFiapCloudGamesSwagger();

builder.Services.AddFiapCloudGamesOpenTelemetry();

builder.Services.AddFiapCloudGamesJwtAuthentication(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? ""));

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<ServiceBusOptions>(opts =>
{
    opts.ConnectionString = configuration["ServiceBus:ConnectionString"]
        ?? configuration["SERVICE_BUS_CONNECTION_STRING"]
        ?? Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING");
});
builder.Services.AddSingleton<IServiceBusClientWrapper, ServiceBusClientWrapper>();

builder.Services.AddHostedService<PurchaseCompletedConsumer>();
builder.Services.AddHostedService<GameConsumer>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TracingEnrichmentMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

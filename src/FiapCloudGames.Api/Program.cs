using Azure.Messaging.ServiceBus;
using FiapCloudGames.Users.Api.BackgroundServices;
using FiapCloudGames.Users.Api.Extensions;
using FiapCloudGames.Users.Api.Middlewares;
using FiapCloudGames.Users.Application.Interfaces.Publishers;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Infrastructure.Data;
using FiapCloudGames.Users.Infrastructure.Elasticsearch;
using FiapCloudGames.Users.Infrastructure.Publishers;
using FiapCloudGames.Users.Infrastructure.Repositories;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using FiapCloudGames.Users.Infrastructure.Services;
using FiapCloudGames.Users.Shared;
using Microsoft.EntityFrameworkCore;
using Nest;

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

var sbConnectionString = configuration["ServiceBus:ConnectionString"] ?? "";
builder.Services.Configure<ServiceBusOptions>(opts => { opts.ConnectionString = sbConnectionString; });

builder.Services.AddSingleton(new ServiceBusClient(sbConnectionString));
builder.Services.AddSingleton<IServiceBusClientWrapper, ServiceBusClientWrapper>();
builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();

//var esUri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
//var esSettings = new ConnectionSettings(new Uri(esUri))
//    .BasicAuthentication(configuration["Elasticsearch:Username"], configuration["Elasticsearch:Password"]);
//esSettings.DefaultIndex("purchases-history");
//builder.Services.AddSingleton<IElasticClient>(new ElasticClient(esSettings));
var settings = new ConnectionSettings(new Uri(configuration["Elasticsearch:Uri"]?? ""))
    .BasicAuthentication(configuration["Elasticsearch:Username"], configuration["Elasticsearch:Password"])
    .ServerCertificateValidationCallback((o, certificate, chain, errors) => true); // Ignora validação do certificado
var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);

builder.Services.AddScoped<IPurchaseHistoryService, PurchaseHistoryService>();
builder.Services.AddScoped<ISuggestionService, SuggestionService>();
builder.Services.AddScoped<IGameMessageHandler, GameMessageHandler>();
builder.Services.AddScoped<IPurchaseMessageHandler, PurchaseMessageHandler>();

builder.Services.AddScoped<IUserEventPublisher, UserEventPublisher>();
builder.Services.AddScoped<IPurchaseHistoryEventPublisher, PurchaseHistoryEventPublisher>();

builder.Services.AddHostedService<PurchaseCompletedConsumer>();
builder.Services.AddHostedService<GameConsumer>();
builder.Services.AddHostedService<PurchaseHistoryConsumer>();
builder.Services.AddHostedService<ResourceLoggingService>();

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

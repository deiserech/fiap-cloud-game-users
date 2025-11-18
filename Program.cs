using Microsoft.EntityFrameworkCore;
using FiapCloudGames.Users.Data;
using FiapCloudGames.Users.Services;
using FiapCloudGames.Users.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<UsersDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("UsersDb") ?? "Server=(localdb)\\mssqllocaldb;Database=users_db;Trusted_Connection=True;"));

services.AddSingleton<ServiceBusClientWrapper>();
services.AddHostedService<PurchaseCompletedConsumer>();

var app = builder.Build();
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();

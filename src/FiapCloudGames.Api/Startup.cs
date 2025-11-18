using System.Text.Json.Serialization;
using FiapCloudGames.Api.Extensions;
using FiapCloudGames.Api.Middlewares;
using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Application.Services;
using FiapCloudGames.Domain.Interfaces.Repositories;
using FiapCloudGames.Infrastructure.Data;
using FiapCloudGames.Infrastructure.Repositories;
using FiapCloudGames.Users.Api.BackgroundServices;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;

namespace FiapCloudGames.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddSqlServer(Configuration.GetConnectionString("DefaultConnection") ?? "");
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
            services.AddEndpointsApiExplorer();
            services.AddFiapCloudGamesSwagger();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddFiapCloudGamesJwtAuthentication(Configuration);

            services.AddSingleton<ServiceBusClientWrapper>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<ILibraryRepository, LibraryRepository>();
            services.AddScoped<ILibraryService, LibraryService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddHostedService<ResourceLoggingService>();
            services.AddHostedService<PurchaseCompletedConsumer>();

            services.AddFiapCloudGamesOpenTelemetry();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FIAP Cloud Games API v1");
                    c.RoutePrefix = "swagger";
                    c.DisplayRequestDuration();
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                });
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
             {
                 endpoints.MapControllers();
                 endpoints.MapHealthChecks("/health");
             });

            app.UseMiddleware<TracingEnrichmentMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}

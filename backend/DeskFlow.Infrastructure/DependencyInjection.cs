using DeskFlow.Application.Abstractions;
using DeskFlow.Infrastructure.Data;
using DeskFlow.Infrastructure.Repositories;
using DeskFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DeskFlowDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<DeskFlowDbContext>());
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMetricsReader, MetricsReader>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DeskFlowDbContext>();
        await db.Database.MigrateAsync();
        await SeedData.InitializeAsync(scope.ServiceProvider, configuration);
        await PriorityReassessment.RunAsync(db);
    }
}
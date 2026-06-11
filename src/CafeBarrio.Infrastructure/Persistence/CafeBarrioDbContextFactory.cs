using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CafeBarrio.Infrastructure.Persistence;

public class CafeBarrioDbContextFactory : IDesignTimeDbContextFactory<CafeBarrioDbContext>
{
    public CafeBarrioDbContext CreateDbContext(string[] args)
    {
        // Support running from root solution directory or API project directory
        var basePath = Directory.GetCurrentDirectory();
        
        var apiPath = Path.Combine(basePath, "src", "CafeBarrio.API");
        if (Directory.Exists(apiPath))
        {
            basePath = apiPath;
        }
        else if (Path.GetFileName(basePath) == "CafeBarrio.Infrastructure")
        {
            basePath = Path.Combine(basePath, "..", "CafeBarrio.API");
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var builder = new DbContextOptionsBuilder<CafeBarrioDbContext>();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback connection string for migrations if appsettings cannot be found
            connectionString = "Server=localhost;Database=CafeDeBarrio;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        builder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(CafeBarrioDbContext).Assembly.FullName));

        return new CafeBarrioDbContext(builder.Options);
    }
}

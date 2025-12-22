using System.Collections.Generic;
using BLOOM_FILTER.Data;
using BLOOM_FILTER.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --------------------
    // Core services
    // --------------------
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // --------------------
    // SQL Server (Docker-safe with retry)
    // --------------------
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sql =>
            {
                sql.CommandTimeout(600);//needed for large seeding

                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null
                );

                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            }
        ));

    builder.Services.AddScoped<IApplicationDbContext>(sp =>
        sp.GetRequiredService<ApplicationDbContext>());

    // --------------------
    // Redis (Docker-safe)
    // --------------------
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var options = ConfigurationOptions.Parse(
            builder.Configuration["Redis:ConnectionString"] ?? "redis:6379"
        );

        options.AbortOnConnectFail = false; // critical for Docker
        options.ConnectRetry = 5;
        options.ReconnectRetryPolicy = new ExponentialRetry(5000);

        return ConnectionMultiplexer.Connect(options);
    });

    // --------------------
    // Application services
    // --------------------
    builder.Services.AddScoped<UserBloomService>();

    // --------------------
    // Kestrel binding (container port)
    // --------------------
    builder.WebHost.UseUrls("http://0.0.0.0:8080");

    var app = builder.Build();
    // --------------------
    // Seed DB & Bloom Filter
    // --------------------
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var bloom = services.GetRequiredService<UserBloomService>();

            // Apply migrations (create DB if it doesn't exist)
            await db.Database.MigrateAsync();

            // Ensure Bloom filter exists
            await bloom.EnsureFilterExistsAsync();

        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }


    // --------------------
    // Middleware
    // --------------------
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An Exception Occurred: {ex.Message}");
}
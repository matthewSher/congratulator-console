using Congratulator.Infrastructure;
using Congratulator.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Congratulator.ConsoleApp;

bool useSqlite = args.Contains("--sqlite", StringComparer.OrdinalIgnoreCase);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        if (!useSqlite)
            cfg.AddJsonFile("appsettings.Postgres.json", optional: true, reloadOnChange: true);
    })
    .UseContentRoot(Directory.GetCurrentDirectory())
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        var cs = useSqlite
            ? configuration.GetConnectionString("SqliteConnection")
            : configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<BirthdayDbContext>(o =>
        {
            if (useSqlite)
            {
                o.UseSqlite(cs);
                return;
            }
            o.UseNpgsql(cs);
        });

        services.AddScoped<IBirthdayRepository>(sp => sp.GetRequiredService<BirthdayDbContext>());
        services.AddSingleton<ConsoleRenderer>();
        services.AddSingleton<PromptHelper>();
        services.AddHostedService<ConsoleAppService>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BirthdayDbContext>();

    if (useSqlite)
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
}

await host.RunAsync();

using AzureBlobDownloader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

class Program
{
    static async Task Main(string[] args)
    {
        // Set up logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/AzureBlobDownloader.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Application started.");

            //setup dependency injection
            var serviceProvider = ConfigureServices();

            //configure user input
            var userInputService = serviceProvider.GetRequiredService<IUserInputService>();
            userInputService.GetUserInput();

            // run the application
            var app = serviceProvider.GetRequiredService<App>();
            await app.Run();

            Log.Information("Application ended.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }


    private static IServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<IConfiguration>(BuildConfiguration())
            .AddSingleton<IUserInputService, UserInputService>()
            .AddSingleton<App>()
            .BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    }
}

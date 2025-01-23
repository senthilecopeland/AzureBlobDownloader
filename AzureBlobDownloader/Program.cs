
using AzureBlobDownloader;
using Microsoft.Extensions.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
           .WriteTo.Console() 
           .WriteTo.File("logs/AzureBlobDownloader.txt", rollingInterval: RollingInterval.Day) 
           .CreateLogger();

Log.Information("Application started.");

var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) 
            .AddJsonFile("appsettings.json") 
            .Build();


var appSettings = configuration.GetSection("BlobSettings").Get<BlobSettings>();

Console.WriteLine("Hello, World!");

Log.Information("Application ended.");

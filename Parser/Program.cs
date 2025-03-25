using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using NLog.Extensions.Logging;
using ServiceLayer.Services.Parsing;
using DataLayer.Entities;
using ServiceLayer.Models;

namespace Parser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string workingDirectory = Directory.GetCurrentDirectory();
            string basePath = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;

            var appConfigBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("API/appsettings.json", optional: false)
                .AddJsonFile("API/appsettings.Development.json", optional: true);

            var sourcesConfigBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("API/sources.json", optional: false);

            IConfiguration appConfiguration = appConfigBuilder.Build();
            IConfiguration sourcesConfiguration = sourcesConfigBuilder.Build();

            List<string> regions = sourcesConfiguration.GetChildren().Select(x => x.Key).ToList();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(appConfiguration.GetConnectionString("DefaultConnection"));

            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddNLog());
            ILogger<Program> programLogger = factory.CreateLogger<Program>();


            AppDbContext dbContext = new(optionsBuilder.Options);
            await dbContext.Database.MigrateAsync();

            while (true)
            {
                List<IWeaterParser> weaterParsers = [
                    new RP5Parser(),
                    new YandexParser(),
                    new MeteoblueParser(),
                    new GismeteoParser(),
                    new OpenMeteoParser()
                    ];

                foreach (string region in regions)
                {
                    DataSource dataSource = sourcesConfiguration.GetSection(region).Get<DataSource>()!;
                    dataSource.RegionName = region;
                    try
                    {
                        foreach (IWeaterParser parser in weaterParsers)
                        {
                            IEnumerable<WeatherRecord> data = await parser.GetWeaterRecordsAsync(dataSource);
                            programLogger.LogInformation("parse {DataCount} records from {Source} for region {Region}", data.Count(), parser.SourceName, region);
                            await dbContext.WeaterRecords.AddRangeAsync(data);
                        }
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        programLogger.LogError(e.ToString());
                    }
                }
                await Task.Delay(new TimeSpan(24, 0, 0));
            }
        }
    }
}

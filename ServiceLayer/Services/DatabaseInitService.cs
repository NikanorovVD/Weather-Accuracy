using DataLayer;
using DataLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ServiceLayer.Constants;
using ServiceLayer.Extensions;
using ServiceLayer.Models.Settings;


namespace ServiceLayer.Services
{
    public class DatabaseInitService : BackgroundService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _dbContext;
        private readonly SuperuserSettings _adminSettings;
        public DatabaseInitService(IServiceScopeFactory serviceScopeFactory, IOptions<SuperuserSettings> options)
        {
            var scope = serviceScopeFactory.CreateScope();
            _userManager = scope.ServiceProvider.GetService<UserManager<AppUser>>();
            _roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
            _dbContext = scope.ServiceProvider.GetService<AppDbContext>();
            _adminSettings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await MigrateDatabaseAsync();
            await SeedDatabaseDataAsync();
        }

        private async Task MigrateDatabaseAsync()
        {
            await _dbContext.Database.MigrateAsync();
        }

        private async Task SeedDatabaseDataAsync()
        {
            foreach (var role in AppRoles.AllRoles())
            {
                IdentityRole? identity_role = await _roleManager.FindByNameAsync(role);
                if (identity_role == null)
                {
                    IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded) throw new Exception(result.GetMessage());
                }
            }

            AppUser? adminUser = await _userManager.FindByNameAsync(_adminSettings.UserName);
            if (adminUser == null)
            {
                adminUser = new AppUser()
                {
                    UserName = _adminSettings.UserName,
                };

                IdentityResult result = await _userManager.CreateAsync(adminUser, _adminSettings.Password);
                if (!result.Succeeded) throw new Exception(result.GetMessage());

                result = await _userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
                if (!result.Succeeded) throw new Exception(result.GetMessage());
            }


            //foreach(string dir in Directory.GetDirectories("C://Files/gismeteo"))
            //{
            //    foreach(string file in Directory.GetFiles(dir))
            //    {
            //        string content = File.ReadAllText(file);
            //        if (string.IsNullOrWhiteSpace(content)) continue;
            //        IEnumerable<WeatherRecord>? records = JsonSerializer.Deserialize<IEnumerable<WeatherRecord>>(content);
            //        if (records == null) continue;
            //        foreach(var record in records)
            //        {
            //            record.DateTime = DateTime.SpecifyKind(record.DateTime, DateTimeKind.Utc);
            //            record.ForecastDateTime = DateTime.SpecifyKind(record.ForecastDateTime, DateTimeKind.Utc);
            //            record.SourceId = "Gismeteo";
            //        }
            //        _dbContext.WeaterRecords.AddRange(records);
            //        _dbContext.SaveChanges();
            //    }
            //}
        }

        //private async Task CreateCsv()
        //{
        //    List<WeatherRecord> facts = await new ArchiveParser().GetArchiveRecordsAsync(
        //        new DateTime(2024,12,10)
        //        )
        //    List<ForecastResult> results = _dbContext.WeaterRecords
        //        .Where()
        //        .Select(r => new ForecastResult()
        //        {
        //            DaysForecast = 
        //        })
        //}

        //class ForecastResult
        //{
        //    public int DaysForecast { get; set; }
        //    public decimal Forecast {  get; set; }
        //    public decimal Fact {  get; set; }
        //}
    }
}

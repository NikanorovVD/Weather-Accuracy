using DataLayer;
using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Extensions;
using ServiceLayer.Models;
using ServiceLayer.Services.Parsing;

namespace ServiceLayer.Services
{
    public class AccuracyService
    {
        private readonly AppDbContext _appDbContext;
        private readonly FactRecordsService _factRecordsService;

        public AccuracyService(AppDbContext appDbContext, FactRecordsService factRecordsService)
        {
            _appDbContext = appDbContext;
            _factRecordsService = factRecordsService;
        }

        public async Task<IEnumerable<ForecastAccuracy>> GetForecastAccuraciesForRegionAsync(string region, int periodDays)
        {
            List<string> souces = _appDbContext.WeaterRecords.Select(r => r.Source).Distinct().ToList();
            souces.Remove(ArchiveParser.SourceName);

            List<ForecastAccuracy> result = [];
            for(int leadDays = 1; leadDays < 9; leadDays++)
            {
                IEnumerable<ForecastAccuracy> accuracies = await GetForecastAccuraciesAsync([region], souces, periodDays, leadDays);
                result.AddRange(accuracies);
            }
            return result;
        }

        public async Task<IEnumerable<ForecastAccuracy>> GetForecastAccuraciesAsync(IEnumerable<string> regions, IEnumerable<string> sources, int periodDays, int leadDays)
        {
            //return await regions.SelectManyAsync<string, ForecastAccuracy>(r => 
            //    sources.Select(async s => await GetForecastAccuracyAsync(r, s, periodDays, leadDays))
            //);
            List<ForecastAccuracy> accuracies = [];
            foreach (var region in regions) 
            {
                foreach (var source in sources)
                {
                    ForecastAccuracy accuracy = await GetForecastAccuracyAsync(region, source, periodDays, leadDays);
                    accuracies.Add(accuracy);
                }
            }
            return accuracies;
        }

        public async Task<ForecastAccuracy> GetForecastAccuracyAsync(string region, string source, int periodDays, int leadDays)
        {
            DateTime startDate = DateTime.UtcNow.AddDays(-periodDays);
            IEnumerable<WeatherRecord> forecastRecords = await _appDbContext.WeaterRecords
                .Where(r =>
                r.LeadDays == leadDays &&
                r.Source == source &&
                r.Region == region &&
                r.ForecastDateTime.Date >= startDate.Date &&
                r.ForecastDateTime.Date < DateTime.UtcNow.Date
                ).ToListAsync();

            IEnumerable<WeatherRecord> factRecords = await _appDbContext.WeaterRecords
                .Where(r =>
                r.Region == region &&
                r.Source == ArchiveParser.SourceName &&
                r.ForecastDateTime.Date >= startDate.Date &&
                r.ForecastDateTime.Date < DateTime.UtcNow.Date)
                .ToListAsync();//await _factRecordsService.GetFactRecords(startDate, DateTime.UtcNow);

            IEnumerable<ForecastDeviation> deviations = forecastRecords.Select(r => {
                WeatherRecord fact = factRecords.Single(f => f.ForecastDateTime.Date == r.ForecastDateTime.Date);
                return new ForecastDeviation()
                {
                    TemperatureAvg = (r.TemperatureAvg != null && fact.TemperatureAvg != null) ?
                        Math.Abs(r.TemperatureAvg.Value - fact.TemperatureAvg.Value): null,
                    TemperatureMax = (r.TemperatureMax != null && fact.TemperatureMax != null) ?
                        Math.Abs(r.TemperatureMax.Value - fact.TemperatureMax.Value) : null,
                    TemperatureMin = (r.TemperatureMin != null && fact.TemperatureMin != null) ?
                        Math.Abs(r.TemperatureMin.Value - fact.TemperatureMin.Value) : null,
                    Precipitation = (r.Precipitation != null && fact.Precipitation != null) ?
                        Math.Abs(r.Precipitation.Value - fact.Precipitation.Value) : null,
                    AtmosphericPressureAvg = (r.AtmosphericPressureAvg != null && fact.AtmosphericPressureAvg != null) ?
                        Math.Abs(r.AtmosphericPressureAvg.Value - fact.AtmosphericPressureAvg.Value) : null,
                    Humidity = (r.Humidity != null && fact.Humidity != null) ?
                        Math.Abs(r.Humidity.Value - fact.Humidity.Value) : null,
                    WindGust = (r.WindGust != null && fact.WindGust != null) ?
                        Math.Abs(r.WindGust.Value - fact.WindGust.Value) : null,
                    WindSpeed = (r.WindSpeed != null && fact.WindSpeed != null) ?
                        Math.Abs(r.WindSpeed.Value - fact.WindSpeed.Value) : null,
                };
            }).ToList();

            return new ForecastAccuracy()
            {
                LeadDays = leadDays,
                Region = region,
                Source = source,
                Deviation = new ForecastDeviation()
                {
                    TemperatureAvg = deviations.Average(f => f.TemperatureAvg),
                    TemperatureMax = deviations.Average(f => f.TemperatureMax),
                    TemperatureMin = deviations.Average(f => f.TemperatureMin),
                    AtmosphericPressureAvg = deviations.Average(f => f.AtmosphericPressureAvg),
                    Humidity = deviations.Average(f => f.Humidity),
                    Precipitation = deviations.Average(f => f.Precipitation),
                    WindGust = deviations.Average(f => f.WindGust),
                    WindSpeed = deviations.Average(f => f.WindSpeed)
                }
            };
        }
    }
}

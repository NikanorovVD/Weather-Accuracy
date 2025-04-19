using DataLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Collections;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class WeatherController : ControllerBase
    {
        private readonly AccuracyService _accuracyService;

        public WeatherController(AccuracyService accuracyService)
        {
            _accuracyService = accuracyService;
        }

        [HttpGet("accuracy")]
        public async Task<ForecastAccuracy> GetForecastAccuraciesAsync(string region, string source, int periodDays, int leadDays)
        {
            return await _accuracyService.GetForecastAccuracyAsync(region, source, periodDays, leadDays);
        }

        [HttpGet("region")]
        public async Task<IEnumerable<ForecastAccuracy>> GetForecastAccuraciesAsync(string region, int periodDays)
        {
            return await _accuracyService.GetForecastAccuraciesForRegionAsync(region, periodDays);
        }

    }
}

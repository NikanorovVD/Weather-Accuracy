using DataLayer.Entities;
using ServiceLayer.Models;
using ServiceLayer.Models.Errors;
using System.Globalization;
using System.Text.Json;

namespace ServiceLayer.Services.Parsing
{
    public class ArchiveParser
    {
        private const string _url = "https://archive-api.open-meteo.com/v1/archive";
        private static readonly CultureInfo _culture = CultureInfo.CreateSpecificCulture("en-US");

        public static string SourceName => "Archive";

        public async Task<IEnumerable<WeatherRecord>> GetArchiveRecordsAsync(DataSource dataSource, DateTime from, DateTime to)
        {
            string requestString = QueryHelper.CreateQuery(_url,
                ("latitude", dataSource.Latitude.ToString(_culture)),
                ("longitude", dataSource.Longitude.ToString(_culture)),
                ("start_date", GetDateString(DateOnly.FromDateTime(from))),
                ("end_date", GetDateString(DateOnly.FromDateTime(to))),
                ("daily", "temperature_2m_max,wind_speed_10m_max,temperature_2m_min,temperature_2m_mean,precipitation_sum,wind_gusts_10m_max,surface_pressure_mean,relative_humidity_2m_mean"),
                ("timezone", "auto")
                );

            using HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestString);
            HttpResponseMessage response = await client.GetAsync(requestString);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new RequestException("Error status code", requestString, response.StatusCode, content);

            OpenMeteoResponse? openMeteoResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(content)
                ?? throw new RequestException("Error deserilizing data", requestString, response.StatusCode, content);

            int days = (int)((to - from).TotalDays) + 1;
            return Enumerable.Range(0, days).Select(i => new WeatherRecord()
            {
                ForecastDateTime = from.AddDays(i),
                MadeOnDateTime = from.AddDays(i),
                Source = SourceName,
                Region = dataSource.RegionName,
                LeadDays = 0,

                TemperatureMin = openMeteoResponse.daily.temperature_2m_min.ElementAt(i),
                TemperatureMax = openMeteoResponse.daily.temperature_2m_max.ElementAt(i),
                TemperatureAvg = openMeteoResponse.daily.temperature_2m_mean.ElementAt(i),
                WindSpeed = openMeteoResponse.daily.wind_speed_10m_max.ElementAt(i),
                WindGust = openMeteoResponse.daily.wind_gusts_10m_max.ElementAt(i),
                Precipitation = openMeteoResponse.daily.precipitation_sum.ElementAt(i),
                AtmosphericPressureAvg = openMeteoResponse.daily.surface_pressure_mean.ElementAt(i) * 0.750064M,
                Humidity = openMeteoResponse.daily.relative_humidity_2m_mean.ElementAt(i)
            });     
        }


        private string GetDateString(DateOnly date)
        {
            return string.Format("{0:d4}-{1:d2}-{2:d2}", date.Year, date.Month, date.Day);
        }
    }

    public class OpenMeteoResponse
    {
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public decimal generationtime_ms { get; set; }
        public int utc_offset_seconds {  get; set; }
        public string timezone {  get; set; }
        public string timezone_abbreviation {  get; set; }
        public decimal elevation { get; set; }
        public DailyUnits daily_units {  get; set; }
        public Daily daily { get; set; }

        public class DailyUnits
        {
            public string time { get; set; }
            public string temperature_2m_max { get; set; }
            public string temperature_2m_min { get; set; }
            public string temperature_2m_mean { get; set; }
            public string precipitation_sum { get; set; }
        }

        public class Daily
        {
            public IEnumerable<string> time { get; set; }
            public IEnumerable<decimal?> temperature_2m_max { get; set; }
            public IEnumerable<decimal?> temperature_2m_min { get; set; }
            public IEnumerable<decimal?> temperature_2m_mean { get; set; }
            public IEnumerable<decimal?> precipitation_sum { get; set; }
            public IEnumerable<decimal?> wind_speed_10m_max { get; set; }
            public IEnumerable<decimal?> wind_gusts_10m_max { get; set; }
            public IEnumerable<decimal?> surface_pressure_mean { get; set; }
            public IEnumerable<decimal?> relative_humidity_2m_mean { get; set; }

        }
    }
}

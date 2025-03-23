using DataLayer.Entities;
using System.Text.Json;

namespace ServiceLayer.Services.Parsing
{
    public class ArchiveParser
    {
        private const string _url = "https://archive-api.open-meteo.com/v1/archive";

        public async Task<IEnumerable<WeatherRecord>> GetArchiveRecordsAsync(DateTime from, DateTime to)
        {
            string queryString = $"latitude=55&" +
                $"longitude=37&" +
                $"start_date={GetDateString(DateOnly.FromDateTime(from))}&" +
                $"end_date={GetDateString(DateOnly.FromDateTime(to))}&" +
                $"daily=temperature_2m_max,temperature_2m_min,temperature_2m_mean,precipitation_sum&" +
                $"timezone=Europe%2FMoscow";
            string requestString = $"{_url}?{queryString}";
            using HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(requestString);
            if (!response.IsSuccessStatusCode) throw new Exception($"Archive request failed with code {response.StatusCode}" +
                $"{Environment.NewLine}{await response.Content.ReadAsStringAsync()}" +
                $"{Environment.NewLine}Request was: {requestString}");

            //Console.WriteLine($" Received message:{Environment.NewLine}{await response.Content.ReadAsStringAsync()}");
            OpenMeteoResponse? arhive = JsonSerializer.Deserialize<OpenMeteoResponse>(response.Content.ReadAsStream());
            if (arhive == null) throw new Exception($"Archive record deserilization failed;" +
                $" Received message:{Environment.NewLine}{await response.Content.ReadAsStringAsync()}");
           
            int size = (int)((to - from).TotalDays)+1;
            List<WeatherRecord> records = [];
            for(int i = 0; i < size; i++)
            {
                WeatherRecord record = new WeatherRecord()
                {
                    TemperatureMin = arhive.daily.temperature_2m_min.ElementAt(i),
                    TemperatureMax = arhive.daily.temperature_2m_max.ElementAt(i),
                    TemperatureAvg = arhive.daily.temperature_2m_mean.ElementAt(i),
                    Precipitation = arhive.daily.precipitation_sum.ElementAt(i),
                    ForecastDateTime = from.AddDays(i),
                    MadeOnDateTime = from.AddDays(i),
                };
                records.Add(record);
            }
            return records;
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
            public IEnumerable<decimal?>? temperature_2m_mean { get; set; }
            public IEnumerable<decimal?> precipitation_sum { get; set; }
            public IEnumerable<decimal?> wind_speed_10m_max { get; set; }
            public IEnumerable<decimal?> wind_gusts_10m_max { get; set; }

        }
    }
}

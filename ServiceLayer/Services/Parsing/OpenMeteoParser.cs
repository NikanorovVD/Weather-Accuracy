using DataLayer.Entities;
using System.Text.Json;

namespace ServiceLayer.Services.Parsing
{
    public class OpenMeteoParser
    {
        string _url = "https://api.open-meteo.com/v1/forecast";
        public const int MaxDays = 16;
        public async Task<IEnumerable<WeatherRecord>> GetWeaterRecordsAsync(string regionName, string latitude, string longitude, string timezone)
        {
            string requestString = QueryHelper.CreateQuery(_url,
                ("latitude", latitude),
                ("longitude", longitude),
                ("daily", "temperature_2m_max,wind_speed_10m_max,temperature_2m_min,temperature_2m_mean,precipitation_sum,wind_gusts_10m_max"),
                ("timezone", timezone),
                ("forecast_days", MaxDays.ToString())
                );
            using HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(requestString);
            if (!response.IsSuccessStatusCode) throw new Exception($"Forecast request failed with code {response.StatusCode}" +
                $"{Environment.NewLine}{await response.Content.ReadAsStringAsync()}" +
                $"{Environment.NewLine}Request was: {requestString}");

            OpenMeteoResponse? openMeteoResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(response.Content.ReadAsStream());
            if (openMeteoResponse == null) throw new Exception($"Archive record deserilization failed;" +
                $" Received message:{Environment.NewLine}{await response.Content.ReadAsStringAsync()}");

            List<WeatherRecord> records = [];
            for (int i = 0; i < MaxDays; i++)
            {
                WeatherRecord record = new WeatherRecord()
                {
                    ForecastDateTime = DateTime.UtcNow.AddDays(i),
                    Source = "OpenMeteo",
                    Region = regionName,

                    TemperatureMin = openMeteoResponse.daily.temperature_2m_min.ElementAt(i),
                    TemperatureMax = openMeteoResponse.daily.temperature_2m_max.ElementAt(i),
                    TemperatureAvg = openMeteoResponse.daily.temperature_2m_mean.ElementAt(i),
                    WindSpeed = openMeteoResponse.daily.wind_speed_10m_max.ElementAt(i),
                    WindGust = openMeteoResponse.daily.wind_gusts_10m_max.ElementAt(i),
                    Precipitation = openMeteoResponse.daily.precipitation_sum.ElementAt(i)
                };
                records.Add(record);
            }
            return records;
        }
    }
}

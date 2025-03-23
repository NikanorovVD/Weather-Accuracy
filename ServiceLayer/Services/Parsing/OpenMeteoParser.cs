using DataLayer.Entities;
using ServiceLayer.Models.Errors;
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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestString);
            HttpResponseMessage response = await client.GetAsync(requestString);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new RequestException("Error status code", requestString, response.StatusCode, content);

            OpenMeteoResponse? openMeteoResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(content) 
                ?? throw new RequestException("Error status code", requestString, response.StatusCode, content);

            return Enumerable.Range(0, MaxDays).Select(i => new WeatherRecord()
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
            });
        }
    }
}

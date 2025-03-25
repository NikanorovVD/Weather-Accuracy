using DataLayer.Entities;
using ServiceLayer.Models;
using ServiceLayer.Models.Errors;
using System.Globalization;
using System.Text.Json;

namespace ServiceLayer.Services.Parsing
{
    public class OpenMeteoParser: IWeaterParser
    {
        private const string _url = "https://api.open-meteo.com/v1/forecast";
        public const int MaxDays = 16;
        private static readonly CultureInfo _culture = CultureInfo.CreateSpecificCulture("en-US");

        public string SourceName => "OpenMeteo";

        public async Task<IEnumerable<WeatherRecord>> GetWeaterRecordsAsync(DataSource dataSource)
        {
            string requestString = QueryHelper.CreateQuery(_url,
                ("latitude", dataSource.Latitude.ToString(_culture)),
                ("longitude", dataSource.Longitude.ToString(_culture)),
                ("daily", "temperature_2m_max,wind_speed_10m_max,temperature_2m_min,temperature_2m_mean,precipitation_sum,wind_gusts_10m_max"),
                ("timezone", dataSource.Timezone),
                ("forecast_days", MaxDays.ToString())
                );

            using HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestString);
            HttpResponseMessage response = await client.GetAsync(requestString);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new RequestException("Error status code", requestString, response.StatusCode, content);

            OpenMeteoResponse? openMeteoResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(content) 
                ?? throw new RequestException("Error deserilizing data", requestString, response.StatusCode, content);

            return Enumerable.Range(0, MaxDays).Select(i => new WeatherRecord()
            {
                ForecastDateTime = DateTime.UtcNow.AddDays(i),
                Source = SourceName,
                Region = dataSource.RegionName,

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

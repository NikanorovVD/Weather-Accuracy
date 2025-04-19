using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DataLayer.Entities;
using ServiceLayer.Models;
using ServiceLayer.Models.Errors;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ServiceLayer.Services.Parsing
{
    public class YandexParser : IWeaterParser
    {
        public const int MaxDays = 10;
        public string SourceName => "Yandex";
        private const string _serverUrl = @"https://yandex.ru/pogoda/month";
        private static readonly CultureInfo _culture = CultureInfo.CreateSpecificCulture("en-US");
        public async Task<IEnumerable<WeatherRecord>> GetWeaterRecordsAsync(DataSource dataSource)
        {
            string url = GetUrl(dataSource);

            using HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new RequestException("Error status code", url, response.StatusCode, content);

            var parser = new HtmlParser();
            var document = parser.ParseDocument(content);

            int startIndex = (int)DateTime.Now.DayOfWeek - 1;
            if (startIndex == -1) startIndex = 6;

            decimal[] maxT = ParseParameter(document, ".climate-calendar-day__detailed-basic-temp-day .temp__value", startIndex)
                 .Select(s => decimal.Parse(s.Replace('−', '-')))
                 .ToArray();

            decimal[] minT = ParseParameter(document, ".climate-calendar-day__detailed-basic-temp-night .temp__value", startIndex)
                .Select(s => decimal.Parse(s.Replace('−', '-')))
                .ToArray();

            decimal[] pressure = ParseParameter(document, ".climate-calendar-day__detailed-data-table-cell_value_yes:contains('мм рт. ст.')", startIndex)
                .Select(s => decimal.Parse(Regex.Match(s, @"\d+").Value))
                .ToArray();

            decimal[] wind = ParseParameter(document, ".climate-calendar-day__detailed-data-table-cell_value_yes:contains('м/с')", startIndex)
                .Select(s => decimal.Parse(Regex.Match(s, @"\d+(\.\d+)?").Value, _culture))
                .ToArray();

            decimal[] humidity = ParseParameter(document, ".climate-calendar-day__detailed-data-table-cell_value_yes:contains('%')", startIndex)
                .Select(s => decimal.Parse(Regex.Match(s, @"\d+").Value))
                .ToArray();

            return Enumerable.Range(0, MaxDays).Select(i => new WeatherRecord()
            {
                ForecastDateTime = DateTime.UtcNow.AddDays(i),
                Source = SourceName,
                Region = dataSource.RegionName,
                LeadDays = i,

                TemperatureMax = maxT[i],
                TemperatureMin = minT[i],
                TemperatureAvg = (maxT[i] + minT[i]) / 2,
                AtmosphericPressureAvg = pressure[i],
                WindSpeed = wind[i],
                Humidity = humidity[i]
            });
        }

        private static string GetUrl(DataSource dataSource)
        {
            return QueryHelper.CreateQuery(_serverUrl,
                ("lat", dataSource.Latitude.ToString(_culture)),
                ("lon", dataSource.Longitude.ToString(_culture))
                );
        }

        private IEnumerable<string?> ParseParameter(IHtmlDocument document, string selector, int startIndex)
        {
            return document
                .QuerySelectorAll("tr.climate-calendar__row")
                .ToArray()[1..]
                .SelectMany(tr => tr.GetElementsByClassName(" climate-calendar__cell"))
                .ToArray()[startIndex..(startIndex+MaxDays)]
                .Select(c => c.QuerySelector(selector))
                .Select(c => c?.TextContent);
        }
    }
}

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DataLayer.Entities;
using ServiceLayer.Models;
using ServiceLayer.Models.Errors;
using System.Globalization;

namespace ServiceLayer.Services.Parsing
{
    public class MeteoblueParser : IWeaterParser
    {
        public string SourceName => "Meteoblue";
        public int MaxDays = 14;
        private const string _serverUrl = @"https://www.meteoblue.com/ru/погода/14-дней";
        private static readonly CultureInfo _culture = CultureInfo.CreateSpecificCulture("en-US");

        public async Task<IEnumerable<WeatherRecord>> GetWeaterRecordsAsync(DataSource dataSource)
        {
            string url = GetUrl(dataSource.Meteoblue);

            using HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new RequestException("Error status code", url, response.StatusCode, content);

            var parser = new HtmlParser();
            var document = parser.ParseDocument(content);
            decimal[] maxTemperatures = ParseColumn(document, 3).Select(x => (decimal)x).ToArray();
            decimal[] minTemperatures = ParseColumn(document, 4).Select(x => (decimal)x).ToArray();
            decimal[] precipitations = ParsePrecipitations(document);

            return Enumerable.Range(0, MaxDays).Select(i => new WeatherRecord()
            {
                ForecastDateTime = DateTime.UtcNow.AddDays(i),
                Source = SourceName,
                Region = dataSource.RegionName,

                TemperatureAvg = (maxTemperatures[i] + minTemperatures[i]) / 2,
                TemperatureMax = maxTemperatures[i],
                TemperatureMin = minTemperatures[i],
                Precipitation = precipitations[i]
            });
        }

        private static string GetUrl(string meteoblueRegion)
        {
            return $"{_serverUrl}/{meteoblueRegion}";
        }

        private int[] ParseColumn(IHtmlDocument document, int columnIndex)
        {
            return document.QuerySelector("table.forecast-table")
                .QuerySelector("tbody")
                .QuerySelectorAll("tr")[columnIndex]
                .QuerySelectorAll("td")
                .Select(c => c.TextContent.Trim(' ', '°'))
                .Select(int.Parse)
                .ToArray();
        }

        private decimal[] ParsePrecipitations(IHtmlDocument document)
        {
            var canvas = document.GetElementById("canvas_14_days_forecast_precipitations");
            string preticipationString = canvas.GetAttribute("data-precipitation");
            return preticipationString
                .Trim('[', ']')
                .Split(',')
                .Select(s => decimal.Parse(s.ToString(), _culture))
                .ToArray();
        }
    }
}

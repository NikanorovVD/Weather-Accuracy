using AngleSharp.Html.Parser;
using DataLayer.Entities;
using ServiceLayer.Models;
using ServiceLayer.Models.Errors;

namespace ServiceLayer.Services.Parsing
{
    public class YandexParser : IWeaterParser
    {
        public string SourceName => "Yandex";
        private const string _serverUrl = @"https://yandex.ru/pogoda/month";
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

            var trs = document
                .QuerySelectorAll("tr.climate-calendar__row").ToList();

            var cells = document
                .QuerySelectorAll("tr.climate-calendar__row")
                .SelectMany(tr => tr.QuerySelectorAll("td"))
                .ToList();
            return [];
        }

        private static string GetUrl(DataSource dataSource)
        {
            return QueryHelper.CreateQuery(_serverUrl,
                ("lat", dataSource.Latitude.ToString()),
                ("lon", dataSource.Longitude.ToString())
                );
        }
    }
}

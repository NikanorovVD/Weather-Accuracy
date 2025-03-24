using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using DataLayer.Entities;
using ServiceLayer.Models;
using ServiceLayer.Models.Errors;


namespace ServiceLayer.Services.Parsing
{
    public class RP5Parser : IWeaterParser
    {
        public string SourceName => "RP5";
        private const string _serverUrl = @"https://rp5.ru";

        public async Task<IEnumerable<WeatherRecord>> GetWeaterRecordsAsync(DataSource dataSource)
        {
            string url = GetUrl(dataSource.RP5);

            using HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new RequestException("Error status code", url, response.StatusCode, content);

            var parser = new HtmlParser();
            var document = parser.ParseDocument(content);
            IElement table = document.GetElementById("forecastTable")!;

            int colspan = int.Parse(table
                .QuerySelector(".forecastDate")!
                .QuerySelector("td")!
                .GetAttribute("colspan")!);
            int startIndex = colspan - 1;
     

            decimal[] avgT = GetColumn(table, "Температура", "div.t_0", startIndex).Select(x => (decimal)x.Average()).ToArray();
            decimal[] minT = GetColumn(table, "Температура", "div.t_0", startIndex).Select(x => (decimal)x.Min()).ToArray();
            decimal[] maxT = GetColumn(table, "Температура", "div.t_0", startIndex).Select(x => (decimal)x.Max()).ToArray();
            decimal[] pres = GetColumn(table, "Давление", "div.p_0", startIndex).Select(x => (decimal)x.Average()).ToArray();
            decimal[] wind = GetColumn(table, "Ветер: скорость", "div.wv_0", startIndex).Select(x => (decimal)x.Average()).ToArray();
            decimal[] windGust = GetColumn(table, "порывы", "div.wv_0", startIndex).Select(x => (decimal)x.Average()).ToArray();
            decimal[] humidity = GetColumn(table, "Влажность", null, startIndex).Select(x => (decimal)x.Average()).ToArray();


            return Enumerable.Range(0, avgT.Length).Select(i => new WeatherRecord()
            {
                ForecastDateTime = DateTime.UtcNow.AddDays(i+1),
                Source = SourceName,
                Region = dataSource.RegionName,

                TemperatureAvg = avgT[i],
                TemperatureMax = minT[i],
                TemperatureMin = maxT[i],
                Precipitation = null,
                AtmosphericPressureAvg = pres[i],
                Humidity = humidity[i],
                WindSpeed = wind[i],
                WindGust = windGust[i],
            });
        }

        private static string GetUrl(string rp5_region)
        {
            return @$"{_serverUrl}/{rp5_region}";
        }

        int[][] GetColumn(IElement table, string columnContent, string? containerClass, int startIndex)
        {
            var cells = table
              .QuerySelectorAll("tr")
              .Where(
                tr => tr.QuerySelectorAll("a").Any(el => el.TextContent.Contains(columnContent)) ||
                (tr.QuerySelector("td.title")!= null && tr.QuerySelector("td.title").TextContent.Contains(columnContent))
                )
              .FirstOrDefault()!
              .Children.ToList()[1..];

            int endIndex = cells.Count / 4 * 4 + 1;

            if(containerClass != null) cells = cells.Select(el => el.QuerySelector(containerClass)).ToList();
            int[] values =  cells
                .Select(el => el.TextContent.Trim())
                .ToArray()[startIndex..endIndex]
                .Select(x => int.TryParse(x, out int y) ? y : 0)
                .ToArray();


            return values
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 4)
                .Select(g => g.Select(x => x.Value).ToArray())
                .ToArray();
        }
    }
}

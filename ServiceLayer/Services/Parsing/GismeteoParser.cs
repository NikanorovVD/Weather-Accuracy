using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using DataLayer.Entities;
using ServiceLayer.Models;
using ServiceLayer.Models.Errors;

namespace ServiceLayer.Services.Parsing
{
    public class GismeteoParser: IWeaterParser
    {      
        public const int MaxDays = 10;
        private static IHtmlCollection<IElement> _rows;
        private const string _serverUrl = @"https://www.gismeteo.ru";
        public string SourceName => "Gismeteo";

        public async Task<IEnumerable<WeatherRecord>> GetWeaterRecordsAsync(DataSource dataSource)
        {    
            string url = GetUrl(dataSource.Gismeteo);

            using HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) 
                throw new RequestException("Error status code", url, response.StatusCode, content);

            try
            {
                var parser = new HtmlParser();
                var document = parser.ParseDocument(content);
                IElement table = document.GetElementsByClassName("widget-items js-scroll-item").Single();
                _rows = table.Children;

                decimal[] temprsAvg = GetParameter(GetTemperatureAvg);
                decimal?[] temprsMax = GetParameter(GetTemperatureMax);
                decimal?[] temprsMin = GetParameter(GetTemperatureMin);
                decimal[] preticipations = GetParameter(GetPreticipation);
                decimal[] pressureMax = GetParameter(GetMaxPressure);
                decimal[] pressureMin = GetParameter(GetMinPressure);
                decimal[] humidity = GetParameter(GetHumidity);
                uint[] uv = GetParameter(GetUVIndex);
                uint[] geomagnetic = GetParameter(GetGeomagneticActivity);
                decimal[] windSpeed = GetParameter(GetWindSpeed);
                decimal?[] windGust = GetParameter(GetWindGust);
                string[] windDirection = GetParameter(GetWindDirection);
                string[] coudCover = GetParameter(GetCloudCover);

                DateTime today = DateTime.UtcNow.Date;

                return Enumerable.Range(0, MaxDays).Select(i => new WeatherRecord()
                {
                    ForecastDateTime = today.AddDays(i),
                    Source = SourceName,
                    Region = dataSource.RegionName,
                    LeadDays = i,

                    TemperatureAvg = temprsAvg[i],
                    TemperatureMax = temprsMax[i],
                    TemperatureMin = temprsMin[i],
                    Precipitation = preticipations[i],
                    AtmosphericPressureAvg = (pressureMin[i] + pressureMax[i]) / 2,
                    Humidity = humidity[i],
                    WindSpeed = windSpeed[i],
                    WindGust = windGust[i],
                });
            }
            catch (Exception ex) 
            {
                throw new RequestException($"Error parsing data: {ex}", url, response.StatusCode, content);
            }
        }

        private static string GetUrl(string region)
        {
            return @$"{_serverUrl}/{region}/10-days/";
        }

        private static T[] GetParameter<T>(Func<IEnumerable<T>> pasingFunc)
        {
            var result = pasingFunc().ToArray();
            if (result.Length != MaxDays) throw new Exception("Invalid data count");
            return result;
        }

        private static IEnumerable<decimal> GetTemperatureAvg()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "temperature-avg").Single();
            var values = tempr_row
                .GetElementsByClassName("chart")
                .Single()
                .QuerySelectorAll("temperature-value")
                .Select(v => v.GetAttribute("value"));
            return values.Select(t => decimal.Parse(t));
        }
        private static IEnumerable<decimal?> GetTemperatureMax()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "temperature-air").Single();
            var values = tempr_row
                .GetElementsByClassName("chart")
                .Single()
                .GetElementsByClassName("value")
                .Select(v => v.GetElementsByClassName("maxt").SingleOrDefault())
                .Select(e => e?.GetElementsByTagName("temperature-value").Single())
                .Select(v => v?.GetAttribute("value"));
            return values.Select<string?, decimal?>(t => t == null ? null : decimal.Parse(t));
        }
        private static IEnumerable<decimal?> GetTemperatureMin()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "temperature-air").Single();
            var values = tempr_row
                .GetElementsByClassName("chart")
                .Single()
                .GetElementsByClassName("value")
                .Select(v => v.GetElementsByClassName("mint").SingleOrDefault())
                .Select(e => e?.GetElementsByTagName("temperature-value").Single())
                .Select(v => v?.GetAttribute("value"));
            return values.Select<string?, decimal?>(t => t == null ? null : decimal.Parse(t));
        }

        private static IEnumerable<decimal> GetPreticipation()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "precipitation-bars").Single();
            var values = tempr_row
                .GetElementsByClassName("item-unit")
                .Select(v => v.TextContent);
            return values.Select(t => decimal.Parse(t));
        }

        private static IEnumerable<decimal> GetMaxPressure()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "pressure").Single();
            var values = tempr_row
                .GetElementsByClassName("chart")
                .Single()
                .GetElementsByClassName("maxt")
                .SelectMany(e => e.QuerySelectorAll("pressure-value"))
                .Select(v => v.GetAttribute("value"));
            return values.Select(t => decimal.Parse(t));
        }

        private static IEnumerable<decimal> GetMinPressure()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "pressure").Single();
            var values = tempr_row
                .GetElementsByClassName("chart")
                .Single()
                .GetElementsByClassName("mint")
                .SelectMany(e => e.QuerySelectorAll("pressure-value"))
                .Select(v => v.GetAttribute("value"));
            return values.Select(t => decimal.Parse(t));
        }

        private static IEnumerable<decimal> GetHumidity()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "humidity-avg").Single();
            return tempr_row
                .GetElementsByClassName("row-item")
                .Select(v => v.TextContent)
                .Select(t => decimal.Parse(t));
        }
        private static IEnumerable<uint> GetUVIndex()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "radiation").Single();
            return tempr_row
                .GetElementsByClassName("row-item")
                .Select(v => v.TextContent)
                .Select(t => uint.Parse(t));
        }

        private static IEnumerable<uint> GetGeomagneticActivity()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "geomagnetic").Single();
            return tempr_row
                .GetElementsByClassName("row-item")
                .Select(r => r.GetElementsByClassName("item")
                    .Single())
                .Select(v => v.TextContent)
                .Select(t => uint.Parse(t));
        }

        private static IEnumerable<decimal> GetWindSpeed()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "wind-speed").Single();
            return tempr_row
                .GetElementsByClassName("row-item")
                .Select(r => r.GetElementsByTagName("speed-value").Single())
                .Select(v => v.GetAttribute("value"))
                .Select(t => decimal.Parse(t));
        }
        private static IEnumerable<decimal?> GetWindGust()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "wind-gust").Single();
            var rowItems = tempr_row.GetElementsByClassName("row-item");
            return rowItems
                .Select(r => r.GetElementsByTagName("speed-value").SingleOrDefault())
                .Select(v => v?.GetAttribute("value"))
                .Select<string?, decimal?>(t => t == null ? null : decimal.Parse(t));
        }

        private static IEnumerable<string> GetWindDirection()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "wind-direction").Single();
            return tempr_row
                .GetElementsByClassName("row-item")
                .Select(r => r.GetElementsByClassName("direction").Single())
                .Select(v => v.TextContent);
        }

        private static IEnumerable<string> GetCloudCover()
        {
            IElement tempr_row = _rows.Where(r => r.GetAttribute("data-row") == "icon-tooltip").Single();
            return tempr_row
                .GetElementsByClassName("row-item")
                .Select(v => v.GetAttribute("data-tooltip")!);
        }
    }
}

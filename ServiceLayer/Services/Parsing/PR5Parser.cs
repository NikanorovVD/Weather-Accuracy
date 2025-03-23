using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace ServiceLayer.Services.Parsing
{
    public class PR5Parser
    {
        static async Task RP5(string[] args)
        {
            string url = @"https://rp5.ru/Погода_в_Зеленограде";
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

            HttpClient client = new HttpClient();
            HttpResponseMessage responseMessage = await client.SendAsync(httpRequest);

            //Console.WriteLine($"Status: {(int)responseMessage.StatusCode} {responseMessage.StatusCode}");

            string content = await responseMessage.Content.ReadAsStringAsync();

            var parser = new HtmlParser();
            var document = parser.ParseDocument(content);
            IElement table = document.GetElementById("forecastTable") ?? throw new Exception("forecastTable not found");
            IElement tepmerature_row = table
                .QuerySelectorAll("tr")
                .Where(tr => tr.QuerySelectorAll("a")
                    .Any(el => el.TextContent == "Температура"))
                .FirstOrDefault()!;
            {
                foreach (
                    IElement temperature in tepmerature_row
                    .GetElementsByClassName("t_0")
                    .SelectMany(e => e.QuerySelectorAll("b"))
                    )
                {
                    //Console.WriteLine(temperature.TextContent);
                }
            }
        }
    }
}

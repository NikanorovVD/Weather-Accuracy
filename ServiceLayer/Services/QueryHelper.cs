namespace ServiceLayer.Services
{
    public static class QueryHelper
    {
        public static string CreateQuery(string url, params (string Key, string Value)[] args)
        {
            string query = string.Join("&", args.Select(arg => $"{arg.Key}={arg.Value}"));
            return $"{url}?{query}";
        }
    }
}

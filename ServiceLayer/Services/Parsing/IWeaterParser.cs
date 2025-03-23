using DataLayer.Entities;
using ServiceLayer.Models;

namespace ServiceLayer.Services.Parsing
{
    public interface IWeaterParser
    {
        public Task<IEnumerable<WeatherRecord>> GetWeaterRecordsAsync(DataSource dataSource);
        public string SourceName {  get; }
    }
}

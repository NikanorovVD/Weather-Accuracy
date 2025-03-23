using DataLayer.Entities;

namespace ServiceLayer.Services.Parsing
{
    public interface IWeaterParser
    {
        public Task<IEnumerable<WeatherRecord>> GetWeaterRecordsAsync();
    }
}

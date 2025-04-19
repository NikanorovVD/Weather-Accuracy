using DataLayer;
using DataLayer.Entities;
using ServiceLayer.Models;
using ServiceLayer.Services.Parsing;

namespace ServiceLayer.Services
{
    public class FactRecordsService
    {
        private readonly ArchiveParser _archiveParser;
        private readonly AppDbContext _appDbContext;
        public FactRecordsService(AppDbContext appDbContext)
        {
            _archiveParser = new ArchiveParser();
            _appDbContext = appDbContext;
        }

        public async Task CreateFactRecords(DataSource dataSource, DateTime from)
        {
            var records = GetFactRecords(dataSource, from, DateTime.Now.AddDays(-1));
            _appDbContext.AddRange(records);
            //_appDbContext.SaveChanges();
        }

        public async Task<IEnumerable<WeatherRecord>> GetFactRecords(DataSource dataSource, DateTime from, DateTime to)
        {
            return await _archiveParser.GetArchiveRecordsAsync(dataSource, from, to);
        }
    }
}

using DataLayer;
using DataLayer.Entities;
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

        public async Task CreateFactRecords(DateTime from)
        {
            var records = GetFactRecords(from, DateTime.Now.AddDays(-1));
            _appDbContext.AddRange(records);
            //_appDbContext.SaveChanges();
        }

        private async Task<IEnumerable<WeatherRecord>> GetFactRecords(DateTime from, DateTime to)
        {
            return await _archiveParser.GetArchiveRecordsAsync(from, to);
        }
    }
}

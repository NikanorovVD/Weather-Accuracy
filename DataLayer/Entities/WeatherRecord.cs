namespace DataLayer.Entities
{
    public class WeatherRecord
    {
        public WeatherRecord()
        {
            MadeOnDateTime = DateTime.UtcNow;
        }
        public int WeatherRecordID {  get; set; }
        public DateTime MadeOnDateTime { get; set; }
        public DateTime ForecastDateTime { get; set; }
        public string Source { get; set; }
        public string Region { get; set; }
        public decimal? TemperatureAvg { get; set; }
        public decimal? TemperatureMax { get; set; }
        public decimal? TemperatureMin { get; set; }
        public decimal? Precipitation { get; set; }
        public string? CloudCover { get; set; }
        public decimal? WindSpeed { get; set; }
        public decimal? WindGust { get; set; }
        public string? WindDirection { get; set; }
        public decimal? Humidity { get; set; }
        public decimal? AtmosphericPressureMax { get; set; }
        public decimal? AtmosphericPressureMin { get; set; }
        public uint? UVIndex { get; set; }
        public uint? GeomagneticActivity { get; set; }
    }
}

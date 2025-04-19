namespace ServiceLayer.Models
{
    public class ForecastAccuracy
    {
        public string Source { get; set; }
        public string Region { get; set; }
        public int LeadDays { get; set; }
        public ForecastDeviation Deviation { get; set; }
    }
}

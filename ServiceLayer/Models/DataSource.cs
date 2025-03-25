namespace ServiceLayer.Models
{
    public class DataSource
    {
        public string RegionName {  get; set; }
        public decimal Latitude {  get; set; }
        public decimal Longitude {  get; set; }
        public string Gismeteo {  get; set; }
        public string Timezone { get; set; }
        public string RP5 {  get; set; }
        public string Meteoblue {  get; set; }
    }
}

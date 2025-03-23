using DataLayer.Entities;

namespace ServiceLayer.Models
{
    public class ForecastDeviation
    {
        public DateTime ForecastDateTime { get; set; }
        public DateTime DateTime { get; set; }
        public decimal? TemperatureAvg { get; set; }
        public decimal? TemperatureMax { get; set; }
        public decimal? TemperatureMin { get; set; }
        public decimal? Precipitation { get; set; }
        public decimal? WindSpeed { get; set; }
        public decimal? WindGust { get; set; }
        public decimal? Humidity { get; set; }
        public decimal? AtmosphericPressureMax { get; set; }
        public decimal? AtmosphericPressureMin { get; set; }
        public uint? UVIndex { get; set; }
        public uint? GeomagneticActivity { get; set; }

        public ForecastDeviation(WeatherRecord forecast, WeatherRecord fact)
        {
            ForecastDateTime = forecast.MadeOnDateTime;
            DateTime = fact.ForecastDateTime;

            if (forecast.TemperatureAvg == null || fact.TemperatureAvg == null) TemperatureAvg = null;
            else TemperatureAvg = forecast.TemperatureAvg - fact.TemperatureAvg;

            if (forecast.TemperatureMax == null || fact.TemperatureMax == null) TemperatureMax = null;
            else TemperatureMax = forecast.TemperatureMax - fact.TemperatureMax;

            if (forecast.TemperatureMin == null || fact.TemperatureMin == null) TemperatureMin = null;
            else TemperatureMin = forecast.TemperatureMin - fact.TemperatureMin;

            if (forecast.Precipitation == null || fact.Precipitation == null) Precipitation = null;
            else Precipitation = forecast.Precipitation - fact.Precipitation;

            if (forecast.WindSpeed == null || fact.WindSpeed == null) WindSpeed = null;
            else WindSpeed = forecast.WindSpeed - fact.WindSpeed;

            if (forecast.WindGust == null || fact.WindGust == null) WindGust = null;
            else WindGust = forecast.WindGust - fact.WindGust;

            if (forecast.Humidity == null || fact.Humidity == null) Humidity = null;
            else Humidity = forecast.Humidity - fact.Humidity;

            if (forecast.AtmosphericPressureMax == null || fact.AtmosphericPressureMax == null) AtmosphericPressureMax = null;
            else AtmosphericPressureMax = forecast.AtmosphericPressureMax - fact.AtmosphericPressureMax;

            if (forecast.AtmosphericPressureMin == null || fact.AtmosphericPressureMin == null) AtmosphericPressureMin = null;
            else AtmosphericPressureMin = forecast.AtmosphericPressureMin - fact.AtmosphericPressureMin;

            if (forecast.UVIndex == null || fact.UVIndex == null) UVIndex = null;
            else UVIndex = forecast.UVIndex - fact.UVIndex;

            if (forecast.GeomagneticActivity == null || fact.GeomagneticActivity == null) GeomagneticActivity = null;
            else GeomagneticActivity = forecast.GeomagneticActivity - fact.GeomagneticActivity;
        }
    }
}

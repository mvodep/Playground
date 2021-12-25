using System;
using System.Text.Json;

namespace HelloWorld
{
    public interface IWeatherForecast
    {
        void makeASunnyDay();
    }

    public class WeatherForecast : IWeatherForecast
    {
        public DateTimeOffset Date { get; set; }
        public int TemperatureCelsius { get; set; }
        public string Summary { get; set; }

        public void makeASunnyDay()
        {
            TemperatureCelsius = 39;
            Summary = "Sunny";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var weatherForecast = new WeatherForecast
            {
                Date = DateTime.Parse("2019-08-01"),
                TemperatureCelsius = 25,
                Summary = "Hot"
            };

            string jsonString = JsonSerializer.Serialize(weatherForecast);

            Console.WriteLine(jsonString);
        }
    }
}

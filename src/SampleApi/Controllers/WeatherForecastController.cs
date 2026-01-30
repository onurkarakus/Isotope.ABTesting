using Microsoft.AspNetCore.Mvc;
using Isotope.ABTesting.Abstractions;
using Isotope.ABTesting.Fallbacks;
using Isotope.ABTesting.Strategies;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IABTestClient _aBTestClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IABTestClient aBTestClient)
        {
            _logger = logger;
            _aBTestClient = aBTestClient;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var result = await _aBTestClient.Experiment("WeatherForecastExperiment")
                .WithVariants(("VariantA", 10), ("VariantB", 90))
                .UseAlgorithm<DeterministicHashStrategy>()
                .OnFailure(Fallback.ToVariant("VariantB"))
                .GetVariantAsync("user-id-123");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}

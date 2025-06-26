using Microsoft.AspNetCore.Mvc;
using System.Net.Http; // Needed for HttpClient
using System.Threading.Tasks; // Needed for async programming

namespace MyWebApiProject.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly HttpClient _httpClient; // Add HttpClient for making HTTP requests

    // Modify constructor to inject HttpClient
    public WeatherForecastController(ILogger<WeatherForecastController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    // New action method to fetch weather data from external URL
    [HttpGet("from-url")]
    public async Task<ActionResult<string>> GetFromUrl([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest("URL parameter is required");
        }

        try
        {
            // Make GET request to the provided URL
            var response = await _httpClient.GetAsync(url);
            
            // Ensure we got a successful response
            response.EnsureSuccessStatusCode();
            

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching weather data from {Url}", url);
            return StatusCode(StatusCodes.Status502BadGateway, $"Error fetching data from external service: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather data");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }
}
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

    [HttpGet("success")]
    public IActionResult GetSuccess()
    {
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        return Ok(forecast);
    }

    [HttpPost("create")]
    public IActionResult CreateForecast([FromBody] WeatherForecast forecast)
    {
        _logger.LogInformation($"Created forecast for {forecast.Date}");
        
        return CreatedAtAction(nameof(GetById), new { id = 123 }, forecast); // 201
    }

    [HttpGet("validate")]
    public IActionResult GetWithValidation([FromQuery] int days)
    {
        if (days <= 0 || days > 30)
        {
            return BadRequest("Days must be between 1 and 30"); // 400
        }

        return Ok($"Valid request for {days} days");
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        if (id != 123)
        {
            return NotFound($"Forecast with ID {id} not found"); // 404
        }

        return Ok(new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = 25,
            Summary = "Warm"
        });
    }

    [HttpGet("maintenance")]
    public IActionResult MaintenanceCheck()
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, 
            new ProblemDetails
            {
                Title = "Service Temporarily Unavailable",
                Detail = "Please try again in 30 minutes",
                Status = 503
            }); // 503
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
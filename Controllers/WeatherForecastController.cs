using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using System.Net.Http; // Needed for HttpClient
using System.Threading.Tasks; // Needed for async programming
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _config;

    // Modify constructor to inject HttpClient
    public WeatherForecastController(ILogger<WeatherForecastController> logger, HttpClient httpClient, IFeatureManager featureManager, IConfiguration config)
    {
        _logger = logger;
        _httpClient = httpClient;
        _featureManager = featureManager;
        _config = config;
    }

    [HttpGet("success")]
    // [Produces("application/json")]
    public IActionResult GetSuccess([FromQuery] string? format)
    {
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        if (string.IsNullOrEmpty(format))
        {
            return StatusCode(406, "Format not specified");
        }

        return format.ToLower() switch
        {
            "json" => Ok(forecast),
            "xml" => new ObjectResult(forecast)
            {
                ContentTypes = new MediaTypeCollection { "application/xml" }
            },
            _ => StatusCode(406, "Unsupported format")
        };
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

    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Admin() => Ok("Admin secret data");

[HttpGet("get-token")]
[AllowAnonymous]
public IActionResult GetToken([FromQuery] string role = "User")
{
    var token = GenerateTokenWithRole("test-user", role);
    return Ok(new { token });
}

private string GenerateTokenWithRole(string username, string role)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role)
    };

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

    [HttpGet("timed-endpoint")]
    [ServiceFilter(typeof(ExecutionTimeFilter))]
    [FeatureGate("TimeTracking")]
    public IActionResult GetTimedData()
    {
        System.Threading.Thread.Sleep(new Random().Next(100, 300));
        return Ok(new { Message = "This execution was timed" });
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
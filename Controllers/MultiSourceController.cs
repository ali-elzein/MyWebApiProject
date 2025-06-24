using Microsoft.AspNetCore.Mvc;

namespace MyWebApiProject.Controllers;

[ApiController]
[Route("[controller]")]

public class MultiSourceController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetFromMultipleSources(
        int id,
        [FromQuery] string? name,
        [FromBody] dynamic? payload,
        [FromHeader(Name = "Accept-Language")] string info
    )
    {
        return Ok(new { id, name, payload = payload ?? "No body provided", info = info});
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            var fileContents = await reader.ReadToEndAsync();
            return Ok($"File contents:\n{fileContents}");
        }
    }
}
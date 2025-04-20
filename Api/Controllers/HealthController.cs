using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/health")]
public class HealthController : ControllerBase
{
    [HttpGet("")]
    public IActionResult GetHealth()
    {
        return Ok("Healthy");
    }
}

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Model;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly BiBaBoulderDbContext _dbContext;

    public TestController(BiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("test")]
    public IActionResult GetTest()
    {
        var count = _dbContext.Spraywalls.Count();
        return Ok(new { SpraywallCount = count });
    }
}

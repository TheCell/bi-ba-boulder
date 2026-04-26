using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Model.Authorization;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize(Roles = AuthorizationRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class AdministrationController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new
        {
            Message = "Welcome to your new controller!",
            Path = "Controllers/AdministrationController.cs"
        });
    }
}

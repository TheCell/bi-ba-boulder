using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Spraywall.Testing;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IQueryHandler<GetTestingQuery, TestDto> _getTestingQueryHandler;

    public TestController(
        IQueryHandler<GetTestingQuery, TestDto> getTestingQueryHandler)
    {
        _getTestingQueryHandler = getTestingQueryHandler;
    }

    [HttpGet]
    public async Task<TestDto> GetTest()
    {
        return await _getTestingQueryHandler.HandleAsync(new GetTestingQuery());
    }

    //[HttpGet("{id}")]

}

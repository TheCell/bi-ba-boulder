using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thecell.Bibaboulder.BiBaBoulder.Extensions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BiBaBoulder.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBiBaBoulderDbContext _dbContext;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IBiBaBoulderDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal == null)
        {
            return null;
        }

        return await claimsPrincipal.GetCurrentUserAsync(_dbContext);
    }

    public string? GetCurrentUserOidcSubject()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }
}

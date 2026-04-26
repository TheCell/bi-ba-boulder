using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.BiBaBoulder.Extensions;

public static class ClaimsExtensions
{
    public static async Task<User?> GetCurrentUserAsync(this ClaimsPrincipal claimsPrincipal, IBiBaBoulderDbContext dbContext)
    {
        var userOidc = claimsPrincipal.FindFirst("sub")?.Value;
        if (userOidc != null)
        {
            return await dbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.OidcSubject == userOidc);
        }

        return null;
    }
}

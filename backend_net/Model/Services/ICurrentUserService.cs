using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model.Services;

public interface ICurrentUserService
{
    Task<User?> GetCurrentUserAsync();
    string? GetCurrentUserOidcSubject();
}

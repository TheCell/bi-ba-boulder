using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Sharedtests;

public class CurrentUserServiceMock : ICurrentUserService
{
    private User? _currentUser;

    public CurrentUserServiceMock WithUser(User? user)
    {
        _currentUser = user;
        return this;
    }

    public Task<User?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    public Task<User> GetCurrentUserOrThrowAsync()
    {
        if (_currentUser is null)
        {
            throw new NotFoundException("No current user configured in mock.");
        }

        return Task.FromResult(_currentUser);
    }

    public string? GetCurrentUserOidcSubject()
    {
        return _currentUser?.OidcSubject;
    }
}

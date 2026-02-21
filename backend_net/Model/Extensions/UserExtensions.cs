using System.Collections.Generic;
using System.Linq;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model.Extensions;

public static class UserExtensions
{
    public static UserRole[] GetUserRoles(this User user)
    {
        var roles = new List<UserRole>();
        if (user.Roles.Contains(AuthorizationRoles.Admin))
        {
            roles.Add(UserRole.Admin);
        }
        if (user.Roles.Contains(AuthorizationRoles.User))
        {
            roles.Add(UserRole.User);
        }
        return roles.ToArray();
    }

    public static bool IsInRole(this User user, UserRole role)
    {
        return user.GetUserRoles().Contains(role);
    }
}

using System;
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
        if (HasRole(user, AuthorizationRoles.Admin))
        {
            roles.Add(UserRole.Admin);
        }
        if (HasRole(user, AuthorizationRoles.Editor))
        {
            roles.Add(UserRole.Editor);
        }
        if (HasRole(user, AuthorizationRoles.ContentAdmin))
        {
            roles.Add(UserRole.ContentAdmin);
        }
        if (HasRole(user, AuthorizationRoles.User))
        {
            roles.Add(UserRole.User);
        }
        return roles.ToArray();
    }

    public static bool IsInRole(this User user, UserRole role)
    {
        return user.GetUserRoles().Contains(role);
    }

    private static bool HasRole(User user, string role)
    {
        return user.Roles
            .Split([',', '[', ']', '"', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}

using System.Linq;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Extensions;

public static class OutdoorAreaVisibilityExtensions
{
    public static IQueryable<OutdoorArea> WhereVisibleTo(this IQueryable<OutdoorArea> outdoorAreas, User? currentUser)
    {
        var isAdmin = currentUser?.IsInRole(UserRole.Admin) ?? false;
        var isContentAdmin = currentUser?.IsInRole(UserRole.ContentAdmin) ?? false;
        var currentUserId = currentUser?.Id;

        return outdoorAreas.Where(area => area.IsPublic || isAdmin || (isContentAdmin && area.CreatedUserId == currentUserId));
    }

    public static bool IsVisibleTo(this OutdoorArea outdoorArea, User? currentUser)
    {
        return outdoorArea.IsPublic
            || (currentUser is not null && currentUser.IsInRole(UserRole.Admin))
            || (currentUser is not null && currentUser.IsInRole(UserRole.ContentAdmin) && outdoorArea.CreatedUserId == currentUser.Id);
    }
}
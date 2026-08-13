using System;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Model.Access;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class UserSectorAccessBuilder : BuilderBase<UserSectorAccess>
{
    public UserSectorAccessBuilder(User user, Sector sector) : base()
    {
        SetUser(user);
        SetSector(sector);
        _instance.AccessSourceType = AccessSourceType.ManualGrant;
    }

    public UserSectorAccessBuilder SetUser(User value)
    {
        _instance.User = value;
        _instance.UserId = value.Id;
        return this;
    }

    public UserSectorAccessBuilder SetSector(Sector value)
    {
        _instance.Sector = value;
        _instance.SectorId = value.Id;
        return this;
    }

    public UserSectorAccessBuilder SetSectorId(Guid value)
    {
        _instance.SectorId = value;
        return this;
    }

    public UserSectorAccessBuilder SetAccessSourceType(AccessSourceType value)
    {
        _instance.AccessSourceType = value;
        return this;
    }

    public UserSectorAccessBuilder SetValidUntil(DateTime? value)
    {
        _instance.ValidUntil = value;
        return this;
    }
}

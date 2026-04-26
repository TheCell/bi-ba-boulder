using System;
using Bogus;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class UserBuilder : BuilderBase<User>
{
    public UserBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Email = bogus.Internet.Email();
        _instance.Username = bogus.Internet.UserName();
        _instance.OidcSubject = "1234567890";
        _instance.Roles = "user";
    }

    public UserBuilder SetEmail(string value)
    {
        _instance.Email = value;
        return this;
    }

    public UserBuilder SetUsername(string value)
    {
        _instance.Username = value;
        return this;
    }

    public UserBuilder SetOidcSubject(string value)
    {
        _instance.OidcSubject = value;
        return this;
    }

    public UserBuilder SetRoles(string value)
    {
        _instance.Roles = value;
        return this;
    }
}

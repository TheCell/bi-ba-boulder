using System;

namespace Thecell.Bibaboulder.Model.Dto;

public class UserDto
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required string[] Roles { get; set; }
}

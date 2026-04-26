using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model;

public class User : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(180)]
    public required string Email { get; set; }

    [MaxLength(255)]
    public required string Username { get; set; }

    /// <summary>
    /// The 'sub' claim from the external OIDC Identity Provider.
    /// Used to link the external identity to this local user record.
    /// </summary>
    [MaxLength(255)]
    public required string OidcSubject { get; set; }

    public required string Roles { get; set; } // JSON or comma-separated

    public bool IsVerified { get; set; }

    public DateTime? VerifyMailSentTime { get; set; }

    public ICollection<SpraywallProblem> SpraywallProblems { get; set; } = [];

    public ICollection<BoulderLog> BoulderLogs { get; set; } = [];

    public ICollection<Bookmark> Bookmarks { get; set; } = [];
}

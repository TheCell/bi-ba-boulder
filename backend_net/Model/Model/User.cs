using System;
using System.ComponentModel.DataAnnotations;

namespace Thecell.Bibaboulder.Model.Model;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(180)]
    public required string Email { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    [Required]
    public required string Roles { get; set; } // JSON or comma-separated

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

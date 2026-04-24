using System;
using System.ComponentModel.DataAnnotations;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model;

public class Email : EntityAuditFields
{
    public Guid Id { get; set; }

    [MaxLength(512)]
    public required string To { get; set; }

    [MaxLength(512)]
    public string? Cc { get; set; }

    [MaxLength(512)]
    public string? Bcc { get; set; }

    [MaxLength(1024)]
    public required string Subject { get; set; }

    public required string Body { get; set; }

    public DateTime SentAt { get; set; }
}

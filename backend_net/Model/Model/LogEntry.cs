using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model;

public class LogEntry : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    public required DateTime Date { get; set; }

    public required bool IsSend { get; set; }

    public required bool IsAttempt { get; set; }

    [ForeignKey(nameof(BoulderLog))]
    public Guid? BoulderLogId { get; set; }

    public BoulderLog? BoulderLog { get; set; }
}

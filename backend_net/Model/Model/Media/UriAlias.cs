using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model.Indoor;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Model.Media;

public class UriAlias : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    public required string Alias { get; set; }

    public required UriType Type { get; set; }

    public Guid? BoulderGymId { get; set; }

    [ForeignKey(nameof(BoulderGymId))]
    public BoulderGym? BoulderGym { get; set; }

    public Guid? OutdoorAreaId { get; set; }

    [ForeignKey(nameof(OutdoorAreaId))]
    public OutdoorArea? OutdoorArea { get; set; }
}

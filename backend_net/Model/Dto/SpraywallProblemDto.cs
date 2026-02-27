using System;

namespace Thecell.Bibaboulder.Model.Dto;

public class SpraywallProblemDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Image { get; set; }
    public int? FontGrade { get; set; }
    public required Guid CreatedById { get; set; }
    public required string CreatedByName { get; set; }
    public required string CreatedDate { get; set; }
    public string? Description { get; set; }
    public required SpraywallProblemMetadataDto Metadata { get; set; }
}

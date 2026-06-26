using System;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.ProblemInterfaces;

namespace Thecell.Bibaboulder.Model.Dto;

public class SpraywallProblemDto : IProblemTags
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Image { get; set; }
    public FontGrade? FontGrade { get; set; }
    public bool IsCircuit { get; set; }
    public bool NoMatch { get; set; }
    public bool FreeFeet { get; set; }
    public required Guid CreatedById { get; set; }
    public required string CreatedByName { get; set; }
    public required string CreatedDate { get; set; }
    public required long Version { get; set; }
    public string? Description { get; set; }
    public required SpraywallProblemMetadataDto Metadata { get; set; }
}

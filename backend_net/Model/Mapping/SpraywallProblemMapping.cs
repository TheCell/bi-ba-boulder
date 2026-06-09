using System;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class SpraywallProblemMapping
{
    public static SpraywallProblemDto MapToSpraywallProblemDto(
        this SpraywallProblem problem,
        string createdByName,
        string imageBase64)
    {
        return new SpraywallProblemDto
        {
            Id = problem.Id,
            Name = problem.Name,
            Image = imageBase64,
            FontGrade = problem.FontGrade,
            CreatedById = problem.CreatorId,
            CreatedByName = createdByName,
            CreatedDate = DateTime.SpecifyKind(problem.CreatedDate, DateTimeKind.Utc).ToString("o", System.Globalization.CultureInfo.InvariantCulture),
            Description = problem.Description,
            Version = problem.Version,
            Metadata = new SpraywallProblemMetadataDto
            {
                CanEdit = false,
                CanDelete = false
            }
        };
    }
}

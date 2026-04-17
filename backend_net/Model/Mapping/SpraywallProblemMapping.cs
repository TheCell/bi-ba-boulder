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
            CreatedDate = problem.CreatedDate.ToString("u"),
            Description = problem.Description,
            Metadata = new SpraywallProblemMetadataDto
            {
                CanEdit = false,
                CanDelete = false
            }
        };
    }
}

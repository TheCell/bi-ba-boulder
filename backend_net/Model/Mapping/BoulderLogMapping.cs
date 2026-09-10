using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Model.Indoor;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class BoulderLogMapping
{
    public static BoulderLogDto MapToBoulderLogDto(this BoulderLog boulderLog)
    {
        return new BoulderLogDto
        {
            Id = boulderLog.Id,
            Version = boulderLog.Version,
            IsSent = boulderLog.IsSent,
            IsProject = boulderLog.IsProject,
            Rating = boulderLog.Rating,
            FontGrade = boulderLog.FontGrade,
            UserId = boulderLog.UserId,
            SpraywallProblemId = boulderLog.SpraywallProblemId
        };
    }
}

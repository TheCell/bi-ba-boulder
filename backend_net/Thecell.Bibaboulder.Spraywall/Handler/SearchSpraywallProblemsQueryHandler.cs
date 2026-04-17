using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class SearchSpraywallProblemsQueryHandler : IQueryHandler<SearchSpraywallProblemsQuery, SpraywallProblemListDto>
{
    private const int PageSize = 30;

    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISpraywallImageService _imageService;

    public SearchSpraywallProblemsQueryHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService,
        ISpraywallImageService imageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _imageService = imageService;
    }

    public async Task<SpraywallProblemListDto> HandleAsync(SearchSpraywallProblemsQuery query)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);

        var dbQuery = _dbContext.SpraywallProblems
            .AsNoTracking()
            .Where(p => p.SpraywallId == query.SpraywallId);

        if (query.GradeMin.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.FontGrade.HasValue && p.FontGrade.Value >= query.GradeMin.Value);
        }

        if (query.GradeMax.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.FontGrade.HasValue && p.FontGrade.Value <= query.GradeMax.Value);
        }

        if (!string.IsNullOrEmpty(query.Name))
        {
            dbQuery = dbQuery.Where(p => p.Name.Contains(query.Name));
        }

        if (!string.IsNullOrEmpty(query.Creator))
        {
            dbQuery = dbQuery.Where(p => p.Creator.Username.Contains(query.Creator));
        }

        dbQuery = query.DateOrder == "asc"
            ? dbQuery.OrderBy(p => p.CreatedDate)
            : dbQuery.OrderByDescending(p => p.CreatedDate);

        var totalCount = await dbQuery.CountAsync();
        var page = Math.Max(1, query.Page);
        var offset = (page - 1) * PageSize;

        var problems = await dbQuery
            .Skip(offset)
            .Take(PageSize)
            .ToListAsync();

        var creatorIds = problems.Select(p => p.CreatorId).Distinct().ToList();
        var creators = await _dbContext.Users
            .AsNoTracking()
            .Where(u => creatorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);

        var problemDtos = new List<SpraywallProblemDto>();
        foreach (var problem in problems)
        {
            var creatorName = creators.GetValueOrDefault(problem.CreatorId, "Unknown");
            var image = await _imageService.GetImageAsBase64Async(problem.SpraywallId, problem.Id);
            if (image == null)
            {
                throw new IOException($"Failed to load image for problem {problem.Id}");
            }
            var spraywallProblemDto = problem.MapToSpraywallProblemDto(creatorName, image);
            spraywallProblemDto.Metadata.CanEdit = currentUser != null && currentUser.Id == problem.CreatorId;
            spraywallProblemDto.Metadata.CanDelete = currentUser != null && (currentUser.Id == problem.CreatorId || isAdmin);
            problemDtos.Add(spraywallProblemDto);
        }

        return new SpraywallProblemListDto
        {
            TotalCount = totalCount,
            CurrentPage = page,
            Problems = problemDtos
        };
    }
}

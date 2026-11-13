using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Model.Indoor;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Indoor.Handler;

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

        var currentUserId = currentUser?.Id;

        var dbQuery = _dbContext.SpraywallProblems
            .AsNoTracking()
            .Where(p => p.SpraywallId == query.SpraywallId)
            .Where(p => !p.IsWip || (currentUserId != null && p.CreatorId == currentUserId));

        if (query.GradeMin.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.FontGrade.HasValue && p.FontGrade.Value >= query.GradeMin.Value);
        }

        if (query.GradeMax.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.FontGrade.HasValue && p.FontGrade.Value <= query.GradeMax.Value);
        }

        if (!string.IsNullOrEmpty(query.Creator))
        {
            dbQuery = dbQuery.Where(p => p.Creator.Username.Contains(query.Creator));
        }

        dbQuery = query.DateOrder == "asc"
            ? dbQuery.OrderBy(p => !p.IsWip).ThenBy(p => p.CreatedDate)
            : dbQuery.OrderBy(p => !p.IsWip).ThenByDescending(p => p.CreatedDate);

        var page = Math.Max(1, query.Page);
        var offset = (page - 1) * PageSize;

        int totalCount;
        List<SpraywallProblem> problems;
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var matches = (await dbQuery
                    .Select(problem => new { problem.Id, problem.Name })
                    .ToListAsync())
                .Select(problem => new
                {
                    problem.Id,
                    Match = SpraywallNameMatcher.FindBestMatch(problem.Name, query.Name)
                })
                .Where(result => result.Match.HasValue)
                .OrderBy(result => result.Match!.Value.Distance)
                .ToList();

            totalCount = matches.Count;
            var problemIds = matches
                .Skip(offset)
                .Take(PageSize)
                .Select(result => result.Id)
                .ToList();
            var matchingProblems = await dbQuery
                .Where(problem => problemIds.Contains(problem.Id))
                .ToListAsync();
            var problemsById = matchingProblems.ToDictionary(problem => problem.Id);
            problems = problemIds.Select(problemId => problemsById[problemId]).ToList();
        }
        else
        {
            totalCount = await dbQuery.CountAsync();
            problems = await dbQuery
                .Skip(offset)
                .Take(PageSize)
                .ToListAsync();
        }

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

using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class GetSpraywallProblemTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly MockSpraywallImageService _imageService;

    public GetSpraywallProblemTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
        _imageService = new MockSpraywallImageService();
    }

    [Fact]
    public async Task GetSpraywallProblem_Ok()
    {
        var user = new UserBuilder().SetUsername("creator").Build();
        await _dbContext.InsertEntityAsync(user);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(user, spraywall)
            .SetName("Test Problem")
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        _currentUserServiceMock.WithUser(user);
        var handler = new GetSpraywallProblemQueryHandler(_dbContext, _currentUserServiceMock, _imageService);
        var result = await handler.HandleAsync(new GetSpraywallProblemQuery
        {
            Id = problem.Id
        });

        Assert.Equal(problem.Id, result.Id);
        Assert.Equal("Test Problem", result.Name);
        Assert.Equal("creator", result.CreatedByName);
        Assert.True(result.Metadata.CanEdit);
        Assert.True(result.Metadata.CanDelete);
    }

    [Fact]
    public async Task GetProblem_NotFound_ThrowsNotFoundException()
    {
        var handler = new GetSpraywallProblemQueryHandler(_dbContext, _currentUserServiceMock, _imageService);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new GetSpraywallProblemQuery
            {
                Id = Guid.CreateVersion7()
            }));
    }
}

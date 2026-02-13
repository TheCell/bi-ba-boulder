using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class CreateSectorTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public CreateSectorTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task CreateSector_Ok()
    {
        var command = CreateCommand();

        var handler = CreateHandler();
        await handler.HandleAsync(command);

        var sector = await _dbContext.Sectors.SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        SectorAssertion.Assert(command, sector);
    }

    [Fact]
    public async Task CreateSector_Error()
    {
        var command = CreateCommand();
        command.Name = null!;

        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await handler.HandleAsync(command));

        Assert.NotNull(exception.Message);
    }

    private CreateSectorCommand CreateCommand()
    {
        return new CreateSectorCommand
        {
            Name = "Test",
            Description = "Test"
        };
    }

    private CreateSectorCommandHandler CreateHandler()
    {
        return new CreateSectorCommandHandler(_dbContext);
    }
}

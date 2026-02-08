using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Outdoor.Handler;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class CreateSectorTest
{
    private readonly ICommandHandler<CreateSectorCommand> _createSectorCommandHandler;

    public CreateSectorTest(
        ICommandHandler<CreateSectorCommand> createSectorCommandHandler)
    {
        _createSectorCommandHandler = createSectorCommandHandler;
    }

    [Fact]
    public async Task GetSector_Ok()
    {
        // arrange
        PrepareTestdata();
        var command = GetCommand();
        // act
        await _createSectorCommandHandler.HandleAsync(command);
        // assert
        Assert.True(false);
    }

    private CreateSectorCommand GetCommand()
    {
        return new CreateSectorCommand
        {
            Name = "Test",
            Description = "Test"
        };
    }

    private void PrepareTestdata()
    {
    }
}

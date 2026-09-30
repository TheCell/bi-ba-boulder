using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class UpdateBlocTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly Faker _bogus;

    public UpdateBlocTest()
    {
        _dbContext = new DbContextMock().Build();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task UpdateBloc_NotFound_NotFoundException()
    {
        var command = new UpdateBlocCommand
        {
            Id = Guid.CreateVersion7(),
            SectorId = Guid.CreateVersion7(),
            Name = _bogus.Lorem.Slug(),
            Version = 1
        };
        var handler = new UpdateBlocCommandHandler(_dbContext);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
        Assert.Equal($"Bloc not found. (Id: {command.Id})", ex.Message);
    }

    [Fact]
    public async Task UpdateBloc_UnknownSector_NotFoundException()
    {
        var sector = new SectorBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new UpdateBlocCommand
        {
            Id = bloc.Id,
            SectorId = Guid.CreateVersion7(),
            Name = _bogus.Lorem.Slug(),
            Version = bloc.Version
        };
        var handler = new UpdateBlocCommandHandler(_dbContext);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
        Assert.Equal($"Sector not found. (Id: {command.SectorId})", ex.Message);
    }

    [Fact]
    public async Task UpdateBloc_Ok()
    {
        var sector = new SectorBuilder().Build();
        var otherSector = new SectorBuilder().Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([sector, otherSector]);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new UpdateBlocCommand
        {
            Id = bloc.Id,
            Version = bloc.Version,
            SectorId = otherSector.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            Coordinates = "46.9914628, 7.5589870",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath()
        };

        var handler = new UpdateBlocCommandHandler(_dbContext);
        await handler.HandleAsync(command);

        var updated = await _dbContext.Blocs
            .AsNoTracking()
            .SingleAsync(item => item.Id == bloc.Id, TestContext.Current.CancellationToken);
        BlocAssertion.Assert(command, updated);
    }
}

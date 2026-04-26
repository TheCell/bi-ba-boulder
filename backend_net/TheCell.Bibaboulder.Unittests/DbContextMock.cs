using System;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model;

namespace TheCell.Bibaboulder.Unittests;

public class DbContextMock
{
    public IBiBaBoulderDbContext Build()
    {
        var options = new DbContextOptionsBuilder<BiBaBoulderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        return new BiBaBoulderDbContext(options);
        //var dbContextMock = new Mock<IBiBaBoulderDbContext>();
        ////dbContextMock.SetupProperty((mock) => mock.Sectors, )
        //return dbContextMock.Object;
    }
}

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Thecell.Bibaboulder.Model;
using System.Text.Json;
using Xunit;

namespace TheCell.Bibaboulder.Sharedtests;

public class BaseTest : IDisposable, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;

    private readonly HttpClient _client;

    protected HttpClient Client()
    {
        return _client;
    }

    protected IBiBaBoulderDbContext BiBaBoulderDbContext { get; }

    protected BaseTest(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var serviceScope = _factory.Services.CreateScope();
        BiBaBoulderDbContext = serviceScope.ServiceProvider.GetRequiredService<BiBaBoulderDbContext>();
    }

    public async ValueTask InitializeAsync()
    {
        await ValueTask.CompletedTask;
    }

    public virtual void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        BiBaBoulderDbContext.Dispose();
        GC.SuppressFinalize(this);
    }


    //public BiBaBoulderDbContext CreateDbContext()
    //{
    //    var scope = Services.CreateScope();
    //    var context = scope.ServiceProvider.GetRequiredService<BiBaBoulderDbContext>();
    //    context.Database.EnsureCreated();
    //    return context;
    //}

    //protected override void Dispose(bool disposing)
    //{
    //    base.Dispose(disposing);
    //    if (disposing)
    //    {
    //        _connection.Dispose();
    //    }
    //}
    protected StringContent GetJsonHttpBody(object data)
    {
        return new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
    }
}

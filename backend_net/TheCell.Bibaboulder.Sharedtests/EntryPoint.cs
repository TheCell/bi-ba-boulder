using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace TheCell.Bibaboulder.Sharedtests;

public sealed class EntryPoint
{
    public static async Task Main(string[] args)
    {
        var webApplicationOptions = new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.FullName
        };
        var builder = WebApplication.CreateBuilder(webApplicationOptions);

        builder.WebHost
            .ConfigureKestrel(serverOptions => { serverOptions.AddServerHeader = false; }); // Disable the "Server" header for security reasons

        var app = builder.Build();
        await app.RunAsync();
    }
}


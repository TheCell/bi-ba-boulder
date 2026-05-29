
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Thecell.Bibaboulder.Common.Appsettings;

namespace Thecell.Bibaboulder.BiBaBoulder.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBiBaBoulderServices(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.Configure<EmailSettings>(webApplicationBuilder.Configuration.GetSection(EmailSettings.SectionName));
        return webApplicationBuilder.Services;
    }
}

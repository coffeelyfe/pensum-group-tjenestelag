using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PensumGroup.Modules.Equities.Features.InvestmentDetails;
using PensumGroup.Modules.Equities.Integrations.Morningstar;

namespace PensumGroup.Modules.Equities;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEquitiesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MorningstarOptions>(configuration.GetSection(MorningstarOptions.SectionName));

        services.AddHttpClient<IMorningstarClient, MorningstarClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddInvestmentDetailsFeatures();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using PensumGroup.Modules.Equities.Features.InvestmentDetails.GetEquityInvestmentDetails;

namespace PensumGroup.Modules.Equities.Features.InvestmentDetails;

public static class FeatureRegistration
{
    public static IServiceCollection AddInvestmentDetailsFeatures(this IServiceCollection services)
    {
        services.AddScoped<IGetEquityInvestmentDetailsFeature, GetEquityInvestmentDetailsFeature>();

        return services;
    }
}

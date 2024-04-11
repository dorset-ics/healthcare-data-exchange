using Core.Common;
using Core.Ingestion;
using Core.Ndop;
using Core.Ods;
using Core.Pds;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
        => services
            .AddPds()
            .AddOds()
            .AddNdop()
            .AddIngestion()
            .AddCommon();
}
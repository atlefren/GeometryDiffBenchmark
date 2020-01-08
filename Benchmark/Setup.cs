using Benchmark;
using Benchmark.services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: FunctionsStartup(typeof(Setup))]

namespace Benchmark
{
    public class Setup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddAppSettingsToConfiguration();
            builder.Services.TryAddTransient<IGeometryFetcher, GeometryFetcher>();
        }
    }
}
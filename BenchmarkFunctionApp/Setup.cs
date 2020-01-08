using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BenchmarkFunctionApp.Setup))]

namespace BenchmarkFunctionApp
{
    public class Setup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IGeometryReader>(new GeometryReader("osm_test.point_versions",
                "osm_test.nodes"));
            //builder.Services.AddScoped<IGeometryReader>((ctx) => new GeometryReader("osm_test.point_versions", "osm_test.nodes"));
        }
    }
}
using System.Diagnostics;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Json;
using ILogger = Serilog.ILogger;
namespace BobsBBQApi.Services;

public class MonitorService
{
    public static ILogger Log => Serilog.Log.Logger;
    public static readonly string ServiceName = Assembly.GetCallingAssembly().GetName().Name ?? "Unknown";
    public static TracerProvider TracerProvider;
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);


    static MonitorService()
    {
        
        //OpenTelemetry
        TracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddConsoleExporter()
            .AddZipkinExporter(opt => opt.Endpoint = new Uri("http://zipkin:9411/api/v2/spans"))
            .AddSource(ActivitySource.Name)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
            .Build();

        // Serilog
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(new JsonFormatter())
            .WriteTo.Seq("http://seq:5341")
            .Enrich.WithSpan()
            .CreateLogger();
    }
}
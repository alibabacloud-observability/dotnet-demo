using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

namespace Demo
{
    internal static class OpentelemetryExporterDemo
    {
        internal static void Run()
        {
            Console.WriteLine("otlp running");
            var serviceName = "otlp-test";
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(serviceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri("http://localhost/api/otlp/traces");
                    opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .Build();
            for(int i = 0; i<10; i++)
            {
                var MyActivitySource = new ActivitySource(serviceName);
                using var activity = MyActivitySource.StartActivity("SayHello");
                activity?.SetTag("bar", "Hello World");
            }
        }
    }
}
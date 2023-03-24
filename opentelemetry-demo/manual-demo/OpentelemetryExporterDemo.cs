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
            // OpenTelemetry上报应用名
            var serviceName = "otlp-test";
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(serviceName)
                .SetResourceBuilder(
                ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddOtlpExporter(opt =>
                                 {
                                     // 根据前提条件中获取的接入点信息进行修改
                                     opt.Endpoint = new Uri("<endpoint>");
                                     // 使用HTTP协议上报数据
                                     opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                                 })
                .AddConsoleExporter() // 可选，在控制台导出数据
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
using System.Diagnostics;
using System.Net.Http;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace Demo
{
    public class Otlp
    {
        public static void Main(string[] args)
        {
            OpentelemetryExporterDemo.Run();
        }
    }    
}
using Jaeger;
using Jaeger.Samplers;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using System;

namespace ManualDemo
{
    class Program
    {
        public static ITracer InitTracer(string serviceName, ILoggerFactory loggerFactory)
        {
            Configuration.SamplerConfiguration samplerConfiguration = new Configuration.SamplerConfiguration(loggerFactory)
                .WithType(ConstSampler.Type)
                .WithParam(1);
            Configuration.SenderConfiguration senderConfiguration = new Configuration.SenderConfiguration(loggerFactory)
                   //(访问https://tracing-analysis.console.aliyun.com 获取jaeger endpoint)
                  .WithEndpoint("http://tracing-analysis-dc-sz.aliyuncs.com/adapt_your_token/api/traces");

            Configuration.ReporterConfiguration reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
                .WithSender(senderConfiguration);

            return (Tracer)new Configuration(serviceName, loggerFactory)
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration)
                .GetTracer();
        }

        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            GlobalTracer.Register(InitTracer("dotnetManualDemo", loggerFactory ));
            Console.WriteLine("start tracing...");
            testTracing();
            Console.WriteLine("end tracing...");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void testTracing()
        {
            ITracer tracer = GlobalTracer.Instance;
            ISpan span = tracer.BuildSpan("parentSpan").WithTag("mytag","parentSapn").Start();
            tracer.ScopeManager.Activate(span, false);
            // ...do business
            testCall();
            span.Finish();
        }

        private static void testCall()
        {
            ITracer tracer = GlobalTracer.Instance;
            ISpan parentSpan = tracer.ActiveSpan;
            ISpan childSpan =tracer.BuildSpan("childSpan").AsChildOf(parentSpan).WithTag("mytag", "spanSecond").Start();
            tracer.ScopeManager.Activate(childSpan, false);
            // ... do business
            childSpan.Finish();
            tracer.ActiveSpan.SetTag("methodName", "testCall");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Jaeger;
using Jaeger.Samplers;
using Microsoft.Extensions.Logging;

namespace GreeterShared
{
    public static class TracingHelper
    {
        public static Tracer InitTracer(string serviceName, ILoggerFactory loggerFactory)
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
    }
}

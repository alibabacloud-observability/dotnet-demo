using System;
using GreeterShared;
using Grpc.Core;
using Helloworld;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Contrib.Grpc.Interceptors;
using Grpc.Core.Interceptors;

namespace GreeterClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
            ITracer tracer = TracingHelper.InitTracer("dotnetGrpcClient", loggerFactory);
            ClientTracingInterceptor tracingInterceptor = new ClientTracingInterceptor(tracer);
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new Greeter.GreeterClient(channel.Intercept(tracingInterceptor));
            String user = "you";

            var reply = client.SayHello(new HelloRequest { Name = user });
            Console.WriteLine("Greeting: " + reply.Message);

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

using System;
using System.Threading.Tasks;
using GreeterShared;
using Grpc.Core;
using Helloworld;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Contrib.Grpc.Interceptors;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
            ITracer tracer = TracingHelper.InitTracer("dotnetGrpcServer", loggerFactory);
            ServerTracingInterceptor tracingInterceptor = new ServerTracingInterceptor(tracer);
            Server server = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()).Intercept(tracingInterceptor) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}

# 通过OpenTelemetry上报.NET应用数据

## 项目目录说明
1. manual-demo: 使用OpenTelemetry .NET SDK手动埋点
2. auto-demo: 自动埋点

## 方法一：使用OpenTelemetry .NET SDK手动埋点

1. 进入示例代码仓库的路径dotnet-demo/opentelemetry-demo/manual-demo，然后安装手动埋点所需的OpenTelemetry相关依赖。
```bash
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Exporter.Console # 可选，在控制台导出数据
dotnet add package OpenTelemetry.Extensions.Hosting 
```

2. 在OpentelemetryExporterDemo.cs文件中创建OpenTelemetry TracerProvider，并添加基于HTTP协议的OtlpExporter，修改上报应用名和上报数据的Endpoint。
```csharp
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
```

3. 修改Program.cs，在Main方法中调用OpentelemetryExporterDemo
```csharp
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
```

4. 在当前路径下运行以下命令。
```bash
dotnet run
```

5. 启动本地示例程序，在链路追踪Tracing Analysis控制台按照自定义的应用名称搜索应用，查看上报的数据。


## 方法二：自动埋点
OpenTelemetry支持自动上传数十种.NET框架Trace的数据，详细的.NET框架列表请参见[Supported Libraries](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/blob/main/docs/config.md#traces-instrumentations)。需要注意，目前支持自动上传Trace数据的OpenTelemetry .NET包为预览版本而非稳定版本。

1. 进入示例代码仓库的路径dotnet-demo/opentelemetry-demo，创建ASP.NET Core应用（Web应用），请替换<your-project-name>为您自己的项目名。
```csharp
mkdir <your-project-name>
cd <your-project-name>
dotnet new mvc
```

2. 下载观测.NET应用必备的OpenTelemetry依赖
```bash
dotnet add package OpenTelemetry.Exporter.Console # 在控制台导出采集的数据
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol # 以OTLP协议导出
```

3. 下载自动埋点依赖
- 下载自动为ASP.NET Core框架埋点的依赖，当应用收到HTTP请求时，会自动生成Span并上报
- 如需为其他框架自动埋点，请参考 [Supported Libraries](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/blob/main/docs/config.md#traces-instrumentations) 下载对应依赖
```bash
dotnet add package OpenTelemetry.Instrumentation.AspNetCore --prerelease
```

4. 修改 `<your-project-name>/Program.cs` 中的代码：
- 在源文件开头导入所需包
```bash
using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
```

- 文件源文件末尾添加DiagnosticsConfig类，设置服务名称。请将`<your-service-name>`替换为您的服务名，`<your-host-name>`替换为您的主机名。
```csharp
public static class DiagnosticsConfig
{
    public const string ServiceName = "<your-service-name>"; // your service name
    public const string HostName = "<your-host-name>"; // your host name
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
}
```

- 添加OpenTelemetry初始化代码（使用HTTP协议上报）
   - 请将以下代码中的`<http_token>`替换成前提条件中获取的Token
```csharp
// ...
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource(DiagnosticsConfig.ActivitySource.Name)
            .SetResourceBuilder(OpenTelemetry.Resources.ResourceBuilder.CreateDefault()
                .AddAttributes(new Dictionary<string, object> {
                    {"service.name", DiagnosticsConfig.ServiceName},
                    {"host.name",DiagnosticsConfig.HostName}
                }))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter() // 在控制台导出Trace数据，可选
            .AddOtlpExporter(opt =>
            {
                // 使用HTTP协议上报
                opt.Endpoint = new Uri("<http_endpoint>");
                opt.Protocol = OtlpExportProtocol.HttpProtobuf;
            })
     );
// ...
```

- 如果您使用gRPC协议上报，请使用以下OpenTelemetry初始化代码
   - 请将以下代码中`<grpc_token>`替换成前提条件中获取的Token，将`<endpoint>`替换成对应地域的Endpoint
```csharp
// ...
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource(DiagnosticsConfig.ActivitySource.Name)
            .SetResourceBuilder(OpenTelemetry.Resources.ResourceBuilder.CreateDefault()
                .AddAttributes(new Dictionary<string, object> {
                    {"service.name", DiagnosticsConfig.ServiceName},
                    {"host.name",DiagnosticsConfig.HostName}
                }))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter() // 在控制台导出Trace数据，可选
            .AddOtlpExporter(opt =>
            {
                // 使用gRPC协议上报
                opt.Endpoint = new Uri("<grpc_endpoint>");
                opt.Headers = "Authentication=<token>";
                opt.Protocol = OtlpExportProtocol.Grpc;
            })
     );
// ...
```

- Program.cs 完整代码如下
```csharp
// 引入所需包
using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

// OpenTelemetry 初始化
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource(DiagnosticsConfig.ActivitySource.Name)
            .SetResourceBuilder(OpenTelemetry.Resources.ResourceBuilder.CreateDefault()
                .AddAttributes(new Dictionary<string, object> {
                    {"service.name", DiagnosticsConfig.ServiceName},
                    {"host.name",DiagnosticsConfig.HostName}
                }))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter() // 在控制台输出Trace数据，可选
            .AddOtlpExporter(opt =>
            {
                // 使用HTTP协议上报
                opt.Endpoint = new Uri("<http_endpoint>");
                opt.Protocol = OtlpExportProtocol.HttpProtobuf;

                // 使用gRPC协议上报
                // opt.Endpoint = new Uri("<grpc_endpoint>");
                // opt.Headers = "Authentication=<token>";
                // opt.Protocol = OtlpExportProtocol.Grpc;
            })
     );


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// 创建DiagnosticsConfig类
public static class DiagnosticsConfig
{
    public const string ServiceName = "<your-service-name>"; // your service name
    public const string HostName = "<your-host-name>"; // your host name
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
}

```

5. 运行并查看
- 控制台输入 `dotnet run` 以运行Web应用。
```bash
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5107
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /path/to/<your-project-name>
```

- 请从控制台中获取URL，例如上面输出样例中的 [http://localhost:5107](http://localhost:5107)，并在浏览器中访问（不同应用的端口号不同）。


6. 在链路追踪Tracing Analysis控制台按照自定义的应用名称搜索应用，查看上报的数据。

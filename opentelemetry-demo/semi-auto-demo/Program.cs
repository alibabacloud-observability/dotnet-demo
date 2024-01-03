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

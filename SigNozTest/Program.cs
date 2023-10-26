
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SigNozTest;



AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: builder.Configuration.GetValue("ServiceName", defaultValue: "otel-test")!,
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
    serviceInstanceId: Environment.MachineName);


builder.Services.AddSingleton<Instrumentation>();

builder.Services.AddLogging().AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(b =>
    {
        b.AddSource(Instrumentation.ActivitySourceName)
            .SetSampler(new AlwaysOnSampler())
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();

        builder.Services.Configure<AspNetCoreInstrumentationOptions>(
            builder.Configuration.GetSection("AspNetCoreInstrumentation"));


        b.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint =
                new Uri(builder.Configuration.GetValue("Otlp:Endpoint",
                    defaultValue: "https://ingest.in.signoz.cloud:443/"));

            otlpOptions.Protocol = OtlpExportProtocol.Grpc;
            otlpOptions.Headers = $"signoz-access-token=789008a8-0d53-4038-ac1b-e50843f7ad9f";


        });

        b.AddConsoleExporter();

    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

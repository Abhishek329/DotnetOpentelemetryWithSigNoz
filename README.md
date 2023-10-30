# DotnetOpentelemetryWithSigNoz

This example uses the new WebApplication host that ships with .NET 6 and shows how to setup and export Activity traces to SigNoz cloud

# Prerequisites

Create an account on SigNoz Cloud - https://signoz.io

* [.Net SDK 6+](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)
* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)

The following example uses a basic Minimal API with ASP.NET Core web application.

## Run instructions for sending data to SigNoz

- Create a new ASP.NET Core web API 

```bash
dotnet new webapi -n sample-app
```

- Install dependencies
   
  Inside your project directory 

  Run
  ```bash
  dotnet add package OpenTelemetry 
  dotnet add package OpenTelemetry.API 
  dotnet add package OpenTelemetry.AutoInstrumentation 
  dotnet add package OpenTelemetry.Exporter.Console 
  dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol 
  dotnet add package OpenTelemetry.Extensions.Hosting
  dotnet add package OpenTelemetry.Instrumentation.Runtime 
  ```

### Replace the contents
  
 - Replace the contents in the file Program.cs with the following code
  ```
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

      var builder = WebApplication.CreateBuilder(args);
      
      // Add services to the container.
      
      builder.Services.AddControllers();
      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();
      
      Action<ResourceBuilder> configureResource = r => r.AddService(
          serviceName: builder.Configuration.GetValue("ServiceName", defaultValue: "sample-net-app")!,
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
                   string headerKey = "signoz-access-token";
                   string headerValue = builder.Configuration.GetValue<string>("Otlp:headers");

                   string formattedHeader = $"{headerKey}={headerValue}";
                   otlpOptions.Headers = formattedHeader;
      
              });

              //export telemetry data to console of the application
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

  ```
 
  - Create new file called Instrumentation.cs and copy the contents

   ```
      public class Instrumentation : IDisposable
       {
           internal const string ActivitySourceName = "Examples.AspNetCore";
           internal const string MeterName = "Examples.AspNetCore";
           private readonly Meter meter;
   
           public Instrumentation()
           {
               string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
               ActivitySource = new ActivitySource(ActivitySourceName, version);
               meter = new Meter(MeterName, version);
               FreezingDaysCounter = meter.CreateCounter<long>("weather.days.freezing", "The number of days where the temperature is below freezing");
           }
   
           public ActivitySource ActivitySource { get; }
   
           public Counter<long> FreezingDaysCounter { get; }
   
   
   
           public void Dispose()
           {
               ActivitySource.Dispose();
               meter.Dispose();
           }
       }
   ```
 
 - Copy the contents int your WeatherForecastController.cs
   
 ```
 private static readonly string[] Summaries = new[]
      {
          "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
      };

      private static readonly HttpClient HttpClient = new();

      private readonly ILogger<WeatherForecastController> logger;
      private readonly ActivitySource activitySource;
      private readonly Counter<long> freezingDaysCounter;

      public WeatherForecastController(ILogger<WeatherForecastController> logger, Instrumentation instrumentation)
      {
          this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

          ArgumentNullException.ThrowIfNull(instrumentation);
          this.activitySource = instrumentation.ActivitySource;
          this.freezingDaysCounter = instrumentation.FreezingDaysCounter;
      }

      [HttpGet]
      public IEnumerable<WeatherForecast> Get()
      {
          using var scope = this.logger.BeginScope("{Id}", Guid.NewGuid().ToString("N"));

          // Making an http call here to serve as an example of
          // how dependency calls will be captured and treated
          // automatically as child of incoming request.
          var res = HttpClient.GetStringAsync("http://google.com").Result;

          // Optional: Manually create an activity. This will become a child of
          // the activity created from the instrumentation library for AspNetCore.
          // Manually created activities are useful when there is a desire to track
          // a specific subset of the request. In this example one could imagine
          // that calculating the forecast is an expensive operation and therefore
          // something to be distinguished from the overall request.
          // Note: Tags can be added to the current activity without the need for
          // a manual activity using Acitivty.Current?.SetTag()
          using var activity = this.activitySource.StartActivity("calculate forecast");

          var rng = new Random();
          var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
              {
                  Date = DateTime.Now.AddDays(index),
                  TemperatureC = rng.Next(-20, 55),
                  Summary = Summaries[rng.Next(Summaries.Length)],
              })
              .ToArray();

          // Optional: Count the freezing days
          this.freezingDaysCounter.Add(forecast.Count(f => f.TemperatureC < 0));

          this.logger.LogInformation(
              "WeatherForecasts generated {count}: {forecasts}",
              forecast.Length,
              forecast);

          return forecast;
      }
  ```
  
 - Replace the contents of WeatherForecast.cs with the following

 ```
 public class WeatherForecast
  {
      public DateTime Date { get; set; }

      public int TemperatureC { get; set; }

      public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

      public string? Summary { get; set; }
  }  
 ```

- Specify the OTLP endpoint to SigNoz Cloud endpoint inside your appSettings.json
  
 ```json
  {
   "Otlp": {
       "endpoint": "[ingest address]",
       "tls": {
         "insecure": false
       },
       "headers": "[ingest key]"  
     }
   }
  ```

 - Run the application

  ```bash
   dotnet build
   dotnet run
  ```
- Generate some traffic on the application

  Web server will be running in the port 5000 by default. Browse `http://localhost:5000/Weatherforecast` to send requests to this local server and check the metrics and trace data at `http://ingest.in.signoz.cloud:443/`
  
### Troubleshooting

Try refreshing with the latest time intervals on the sigNoz cloud dashboard if you dont see the service name.

If you face any problem in instrumenting with OpenTelemetry, refer to docs at 

https://signoz.io/docs/instrumentation





  

  




# DotnetOpentelemetryWithSigNoz

This example uses the new WebApplication host that ships with .NET 6 and shows how to setup and export Activity traces to SigNoz cloud

# Prerequisites

* [.Net SDK 6+](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)
* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)

The following example uses a basic Minimal API with ASP.NET Core web application.

ResourceBuilder is associated with OpenTelemetry to associate the service name, version and the machine on which this program is running.
The sample rate is set to emit all the traces using `AlwaysOnSampler`.

## Step 1 : Create a new ASP.NET Core web API ##
In your terminal(Powershell) go to your directory and run 
`dotnet new webapi -n sample-app`

## Step 2: Install Opentelemtery Packages ##
Navigate to your sample-app directory and install the Opentelemetry NuGet Packages.

*It can be done through Visual studio 2022 sample-app Solution.

   Go to Tools -> NuGet Package Manager -> Manage NuGet Package for Solution
   
   * Browse [OpenTelemetry](https://www.nuget.org/profiles/OpenTelemetry)
     
      Install
     
           1.OpenTelemetry
     
           2.OpenTelemetry.API
     
           3.OpenTelemetry.AutoInstrumentation
     
           4.OpenTelemetry.Exporter.Console
     
           5.OpenTelemetry.Exporter.OpenTelemetryProtocol
     
           6.OpenTelemetry.Extensions.Hosting
     
           7.OpenTelemetry.Instrumentation.Runtime
     
OR

* Install it from your Powershell prompt
  
  Go to your project directory
  
  Run
  
  *`dotnet add package OpenTelemetry --version <suitable/latest version>`
  
  *`dotnet add package OpenTelemetry.API --version <suitable/latest version>`
  
  *`dotnet add package OpenTelemetry.AutoInstrumentation --version <suitable/latest version>`
  
  *`dotnet add package OpenTelemetry.Exporter.Console --version <suitable/latest version>`
  
  *`dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol --version <suitable/latest version>`
  
  *`dotnet add package OpenTelemetry.Extensions.Hosting --version <suitable/latest version>`
  
  *`dotnet add package OpenTelemetry.Instrumentation.Runtime --version <suitable/latest version>`

## Step 3 : Copy the contents
* Copy the contents from the file Program.cs
* Create new file called Instrumentation.cs and copy the contents.
* Copy the contents of WeatherForecastController.cs.
* Copy the contents of WeatherForecast.cs.
* Inside the appsettings.json, replace the Otlp Endpoint with your cloud address and Headers with your Ingestion Key.
   

   `"Otlp": {
    "endpoint": "[ingest address]",
    "tls": {
      "insecure": false
    },
    "headers": {
      "signoz-access-token": "[ingest key]"
    }    
  }` 

## Step 4 : Run the application 
`dotnet build`

`dotnet run`

## Step 5 : Run HttpGet request in Swagger
* .Net 6+ webapi automatically installs Swagger.
    * Run the HttpGet request from the Swagger UI
      
  If there's no swagger installed
  
   * Access the following URL `https://localhost:7156/WeatherForecast`
     to generate a request trace.

  Or
  
  * Run the following command in another terminal
    `curl -X 'GET' \
        'https://localhost:7156/WeatherForecast' \
            -H 'accept: text/plain'`


## Step 6: Check the Signoz Cloud
* Services section should display your newly created service
![image](https://github.com/Abhishek329/DotnetOpentelemetryWithSigNoz/assets/29237536/e0e0491c-d78a-4b9a-9f61-26be730cced0)

* On navigating to the service, details of the API request can be found
  ![image](https://github.com/Abhishek329/DotnetOpentelemetryWithSigNoz/assets/29237536/0799d3a7-6caf-42e8-9804-c7e9cfd715e8)


  

  




using System;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using System.Diagnostics; // ActivitySource är i detta namespace

namespace ConsoleSearch
{
    class Program
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource("ConsoleSearch");

        static void Main(string[] args)
        {
            // Set up OpenTelemetry
            using var tracerProvider = ConfigureOpenTelemetry();

            // Start tracing the app run
            using (var activity = ActivitySource.StartActivity("RunningApp"))
            {
                new App().Run();
            }

            // Dispose the tracer provider when the app ends
            tracerProvider?.Dispose();
        }

        static TracerProvider ConfigureOpenTelemetry()
        {
            return Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ConsoleSearch"))
                .AddSource("ConsoleSearch") // Add a source for this service
                .AddHttpClientInstrumentation() // Trace outgoing HTTP requests
                .AddZipkinExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                })
                .Build();
        }
    }
}

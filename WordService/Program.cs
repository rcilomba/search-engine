using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Exporter;

// Konfigurera Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console() // Logga till konsolen
    .WriteTo.Seq("http://localhost:5341") // Använd min Seq-instans
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Lägg till Serilog till hosten
    builder.Host.UseSerilog();

    // Lägg till OpenTelemetry
    builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WordService"))
            .AddAspNetCoreInstrumentation() // Instrument ASP.NET Core requests
            .AddHttpClientInstrumentation() // Trace outgoing HTTP requests
            .AddZipkinExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            });
    });

    // Lägg till tjänster till DI-kontainern
    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Konfigurera HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Kör applikationen
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    // Stäng Serilog när applikationen stängs
    Log.CloseAndFlush();
}

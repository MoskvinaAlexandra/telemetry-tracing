using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public static OpenTelemetryBuilder AddOtelTracing(this OpenTelemetryBuilder openTelemetryBuilder) => openTelemetryBuilder
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter(otlp =>
                {
                    otlp.ExportProcessorType = ExportProcessorType.Simple;
                })
                .ConfigureResource(rb =>
                {
                    var name = typeof(Program).Assembly.GetName();
                    var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
                    if (string.IsNullOrWhiteSpace(serviceName))
                    {
                        throw new InvalidOperationException("OTEL_SERVICE_NAME environment variable is not set.");
                    }
                    rb.AddService(
                        serviceName: serviceName,
                        serviceVersion: name.Version!.ToString(),
                        autoGenerateServiceInstanceId: true);
                    rb.AddEnvironmentVariableDetector();
                })
                .AddSource("tracing_test_service")
                .AddHttpClientInstrumentation();
        });
}
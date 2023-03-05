using System;
using System.Diagnostics;
using MassTransit.Metadata;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RegistrationRoutingSlipSample.Api
{
    public static class TelemetryConfigurationExtensions
    {
        //jaeger
        //docker run -d --name jaeger -e COLLECTOR_ZIPKIN_HOST_PORT=:9411 -e COLLECTOR_OTLP_ENABLED=true -p 6831:6831/udp -p 6832:6832/udp -p 5778:5778 -p 16686:16686 -p 4317:4317 -p 4318:4318 -p 14250:14250 -p 14268:14268 -p 14269:14269 -p 9411:9411 jaegertracing/all-in-one:latest
        /// <summary>
        /// Configure Open Telemetry on the service
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceName">name for this service</param>
        public static void AddOpenTelemetry(this IServiceCollection services, string serviceName)
        {
            services.AddOpenTelemetry()

                .WithMetrics(builder =>
                {
                    builder.AddConsoleExporter()
                        .AddMeter("MassTransit")
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(serviceName)
                            .AddTelemetrySdk()
                            .AddEnvironmentVariableDetector())
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(HostMetadataCache.IsRunningInContainer ? "http://grafana-agent:14317" : "http://localhost:14317");
                            o.Protocol = OtlpExportProtocol.Grpc;
                            o.ExportProcessorType = ExportProcessorType.Batch;
                            o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                            {
                                MaxQueueSize = 2048,
                                ScheduledDelayMilliseconds = 5000,
                                ExporterTimeoutMilliseconds = 30000,
                                MaxExportBatchSize = 512,
                            };
                        });
                })
                .WithTracing(builder =>
                {
                    builder
                        .AddSource("MassTransit")
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(serviceName)
                            .AddTelemetrySdk()
                            .AddEnvironmentVariableDetector())
                        .AddAspNetCoreInstrumentation()
                        .AddSqlClientInstrumentation(o =>
                        {
                            o.EnableConnectionLevelAttributes = true;
                            o.RecordException = true;
                            o.SetDbStatementForText = true;
                            o.SetDbStatementForStoredProcedure = true;
                        })
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(HostMetadataCache.IsRunningInContainer ? "http://tempo:4317" : "http://localhost:4317");
                            o.Protocol = OtlpExportProtocol.Grpc;
                            o.ExportProcessorType = ExportProcessorType.Batch;
                            o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                            {
                                MaxQueueSize = 2048,
                                ScheduledDelayMilliseconds = 5000,
                                ExporterTimeoutMilliseconds = 30000,
                                MaxExportBatchSize = 512,
                            };
                        })
                        .AddJaegerExporter(o =>
                        {
                            o.Endpoint = new Uri(HostMetadataCache.IsRunningInContainer ? "http://jaeger:6831" : "http://localhost:6831");
                            o.ExportProcessorType = ExportProcessorType.Batch;
                            o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                            {
                                MaxQueueSize = 2048,
                                ScheduledDelayMilliseconds = 5000,
                                ExporterTimeoutMilliseconds = 30000,
                                MaxExportBatchSize = 512,
                            };
                        });
                });
        }
    }
}

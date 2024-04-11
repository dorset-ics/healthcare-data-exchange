using System.Text.Json.Serialization;
using Api.BackgroundServices;
using Api.DocumentFilters;
using Api.Exceptions;
using Api.ResponseMappers;
using Carter;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Infrastructure.Ndop.Mesh.Configuration;
using Infrastructure.Ods.Configuration;
using Infrastructure.Pds.Mesh.Configuration;
using Microsoft.OpenApi.Models;
using Quartz;

namespace Api;

public static class DependencyInjection
{
    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddQuartzScheduler(configuration);

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddGlobalExceptionHandling()
            .AddEndpointsAndSwagger(configuration)
            .AddFhirJsonSerializer()
            .AddResponseMapperFactory();

    private static IServiceCollection AddResponseMapperFactory(this IServiceCollection services) =>
        services.AddSingleton<IResponseMapperFactory, ResponseMapperFactory>();

    private static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services) =>
        services.AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails();

    private static IServiceCollection AddEndpointsAndSwagger(this IServiceCollection services, IConfiguration configuration) =>
        services.AddEndpointsApiExplorer()
            .AddSwaggerGen(x =>
            {
                var openApiConfig = configuration.GetSection("OpenApi");
                string openTitle = openApiConfig.GetValue<string>("Title")!;
                string openApiVersion = openApiConfig.GetValue<string>("Version")!;
                x.SwaggerDoc(openApiVersion, new OpenApiInfo { Title = openTitle, Version = openApiVersion });
                x.DocumentFilter<FhirServerDocumentFilter>();
            })
            .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddCarter();

    private static IServiceCollection AddFhirJsonSerializer(this IServiceCollection services) =>
        services.ConfigureHttpJsonOptions(options =>
        {
            var converter = new FhirJsonConverterFactory(ModelInfo.ModelInspector, new(), new());
            options.SerializerOptions.Converters.Add(converter);
        });

    private static IServiceCollection AddQuartzScheduler(this IServiceCollection services, IConfiguration configuration) =>
        services.AddQuartz(q =>
            {
                var pdsMeshConfiguration = configuration.GetSection(PdsMeshConfiguration.SectionKey).Get<PdsMeshConfiguration>()
                                           ?? throw new Exception($"{PdsMeshConfiguration.SectionKey} has not been configured.");

                var meshPdsSendJobParams = new CronJobTriggerParameters(
                    "PDS MESH Send",
                    "Send messages to the PDS MESH mailbox",
                    "PDS MESH Send Trigger",
                    "Trigger to send messages to the PDS MESH mailbox",
                    pdsMeshConfiguration.SendSchedule);

                AddJobAndTrigger<PdsMeshSendBackgroundService>(q, meshPdsSendJobParams);

                var meshPdsRetrieveJobParams = new CronJobTriggerParameters(
                    "PDS MESH Retrieve",
                    "Retrieve messages from the PDS MESH mailbox",
                    "PDS MESH Retrieve Trigger",
                    "Trigger to retrieve messages from the PDS MESH mailbox",
                    pdsMeshConfiguration.RetrieveSchedule);

                AddJobAndTrigger<PdsMeshRetrieveBackgroundService>(q, meshPdsRetrieveJobParams);

                var ndopMeshConfiguration = configuration.GetSection(NdopMeshConfiguration.SectionKey).Get<NdopMeshConfiguration>()
                                           ?? throw new Exception($"{NdopMeshConfiguration.SectionKey} has not been configured.");

                var meshNdopSendJobParams = new CronJobTriggerParameters(
                    "NDOP MESH Send",
                    "Send messages to the NDOP MESH mailbox",
                    "NDOP MESH Send Trigger",
                    "Trigger to send messages to the NDOP MESH mailbox",
                    ndopMeshConfiguration.SendSchedule);

                AddJobAndTrigger<NdopMeshSendBackgroundService>(q, meshNdopSendJobParams);

                var meshNdopRetrieveJobParams = new CronJobTriggerParameters(
                    "NDOP MESH Retrieve",
                    "Retrieve messages from the NDOP MESH mailbox",
                    "NDOP MESH Retrieve Trigger",
                    "Trigger to retrieve messages from the NDOP MESH mailbox",
                    ndopMeshConfiguration.RetrieveSchedule);

                AddJobAndTrigger<NdopMeshRetrieveBackgroundService>(q, meshNdopRetrieveJobParams);

                var odsCsvDownloadConfiguration = configuration.GetSection(OdsCsvDownloadConfiguration.SectionKey).Get<OdsCsvDownloadConfiguration>()
                                           ?? throw new Exception($"{OdsCsvDownloadConfiguration.SectionKey} has not been configured.");

                var odsCsvDownloadJobParams = new CronJobTriggerParameters(
                    "ODS CSV Download",
                    "Import Organisations from ODS CSV downloads",
                    "ODS CSV Download Trigger",
                    "Trigger to import organisations from ODS CSV downloads",
                    odsCsvDownloadConfiguration.ImportSchedule);

                AddJobAndTrigger<OdsCsvDownloadBackgroundService>(q, odsCsvDownloadJobParams);
            })
            .AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

    private static void AddJobAndTrigger<T>(IServiceCollectionQuartzConfigurator q, CronJobTriggerParameters parameters) where T : IJob
    {
        var jobKeyObj = new JobKey(parameters.JobKey);
        q.AddJob<T>(jobKeyObj, j => j.WithDescription(parameters.JobDescription));
        q.AddTrigger(t => t
            .WithIdentity(parameters.TriggerKey)
            .ForJob(jobKeyObj)
            .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(3)))
            .WithCronSchedule(parameters.CronSchedule)
            .WithDescription(parameters.TriggerDescription)
        );
    }

    internal record CronJobTriggerParameters(string JobKey, string JobDescription, string TriggerKey, string TriggerDescription, string CronSchedule);
}
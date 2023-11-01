﻿using AICentral.Configuration;
using AICentral.Configuration.JSON;
using Microsoft.Extensions.Logging.Abstractions;

namespace AICentral;

public static class ConfigurationEx
{
    public static IServiceCollection AddAICentral(
        this IServiceCollection services,
        AICentralPipelineAssembler providedOptions,
        ILogger? startupLogger = null)
    {
        var logger = startupLogger ?? NullLogger.Instance;
        providedOptions.AddServices(services, logger);
        return services;
    }

    public static IServiceCollection AddAICentral(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "AICentral",
        ILogger? startupLogger = null)
    {
        var logger = startupLogger ?? NullLogger.Instance;
        logger.LogInformation("AICentral - Initialising pipelines");

        var configSection = configuration.GetSection(configSectionName);
        var optionsFromConfig = new ConfigurationBasedPipelineBuilder().BuildPipelinesFromConfig(logger,
            configuration.GetSection(configSectionName),
            configSection.Get<ConfigurationTypes.AICentralConfig>());
        
        services.AddAICentral(optionsFromConfig);

        return services;
    }

    public static void UseAICentral(this WebApplication webApplication)
    {
        var aiCentral = webApplication.Services.GetRequiredService<AICentralPipelines>();
        aiCentral.BuildRoutes(webApplication);
    }
}
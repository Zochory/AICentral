﻿using Microsoft.Extensions.Primitives;

namespace AICentral.Pipelines.Endpoints.AzureOpenAI;

public class AzureOpenAIActionResultHandler: IResult, IDisposable
{
    private readonly HttpResponseMessage _openAiResponseMessage;
    private readonly AICentralUsageInformation _aiCentralUsageInformation;

    public AzureOpenAIActionResultHandler(
        HttpResponseMessage openAiResponseMessage,
        AICentralUsageInformation aiCentralUsageInformation)
    {
        _openAiResponseMessage = openAiResponseMessage;
        _aiCentralUsageInformation = aiCentralUsageInformation;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.Headers.Add("x-aicentral-duration", _aiCentralUsageInformation.Duration.TotalMilliseconds.ToString());
        
        foreach (var responseHeader in _openAiResponseMessage.Headers)
        {
            context.Response.Headers.Add(responseHeader.Key, new StringValues(responseHeader.Value.ToArray()));
        }
        await context.Response.WriteAsync(await _openAiResponseMessage.Content.ReadAsStringAsync());
    }

    public void Dispose()
    {
        _openAiResponseMessage.Dispose();
    }
}
﻿using System.Text.Json;
using AICentral.Core;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace AICentral;

public class AzureOpenAIDetector
{
    public async Task<IncomingCallDetails> Detect(string pipelineName, string deploymentName, AICallType callType, HttpRequest request, CancellationToken cancellationToken)
    {
        return callType switch
        {
            AICallType.Chat => await DetectChat(pipelineName, deploymentName, request, cancellationToken),
            AICallType.Completions => await DetectCompletions(pipelineName, deploymentName, request, cancellationToken),
            AICallType.Embeddings => await DetectEmbeddings(pipelineName, deploymentName, request, cancellationToken),
            AICallType.Transcription => DetectTranscription(pipelineName, deploymentName, request),
            AICallType.Translation => DetectTranslation(pipelineName, deploymentName, request),
            AICallType.Operations => DetectOperations(pipelineName, request),
            AICallType.DALLE2 => await DetectDalle2(pipelineName, request, cancellationToken),
            AICallType.DALLE3 => await DetectDalle3(pipelineName, deploymentName, request, cancellationToken),
            _ => new IncomingCallDetails(pipelineName, callType, null, null, null, QueryHelpers.ParseQuery(request.QueryString.Value), null)
        };
    }

    private async Task<IncomingCallDetails> DetectChat(string pipelineName, string deploymentName, HttpRequest request, CancellationToken cancellationToken)
    {
        var requestContent = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
        return new IncomingCallDetails(
            pipelineName,
            AICallType.Chat,
            string.Join(
                '\n',
                requestContent.RootElement.GetProperty("messages").EnumerateArray()
                    .Select(x => x.GetProperty("content").GetString())),
            deploymentName,
            requestContent,
            QueryHelpers.ParseQuery(request.QueryString.Value),
            null);
    }

    private async Task<IncomingCallDetails> DetectCompletions(string pipelineName, string deploymentName, HttpRequest request, CancellationToken cancellationToken)
    {
        var requestContent = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
        return new IncomingCallDetails(
            pipelineName,
            AICallType.Completions,
            string.Join('\n', requestContent.RootElement.GetProperty("prompt").EnumerateArray().Select(x => x.GetString())),
            deploymentName,
            requestContent,
            QueryHelpers.ParseQuery(request.QueryString.Value),
            null);
    }

    private async Task<IncomingCallDetails> DetectEmbeddings(string pipelineName, string deploymentName, HttpRequest request, CancellationToken cancellationToken)
    {
        var requestContent = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
        return new IncomingCallDetails(
            pipelineName,
            AICallType.Embeddings,
            requestContent.RootElement.GetProperty("input").GetString() ?? string.Empty,
            deploymentName,
            requestContent,
            QueryHelpers.ParseQuery(request.QueryString.Value),
            null);
    }

    private IncomingCallDetails DetectTranscription(string pipelineName, string deploymentName, HttpRequest request)
    {
        return new IncomingCallDetails(
            pipelineName,
            AICallType.Transcription,
            null,
            deploymentName,
            null,
            QueryHelpers.ParseQuery(request.QueryString.Value),
            null);
    }

    private IncomingCallDetails DetectTranslation(string pipelineName, string deploymentName, HttpRequest request)
    {
        return new IncomingCallDetails(
            pipelineName,
            AICallType.Translation,
            null,
            deploymentName,
            null,
            QueryHelpers.ParseQuery(request.QueryString.Value),
            null);
    }
    
    private async Task<IncomingCallDetails> DetectDalle2(string pipelineName, HttpRequest request, CancellationToken cancellationToken)
    {
        var requestContent = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
        return new IncomingCallDetails(
            pipelineName,
            AICallType.DALLE2,
            null,
            null,
            requestContent,
            QueryHelpers.ParseQuery(request.QueryString.Value),
            null);
    }    
    private IncomingCallDetails DetectOperations(string pipelineName, HttpRequest request)
    {
        var queryString = QueryHelpers.ParseQuery(request.QueryString.Value);
        var endpointAffinity = LookForAffinityOnRequest(queryString);

        return new IncomingCallDetails(
            pipelineName,
            AICallType.Operations,
            null,
            null,
            null,
            queryString,
            endpointAffinity);
    }
    
    private async Task<IncomingCallDetails> DetectDalle3(string pipelineName, string deploymentName, HttpRequest request, CancellationToken cancellationToken)
    {
        var requestContent = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
        return new IncomingCallDetails(
            pipelineName,
            AICallType.DALLE3,
            null,
            deploymentName,
            requestContent,
            QueryHelpers.ParseQuery(request.QueryString.Value),
            null);
    }
    
    /// <summary>
    /// A consumer may want affinity to a particular endpoint.
    /// </summary>
    /// <remarks>
    /// DALLE-2 on Azure Open AI is a good example where the operation is asynchronous involving multiple calls. 
    /// </remarks>
    /// <param name="requestQueryString"></param>
    /// <returns></returns>
    private string? LookForAffinityOnRequest(Dictionary<string, StringValues> requestQueryString)
    {
        if (requestQueryString.TryGetValue(QueryPartNames.AzureOpenAIHostAffinityQueryStringName,
                out var affinityMarker))
        {
            if (affinityMarker.Count == 1)
            {
                requestQueryString.Remove(QueryPartNames.AzureOpenAIHostAffinityQueryStringName);
                return affinityMarker.Single();
            }
        }

        return null;
    }
}
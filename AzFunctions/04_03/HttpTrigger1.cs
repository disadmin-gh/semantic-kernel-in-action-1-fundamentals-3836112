using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace dis.function;

class WhatDateIsIt
{
    [KernelFunction, Description("Get the current date")]
    public string Date(IFormatProvider? formatProvider = null) =>
        DateTimeOffset.UtcNow.ToString("D", formatProvider);
}

public class HttpTrigger1
{
    private readonly ILogger<HttpTrigger1> _logger;

    public HttpTrigger1(ILogger<HttpTrigger1> logger)
    {
        _logger = logger;
    }
    [Function("HttpTrigger1")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        var modelDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4.1";
        var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://dis-openai-0705.openai.azure.com/";
        var azureOpenAIApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        _logger.LogInformation($"Model Deployment Name: {modelDeploymentName}");
        _logger.LogInformation($"OpenAI Endpoint: {azureOpenAIEndpoint}");
        _logger.LogInformation($"OpenAI API Key: {(string.IsNullOrEmpty(azureOpenAIApiKey) ? "[Not Set]" : "[Loaded]")}");

        // Optional: Return the variable values in the response
        /*var result = new
        {
            DeploymentName = modelDeploymentName,
            Endpoint = azureOpenAIEndpoint,
            KeyStatus = string.IsNullOrEmpty(azureOpenAIApiKey) ? "Not Set" : "Loaded"
        };
        */

        var builder = Kernel.CreateBuilder();
        builder.Services.AddAzureOpenAIChatCompletion(
            modelDeploymentName,
            azureOpenAIEndpoint,
            azureOpenAIApiKey,
            modelId: "gpt-4.1"
        );
        var kernel = builder.Build();

        KernelFunction kernelFunctionRespondAsScientific =
               KernelFunctionFactory.CreateFromPrompt(
                   "Respond to the user question as if you were a Scientific. Respond to it as you were him, showing your personality",
                   functionName: "RespondAsScientific",
                   description: "Responds to a question as a Scientific.");

        KernelFunction kernelFunctionRespondAsPoliceman =
            KernelFunctionFactory.CreateFromPrompt(
                "Respond to the user question as if you were a Policeman. Respond to it as you were him, showing your personality, humor and level of intelligence.",
                functionName: "RespondAsPoliceman",
                description: "Responds to a question as a Policeman.");

        KernelPlugin roleOpinionsPlugin =
            KernelPluginFactory.CreateFromFunctions(
                "RoleTalk",
                "Responds to questions or statements asuming different roles.",
                new[] {
                    kernelFunctionRespondAsScientific,
                    kernelFunctionRespondAsPoliceman
                      });
        kernel.Plugins.Add(roleOpinionsPlugin);
        kernel.Plugins.AddFromType<WhatDateIsIt>();

        string userPrompt = "I just woke up and found myself in the middle of nowhere, " +
            "do you know what date is it? and what would a policeman and a scientist do in my place?" +
            "Please provide me the date using the WhatDateIsIt plugin and the Date function, and then " +
            "the responses from the policeman and the scientist, on this order. " +
            "For this two responses, use the RoleTalk plugin and the RespondAsPoliceman and RespondAsScientific functions.";

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var result = await kernel.InvokePromptAsync(
            userPrompt,
            new(openAIPromptExecutionSettings)); var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };

        return new OkObjectResult(result.GetValue<string>());



        //return new OkObjectResult(result);
    }
}
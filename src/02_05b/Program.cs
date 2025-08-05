using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.IO;

using System;
using System.IO;


class EnvLoader
{
    public static void Load(string filePath = ".env")
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"⚠️ File '{filePath}' not found.");
            return;
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            var parts = trimmed.Split('=', 2); // Only split at the first '='
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"'); // Remove surrounding quotes
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}


internal class Program
{
    static async Task Main(string[] args)
    {
        EnvLoader.Load(); // Loads from .env

        // Example: retrieve a specific environment variable
        string? modelId = Environment.GetEnvironmentVariable("modelId");
        string? endPoint = Environment.GetEnvironmentVariable("endPoint");
        string? apiKey = Environment.GetEnvironmentVariable("apiKey");
        Console.WriteLine($"modelId: {modelId}");
        Console.WriteLine($"endPoint: {endPoint}");
        Console.WriteLine($"apiKey: {apiKey}");

        if (string.IsNullOrWhiteSpace(modelId) || string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("❌ modelId or apiKey environment variables are missing. Please check your .env file.");
            return;
        }

        Console.WriteLine("Launching Semantic Kernel Sandpit");

        var builder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(modelId, apiKey);


        Kernel kernel = builder.Build();
        // Create chat history
        var history = new ChatHistory("You are a mischevious and jovial assistant. You will crack jokes when a user supplies a prompt.");


        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Start the conversation
        Console.Write("User > ");
        string? userInput;

        while ((userInput = Console.ReadLine()) != null)
        {
            // Add user input
            history.AddUserMessage(userInput);

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings()
          
            {
            };


            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);


            // Print the results
            Console.WriteLine("Assistant > " + result);

            // Add the message from the agent to the chat history
            history.AddMessage(result.Role, result.Content ?? string.Empty);

            // Get user input again
            Console.Write("User > ");
        }
    }
}

using Microsoft.SemanticKernel;
using System.IO;

namespace _05_03e;

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

public class AgentCraftingPractice
{
  public async Task Execute()
  {
    EnvLoader.Load(); // Loads from .env

// Example: retrieve a specific environment variable
    var modelId = Environment.GetEnvironmentVariable("modelId");
    var endPoint = Environment.GetEnvironmentVariable("endPoint");
    var apiKey = Environment.GetEnvironmentVariable("apiKey");


    var builder = Kernel.CreateBuilder();
    builder.Services.AddAzureOpenAIChatCompletion(
        modelId,
        endPoint,
        apiKey);
    var kernel = builder.Build();

    // Create a pirate parrot function using modern SK patterns
    var pirateParrotFunction = KernelFunctionFactory.CreateFromPrompt(
        "Repeat the user message in the voice of a pirate and then end with parrot sounds. " +
        "User message: {{$input}}",
        functionName: "PirateParrot",
        description: "A fun chat bot that repeats the user message in the voice of a pirate."
    );

    // Create a function from the YAML template
    var parrotFromFileFunction = await CreateFunctionFromYamlAsync();

    try
    {
      Console.WriteLine("=== Testing Code-Based Pirate Parrot ===");
      var codeResponse = await kernel.InvokeAsync(pirateParrotFunction,
          new KernelArguments { ["input"] = "Practice makes perfect." });
      Console.WriteLine($"Code Agent Response: {codeResponse}");

      Console.WriteLine("\n=== Testing File-Based Parrot ===");
      var fileResponse = await kernel.InvokeAsync(parrotFromFileFunction,
          new KernelArguments
          {
            ["input"] = "Practice makes perfect.",
            ["count"] = "2"
          });
      Console.WriteLine($"File Agent Response: {fileResponse}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
    }

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadLine();
  }

  private Task<KernelFunction> CreateFunctionFromYamlAsync()
  {
    var pathToPlugin = Path.Combine(Directory.GetCurrentDirectory(), "Agents", "ParrotAgent.yaml");

    // For now, let's create the function based on the YAML content manually
    // The YAML approach isn't directly supported in current SK, so we'll simulate it
    var template = "Repeat the user message in the voice of a parrot and then end with {{$count}} parrot sounds that sound funny. User message: {{$input}}";

    var function = KernelFunctionFactory.CreateFromPrompt(
        template,
        functionName: "ParrotFromFile",
        description: "A fun chat agent that repeats the user message like a parrot would."
    );

    return Task.FromResult(function);
  }
}
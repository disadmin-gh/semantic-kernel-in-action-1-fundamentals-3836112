using System;
using System.IO;

namespace Shared
{
  public static class EnvConfig
  {
    private static readonly Dictionary<string, string> _envVars = new();
    private static bool _loaded = false;

    static EnvConfig()
    {
      LoadEnvFile();
    }

    public static string GetValue(string key, string defaultValue = "")
    {
      if (!_loaded)
        LoadEnvFile();

      // First check environment variables
      var envValue = Environment.GetEnvironmentVariable(key);
      if (!string.IsNullOrEmpty(envValue))
        return envValue;

      // Then check .env file
      if (_envVars.TryGetValue(key, out var value))
        return value;

      return defaultValue;
    }

    public static string AzureOpenAIEndpoint => GetValue("AZURE_OPENAI_ENDPOINT");
    public static string AzureOpenAIKey => GetValue("AZURE_OPENAI_KEY");
    public static string AzureOpenAIDeploymentName => GetValue("AZURE_OPENAI_DEPLOYMENT_NAME");
    public static string OpenAIKey => GetValue("OPENAI_API_KEY");

    private static void LoadEnvFile()
    {
      try
      {
        // Look for .env file starting from current directory and going up
        var currentDir = Directory.GetCurrentDirectory();
        var envFile = FindEnvFile(currentDir);

        if (envFile != null && File.Exists(envFile))
        {
          var lines = File.ReadAllLines(envFile);
          foreach (var line in lines)
          {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
              continue;

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
              var key = parts[0].Trim();
              var value = parts[1].Trim();
              _envVars[key] = value;
            }
          }
        }
        _loaded = true;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Warning: Could not load .env file: {ex.Message}");
        _loaded = true;
      }
    }

    private static string? FindEnvFile(string startDirectory)
    {
      var current = new DirectoryInfo(startDirectory);

      while (current != null)
      {
        var envFile = Path.Combine(current.FullName, ".env");
        if (File.Exists(envFile))
          return envFile;

        // Also check if we're in a subdirectory and there's a .env in src
        var srcEnvFile = Path.Combine(current.FullName, "src", ".env");
        if (File.Exists(srcEnvFile))
          return srcEnvFile;

        current = current.Parent;
      }

      return null;
    }
  }
}

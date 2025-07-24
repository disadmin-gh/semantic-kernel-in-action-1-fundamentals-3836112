using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Simple .env file loader for Semantic Kernel projects
/// Copy this class to any project that needs to load .env configuration
/// </summary>
public static class EnvConfigHelper
{
  private static Dictionary<string, string>? _cachedConfig;

  /// <summary>
  /// Gets a value from the .env file or environment variables
  /// </summary>
  /// <param name="key">The configuration key</param>
  /// <param name="defaultValue">Default value if key is not found</param>
  /// <returns>The configuration value</returns>
  public static string GetValue(string key, string defaultValue = "")
  {
    var config = LoadConfig();

    // First check environment variables
    var envValue = Environment.GetEnvironmentVariable(key);
    if (!string.IsNullOrEmpty(envValue))
      return envValue;

    // Then check .env file
    return config.GetValueOrDefault(key, defaultValue);
  }

  /// <summary>
  /// Loads the .env configuration file
  /// </summary>
  /// <returns>Dictionary of key-value pairs from .env file</returns>
  public static Dictionary<string, string> LoadConfig()
  {
    if (_cachedConfig != null)
      return _cachedConfig;

    _cachedConfig = new Dictionary<string, string>();

    try
    {
      var envFile = FindEnvFile();

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
            // Remove quotes if present
            if (value.StartsWith("\"") && value.EndsWith("\""))
              value = value[1..^1];
            _cachedConfig[key] = value;
          }
        }
        Console.WriteLine($"✅ Loaded .env from: {envFile}");
      }
      else
      {
        Console.WriteLine("⚠️ No .env file found");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error loading .env: {ex.Message}");
    }

    return _cachedConfig;
  }

  /// <summary>
  /// Finds the .env file by searching up the directory tree
  /// </summary>
  private static string? FindEnvFile()
  {
    var current = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (current != null)
    {
      // Check for .env in current directory
      var envFile = Path.Combine(current.FullName, ".env");
      if (File.Exists(envFile))
        return envFile;

      // Check for .env in src subdirectory (if we're in the project root)
      var srcEnvFile = Path.Combine(current.FullName, "src", ".env");
      if (File.Exists(srcEnvFile))
        return srcEnvFile;

      current = current.Parent;
    }

    return null;
  }

  // Convenience properties for common configuration values
  public static string AzureOpenAIEndpoint => GetValue("AZURE_OPENAI_ENDPOINT");
  public static string AzureOpenAIKey => GetValue("AZURE_OPENAI_KEY");
  public static string AzureOpenAIDeploymentName => GetValue("AZURE_OPENAI_DEPLOYMENT_NAME");
  public static string OpenAIKey => GetValue("OPENAI_API_KEY");
}

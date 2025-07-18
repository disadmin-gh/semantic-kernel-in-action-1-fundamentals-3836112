using Microsoft.SemanticKernel;

namespace _02_05b
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var modelDeploymentName = "gpt-4.1";
            var azureOpenAIEndpoint = "https://dis-openai-0705.openai.azure.com/";
            var azureOpenAIKey = "DJiBCSANPd42dZGBApT4G6CeLD4gRSBeU0KmajSBSXzFbiUFY94SJQQJ99BGACHYHv6XJ3w3AAABACOG1pXm";

            var builder = Kernel.CreateBuilder();
            builder.Services.AddAzureOpenAIChatCompletion(
                modelDeploymentName,
                azureOpenAIEndpoint,
                azureOpenAIKey
            );

            var kernel = builder.Build();

            var topic = "The Semantic Kernel SDK has been born on December 19th ";
            var prompt = $"Generate a very short funny poem about the given event Event:{topic}";

            var result = await kernel.InvokePromptAsync(prompt);

            Console.WriteLine(result);
        }
    }
}

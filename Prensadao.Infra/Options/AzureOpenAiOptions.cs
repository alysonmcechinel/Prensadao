namespace Prensadao.Infra.Options;

public class AzureOpenAiOptions
{
    public const string SectionName = "AzureOpenAi";

    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 2048;
    public float Temperature { get; set; } = 1;
}

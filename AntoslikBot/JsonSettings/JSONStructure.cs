namespace AntoslikBot.JsonSettings;

internal sealed class JSONStructure
{
    public string Token { get; set; } = null!;
    public string AiToken { get; set; } = null!;
    public string Prefix { get; set; } = null!;
    public string[] Phrases { get; set; } = null!;
    public ulong[] TriggerIDs { get; set; } = null!;
    public string Prompt { get; set; } = null!;
}
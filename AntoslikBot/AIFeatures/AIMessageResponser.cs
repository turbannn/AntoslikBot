using AntoslikBot.JsonSettings;
using Mscc.GenerativeAI;
using System.Text;
using System.Text.RegularExpressions;

namespace AntoslikBot.AIFeatures;

public class AIMessageResponser
{
    private readonly GenerativeModel _model;
    private readonly ChatSession _chat;
    private readonly JSONReader _jSONReader;

    public AIMessageResponser(JSONReader reader)
    {
        _jSONReader = reader;
        _model = new GenerativeModel()
        {
            ApiKey = reader.jSON.AiToken,
        };
        _chat = _model.StartChat();
    }

    public async Task<string> GenerateResponceAsync(string question)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"System prompt: {_jSONReader.jSON.Prompt}");
        sb.Append("LegendaryPhrases: ");
        foreach (var p in _jSONReader.jSON.Phrases)
        {
            sb.Append(p + ", " + "\n");
        }
        sb.Append($" \n\n Question: {CleanMessageContent(question)}");

        string combinedPrompt = sb.ToString();

        try
        {
            var response = await _chat.SendMessage(combinedPrompt);

            if (response != null && !string.IsNullOrEmpty(response.Text))
            {
                return response.Text;
            }
            else
            {
                return "No text in response.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
    private string CleanMessageContent(string messageContent)
    {
        StringBuilder cleanedMessage = new StringBuilder(messageContent);

        cleanedMessage.Replace(Regex.Match(cleanedMessage.ToString(), @"<@!?(\d+)>|<@&(\d+)>|<@!&(\d+)>|@everyone|@here").Value, ""); //del mentions
        cleanedMessage.Replace(Regex.Match(cleanedMessage.ToString(), @"<#\d+>").Value, ""); //del channel references
        cleanedMessage.Replace(Regex.Match(cleanedMessage.ToString(), @"^\!?\w+").Value, ""); //del commands
        cleanedMessage.Replace(Regex.Match(cleanedMessage.ToString(), @"\s+").Value, " "); //del spaces

        return cleanedMessage.ToString().Trim();
    }
}
using AntoslikBot.JsonSettings;
using Mscc.GenerativeAI;
using System.Text;

namespace AntoslikBot.AIFeatures;

public class AIMessageResponser
{
    private readonly GenerativeModel _model;
    private readonly ChatSession _chat;
    private readonly List<string> _phrases;
    private readonly string _systemPrompt;

    public AIMessageResponser(JSONReader reader)
    {
        _systemPrompt = reader.jSON.Prompt;
        _phrases = [.. reader.jSON.Phrases];
        _model = new GenerativeModel()
        {
            ApiKey = "AIzaSyDDsYodikm3YZ_hwZ0ffklWjYq_o9BBy1E",
        };
        _chat = _model.StartChat();
    }

    public async Task<string> GenerateResponceAsync(string question)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"System prompt: {_systemPrompt}");
        sb.Append("LegendaryPhrases: ");
        foreach (var p in _phrases)
        {
            sb.Append(p + ", " + "\n");
        }
        sb.Append($" \n\n Question: {question}");

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

}

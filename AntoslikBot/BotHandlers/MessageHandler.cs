using Discord.WebSocket;
using Discord;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using AntoslikBot.AIFeatures;
using AntoslikBot.JsonSettings;
using System.Text;

namespace AntoslikBot.BotHandlers;

//TODO: make IHandler interface for all handlers, refactor DI
internal class MessageHandler
{
    #region Fields
    private static Dictionary<int, string> LegendaryPhrases { get; set; } = null!; //cant be readonly, look up why
    private readonly JSONStructure _config;
    private readonly ulong _currentBotId;
    private readonly AIMessageResponser _aiMessageResponser;

    private static readonly ConcurrentQueue<(string Prefix, SocketMessage Msg)> _commandQueue = new();
    private static readonly Queue<Func<Task>> _aiTasksQueue = new();

    private const int MAX_MESSAGE_LENGTH = 170;
    private int COMMAND_QUERY_SEMAFOR;
    #endregion

    #region Ctors
    public MessageHandler(JSONReader reader, DiscordSocketClient client, AIMessageResponser aiResponser)
    {
        _config = reader.jSON;
        _currentBotId = client.CurrentUser.Id;
        _aiMessageResponser = aiResponser;

        LegendaryPhrases = new Dictionary<int, string>();
        for (int i = 0; i < reader.jSON.Phrases.Length; i++)
        {
            LegendaryPhrases.Add(i, reader.jSON.Phrases[i]);
        }

        COMMAND_QUERY_SEMAFOR = 0;
    }
    #endregion

    #region Methods
    public void EnqueueCommand(string prefix, SocketMessage msg)
    {
        _commandQueue.Enqueue((prefix, msg));
        Task.Run(ProcessCommandQueueAsync);
        COMMAND_QUERY_SEMAFOR++;
    }
    private void EnqueueAiTask(SocketMessage msg)
    {
        _aiTasksQueue.Enqueue(async () =>
        {
            string response = await _aiMessageResponser.GenerateResponceAsync(CleanMessageContent(msg.Content));
            await msg.Channel.SendMessageAsync(response, messageReference: new MessageReference(msg.Id));
        });

        // Ensure queue processing starts
        Task.Run(ProcessAiTasksQueueAsync);
    }

    private async Task ProcessCommandQueueAsync()
    {
        while (_commandQueue.TryDequeue(out var command))
        {
            await CommandHandler.HandleTheCommandAsync(command.Prefix, command.Msg);
            if (COMMAND_QUERY_SEMAFOR > 0)
            {
                COMMAND_QUERY_SEMAFOR--;
            }
        }
    }
    public async Task ProcessAiTasksQueueAsync()
    {
        while (_aiTasksQueue.Count > 0)
        {
            Func<Task>? taskToRun = null;

            if (_aiTasksQueue.Count > 0)
            {
                taskToRun = _aiTasksQueue.Dequeue();
            }

            if (taskToRun != null)
            {
                try
                {
                    await taskToRun();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in task execution: {ex.Message}");
                }
            }
            else
            {
                await Task.Delay(50); // No task to process, wait before checking again
            }
        }
    }
    public string CleanMessageContent(string messageContent)
    {
        StringBuilder cleanedMessage = new StringBuilder(messageContent);

        // del mentions
        cleanedMessage.Replace(Regex.Match(cleanedMessage.ToString(), @"<@!?(\d+)>|<@&(\d+)>|<@!&(\d+)>|@everyone|@here").Value, "");

        // del channel references
        cleanedMessage.Replace(Regex.Match(cleanedMessage.ToString(), @"<#\d+>").Value, "");

        // del commands
        cleanedMessage.Replace(Regex.Match(cleanedMessage.ToString(), @"^\!?\w+").Value, "");

        // del spaces
        cleanedMessage.Replace(Regex.Match(cleanedMessage.ToString(), @"\s+").Value, " ");

        return cleanedMessage.ToString().Trim();
    }
    public async Task MessagesHandler(SocketMessage msg)
    {
        if (msg.Author.IsBot)
            return;

        if (msg.MentionedUsers.Any(user => user.Id == _currentBotId))
        {
            EnqueueAiTask(msg);
            return;
        }

        if (msg.Content.StartsWith(_config.Prefix) && COMMAND_QUERY_SEMAFOR > 0)
        {
            await msg.Channel.SendMessageAsync(embed:
                new EmbedBuilder()
                .WithTitle("Query error")
                .WithDescription("Too many commands in query")
                .Build());
            return;
        }

        if (msg.Content.StartsWith(_config.Prefix))
        {
            EnqueueCommand(_config.Prefix, msg);
            return;
        }

        if (!_config.TriggerIDs.Contains(msg.Author.Id))
        {
            return;
        }

        if (msg.Content.Length >= MAX_MESSAGE_LENGTH)
        {
            return;
        }

        if (msg.Content.EndsWith(".gif"))
        {
            await msg.Channel.SendMessageAsync("я", messageReference: new MessageReference(msg.Id));
            return;
        }

        if (msg.Attachments.Count > 0)
        {

            foreach (var attachment in msg.Attachments)
            {
                if (attachment.ContentType.StartsWith("image/")) // types image/png, image/jpeg, image/gif и т.д.
                {
                    await msg.Channel.SendMessageAsync("я", messageReference: new MessageReference(msg.Id));
                    return;
                }
            }

            return;
        }
    }
    #endregion
}

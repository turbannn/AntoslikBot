using Discord.WebSocket;
using Discord;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using AntoslikBot.AIFeatures;
using AntoslikBot.JsonSettings;
using System.Text;
using AntoslikBot.Interfaces;

namespace AntoslikBot.BotHandlers;

//TODO: refactor additional methods
internal class MessageHandler
{
    #region Fields
    //used to need System.Collections.Generic.Dictionary in previous bot version, but now there is no special need, nevertheless let it be
    private static Dictionary<int, string> LegendaryPhrases { get; set; } = null!; //cant be readonly, look up why


    private readonly JSONReader _jSONReader;
    private readonly DiscordSocketClient _client;
    private readonly AIMessageResponser _aiMessageResponser;

    private static readonly ConcurrentQueue<(string Prefix, SocketMessage Msg)> _commandQueue = new();
    private static readonly Queue<Func<Task>> _aiTasksQueue = new();

    private const int MAX_MESSAGE_LENGTH = 170;
    private int COMMAND_QUERY_SEMAFOR;
    #endregion

    #region Ctors
    public MessageHandler(JSONReader reader, DiscordSocketClient client, AIMessageResponser aiResponser)
    {
        _jSONReader = reader;
        _client = client;
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
        Task.Run(ProcessCommandAsync);
        COMMAND_QUERY_SEMAFOR++;
    }
    private async Task ProcessCommandAsync()
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
    private void EnqueueAiTask(SocketMessage msg)
    {
        _aiTasksQueue.Enqueue(async () =>
        {
            string response = await _aiMessageResponser.GenerateResponceAsync(msg.Content);
            await msg.Channel.SendMessageAsync(response, messageReference: new MessageReference(msg.Id));
        });

        // Ensure queue processing starts
        Task.Run(ProcessAiTaskAsync);
    }

    //Issue: REALLY F bad implementation, cuncurency troubles, total refactor needed
    public async Task ProcessAiTaskAsync()
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
                await Task.Delay(100); // No task to process, wait before checking again
            }
        }
    }

    public async Task Handle(SocketMessage msg)
    {
        if (msg.Author.IsBot)
            return;

        if (msg.MentionedUsers.Any(user => user.Id == _client.CurrentUser.Id))
        {
            EnqueueAiTask(msg);
            return;
        }

        if (msg.Content.StartsWith(_jSONReader.jSON.Prefix) && COMMAND_QUERY_SEMAFOR > 0)
        {
            await msg.Channel.SendMessageAsync(embed:
                new EmbedBuilder()
                .WithTitle("Query error")
                .WithDescription("Too many commands in query")
                .Build());
            return;
        }

        if (msg.Content.StartsWith(_jSONReader.jSON.Prefix))
        {
            EnqueueCommand(_jSONReader.jSON.Prefix, msg);
            return;
        }

        if (!_jSONReader.jSON.TriggerIDs.Contains(msg.Author.Id))
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

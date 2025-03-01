namespace AntoslikBot.Program
{
    internal class Program
    {
        private static Task Main(string[] args)
            => new AntonBot().RunBotAsync(args);
    }
}

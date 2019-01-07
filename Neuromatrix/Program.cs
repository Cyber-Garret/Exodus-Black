using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Neuromatrix.Resources.Datatypes;
using Neuromatrix.Resources.Settings;

namespace Neuromatrix
{
    class Program
    {
        private DiscordSocketClient Client;
        private CommandService Command;

        static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            string JSON = "";
            string SettingsLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"/Data/Settings.json";
            if (!File.Exists(SettingsLocation))
            {
                Console.WriteLine($"Not found file {SettingsLocation}");
                Console.ReadLine();
                return;
            }
            using (var Stream = new FileStream(SettingsLocation, FileMode.Open, FileAccess.Read))
            using (var ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            Settings Settings = JsonConvert.DeserializeObject<Settings>(JSON);
            StaticSettings.Token = Settings.token;
            StaticSettings.Owner = Settings.owner;
            StaticSettings.Log = Settings.log;
            StaticSettings.Version = Settings.version;
            StaticSettings.Banned = Settings.banned;

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });
            Command = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
            });

            Client.MessageReceived += Client_MessageReceived;
            await Command.AddModulesAsync(Assembly.GetEntryAssembly());

            Client.Ready += Client_Ready;
            Client.Log += Client_Log;

            await Client.LoginAsync(TokenType.Bot, StaticSettings.Token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Client_Log(LogMessage logMessage)
        {
            Console.WriteLine($"[{DateTime.Now} в {logMessage.Source}] {logMessage.Message}");
            try
            {
                SocketGuild Guild = Client.Guilds.Where(x => x.Id == StaticSettings.Log[0]).FirstOrDefault();
                SocketTextChannel TextChannel = Guild.Channels.Where(x => x.Id == StaticSettings.Log[1]).FirstOrDefault() as SocketTextChannel;
                await TextChannel.SendMessageAsync($"[{DateTime.Now} в {logMessage.Source}] {logMessage.Message}");
            }
            catch
            {
                //Console.WriteLine($"[{DateTime.Now} at Logs] Something went wrong with SocketGuild or SocketTextChannel. Message: {ex.Message}");
            }
        }

        private async Task Client_Ready()
        {
            await Client.SetGameAsync("Destiny 2", null, StreamType.NotStreaming);
        }

        private async Task Client_MessageReceived(SocketMessage MessageParams)
        {
            var Message = MessageParams as SocketUserMessage;
            var Context = new SocketCommandContext(Client, Message);

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.IsBot) return;

            int ArgPos = 0;
            if (!(Message.HasStringPrefix("!", ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos))) return;

            var Result = await Command.ExecuteAsync(Context, ArgPos);
            if (!Result.IsSuccess)
                Console.WriteLine($"[{DateTime.Now} in command ] Sometimes went wrong with commands. Text: {Context.Message.Content} | Error: {Result.ErrorReason}");
        }
    }
}

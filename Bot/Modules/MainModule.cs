using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
	public class MainModule : ModuleBase<SocketCommandContext>
	{
		[Command("бип")]
		[Summary("Простая команда проверки моей работоспособности.")]
		public async Task Bip()
		{
			await ReplyAsync("Бип...");
		}

	}
}

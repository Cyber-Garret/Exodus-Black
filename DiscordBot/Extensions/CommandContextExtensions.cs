using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace DiscordBot.Extensions
{
	public static class CommandContextExtensions
	{
		public static readonly Emoji _HeavyCheckMark = new Emoji("\u2714");
		public static readonly Emoji _X = new Emoji("\u274C");
		private const int _confirmationTimeoutSeconds = 10;

		public static async Task<bool?> GetUserConfirmationAsync(this ICommandContext context, string mainMessage)
		{
			if (!mainMessage.EndsWith(Environment.NewLine))
				mainMessage += Environment.NewLine;

			var confirmationMessage = await context.Channel.SendMessageAsync(mainMessage +
				$"Нажми на {_HeavyCheckMark} или {_X} в течении следующих {_confirmationTimeoutSeconds} сек. для подтверждения или отмены операции.");

			await confirmationMessage.AddReactionAsync(_HeavyCheckMark);
			await confirmationMessage.AddReactionAsync(_X);

			for (var i = 0; i < _confirmationTimeoutSeconds; i++)
			{
				await Task.Delay(1000);

				var denyingUsers = await confirmationMessage.GetReactionUsersAsync(_X, int.MaxValue).FlattenAsync();
				if (denyingUsers.Any(u => u.Id == context.User.Id))
				{
					await RemoveReactionsAndUpdateMessage($"Ты нажал(-а) {_X}. Отменяю операцию.");
					return false;
				}

				var confirmingUsers = await confirmationMessage.GetReactionUsersAsync(_HeavyCheckMark, int.MaxValue).FlattenAsync();
				if (confirmingUsers.Any(u => u.Id == context.User.Id))
				{
					await RemoveReactionsAndUpdateMessage($"Ты нажал(-а) {_HeavyCheckMark}. Выполняю операцию.");
					return true;
				}
			}

			await RemoveReactionsAndUpdateMessage("Не было получено подтверждение операции.");
			return null;

			async Task RemoveReactionsAndUpdateMessage(string bottomMessage)
			{
				await confirmationMessage.RemoveAllReactionsAsync();
				await confirmationMessage.ModifyAsync(m => m.Content = mainMessage + bottomMessage);
			}
		}
	}
}

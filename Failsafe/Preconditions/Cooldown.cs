﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Discord.Commands;

using Failsafe.Properties;

namespace Failsafe.Preconditions
{
	public sealed class Cooldown : PreconditionAttribute
	{
		private TimeSpan CooldownLength { get; set; }
		private readonly ConcurrentDictionary<CooldownInfo, DateTime> _cooldowns = new();

		/// <summary>
		/// Устанавливает время отката этой команды для пользователя.
		/// </summary>
		/// <param name="seconds">Время отката в секундах.</param>
		public Cooldown(int seconds)
		{
			CooldownLength = TimeSpan.FromSeconds(seconds);
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var key = new CooldownInfo(context.User.Id, command.GetHashCode());
			// Проверяет есть ли сообщение с таким хэш кодом в словаре.
			if (_cooldowns.TryGetValue(key, out DateTime endsAt))
			{
				// Высчитывает разницу между текущим временме и временем когда откат закончится.
				var difference = endsAt.Subtract(DateTime.UtcNow);
				// Сообщение если команда все еще в откате.
				if (difference.Ticks > 0)
				{
					return Task.FromResult(PreconditionResult.FromError(string.Format(Resources.CooldownMessage, difference.Seconds)));
				}
				// Обновляет время отката.
				var time = DateTime.UtcNow.Add(CooldownLength);
				_cooldowns.TryUpdate(key, time, endsAt);
			}
			else
			{
				_cooldowns.TryAdd(key, DateTime.UtcNow.Add(CooldownLength));
			}

			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		internal struct CooldownInfo
		{
			public ulong UserId { get; }
			public int CommandHashCode { get; }

			public CooldownInfo(ulong userId, int commandHashCode)
			{
				UserId = userId;
				CommandHashCode = commandHashCode;
			}
		}
	}
}

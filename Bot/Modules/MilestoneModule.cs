using Bot.Services;
using Bot.Services.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

namespace Bot.Modules
{
	class MilestoneModule
	{
		private readonly MilestoneDataService milestoneData;
		private readonly MilestoneHandlerService milestoneHandler;

		public MilestoneModule(IServiceProvider service)
		{
			milestoneData = service.GetRequiredService<MilestoneDataService>();
			milestoneHandler = service.GetRequiredService<MilestoneHandlerService>();
		}

		// TODO: Raid command
		//[Command("рейд")]
		// TODO: Strike command
		//[Command("налёт")]
		// TODO: Other command
		//[Command("активность")]
	}
}

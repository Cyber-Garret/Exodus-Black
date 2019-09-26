using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Neira.Bot.UI.Model
{
	public class Clan : INotifyPropertyChanged
	{
		private string name;
		private DateTimeOffset createDate;
		private string motto;
		private string about;
		private long memberCount;
		private ulong? guildId;

		public long Id { get; set; }
		public string Name
		{
			get => name; set
			{
				name = value;
				OnPropertyChanged(nameof(Name));
			}
		}
		public DateTimeOffset CreateDate
		{
			get => createDate; set
			{
				createDate = value;
				OnPropertyChanged(nameof(CreateDate));
			}
		}
		public string Motto
		{
			get => motto; set
			{
				motto = value;
				OnPropertyChanged(nameof(Motto));
			}
		}
		public string About
		{
			get => about; set
			{
				about = value;
				OnPropertyChanged(nameof(About));
			}
		}
		public long MemberCount
		{
			get => memberCount; set
			{
				memberCount = value;
				OnPropertyChanged(nameof(MemberCount));
			}
		}
		public ulong? GuildId
		{
			get => guildId; set
			{
				guildId = value;
				OnPropertyChanged(nameof(GuildId));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}

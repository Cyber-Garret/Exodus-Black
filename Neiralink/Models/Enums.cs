using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Neiralink.Models
{
	public static class EnumExtensions
	{
		public static string GetEnumDisplayName(this Enum enumType)
		{
			return enumType.GetType().GetMember(enumType.ToString())
						   .First()
						   .GetCustomAttribute<DisplayAttribute>()
						   .Name;
		}
	}

	public enum LangKey
	{
		/// <summary>
		/// English locale
		/// </summary>
		[Display(Name = "English")]
		EN,
		/// <summary>
		/// Russian locale
		/// </summary>
		[Display(Name = "Russian")]
		RU,
		/// <summary>
		/// Ukrainian locale
		/// </summary>
		[Display(Name = "Ukrainian")]
		UK
	}

	public enum MilestoneType : byte
	{
		/// <summary>
		/// Destiny 2 raids
		/// </summary>
		Raid,
		/// <summary>
		/// Destiny 2 nightfalls and orderal: nightfalls
		/// </summary>
		Nightfall,
		/// <summary>
		/// Вungeons, public events, anything from other games
		/// </summary>
		Other
	}

	public enum GameName : byte
	{
		/// <summary>
		/// Destiny 2
		/// </summary>
		[Display(Name = "Destiny 2")]
		Destiny2,
		/// <summary>
		/// The Division 2
		/// </summary>
		[Display(Name = "The Division 2")]
		Division2,
		/// <summary>
		/// CoD: Warzone
		/// </summary>
		[Display(Name = "CoD: Warzone")]
		Warzone
	}
}

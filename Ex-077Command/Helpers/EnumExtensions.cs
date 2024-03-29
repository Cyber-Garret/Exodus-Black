﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Ex077.Helpers
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
}

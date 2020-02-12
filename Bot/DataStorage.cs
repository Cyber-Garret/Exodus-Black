using Newtonsoft.Json;

using System.IO;

namespace Bot
{
	internal class DataStorage
	{
		/// <summary>
		/// Create or read file content by full path and return mapped to class json content
		/// </summary>
		/// <typeparam name="T">Model for restoring</typeparam>
		/// <param name="filePath">Full path to file</param>
		/// <returns></returns>
		internal static T RestoreObject<T>(string filePath)
		{
			var json = GetOrCreateFileContents(filePath);
			return JsonConvert.DeserializeObject<T>(json);
		}

		private static string GetOrCreateFileContents(string filePath)
		{
			if (!File.Exists(filePath))
			{
				File.WriteAllText(filePath, "");
				return "";
			}
			return File.ReadAllText(filePath);
		}
	}
}

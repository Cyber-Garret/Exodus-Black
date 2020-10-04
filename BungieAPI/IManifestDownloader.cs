using System.Threading.Tasks;

namespace BungieAPI
{
	public interface IManifestDownloader
	{
		Task<string> DownloadManifest(string localDatabasePath, string currentVersion);
	}
}
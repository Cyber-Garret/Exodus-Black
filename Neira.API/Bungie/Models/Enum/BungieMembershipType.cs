namespace Neira.API.Bungie
{
	/// <summary>
	/// The types of membership the Accounts system supports. "All" is only valid for searching capabilities: you need to pass the actual matching BungieMembershipType for any query where you pass a known membershipId.
	/// </summary>
	public enum BungieMembershipType
	{
		None = 0,
		TigerXbox = 1,
		TigerPsn = 2,
		TigerSteam = 3,
		TigerBlizzard = 4,
		TigerStadia = 5,
		TigerDemon = 10,
		BungieNext = 254,
		All = -1
	}
}

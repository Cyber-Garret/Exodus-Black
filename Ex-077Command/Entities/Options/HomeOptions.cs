namespace Ex077.Entities.Options;

public class HomeOptions
{
	public const string OptionsName = "HomePage";

	public string DocLink { get; set; } = string.Empty;
	public int ServersCount { get; set; }
	public int UsersCount { get; set; }
	public int MilestonesCount { get; set; }
}

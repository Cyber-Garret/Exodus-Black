namespace BungieAPI.Definitions
{
	public class DestinyStatDefinition : AbstractDefinition
	{
		public DestinyStatAggregationType AggregationType { get; set; }
		public bool HasComputedBlock { get; set; }
		public DestinyStatCategory StatCategory { get; set; }

	}
}
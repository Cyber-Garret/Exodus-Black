namespace Bot
{
	internal static class GlobalVariables
	{
		/// <summary>
		/// Ulong ID for reserving places in milestones
		/// </summary>
		internal const ulong ReservedID = 100500;
		/// <summary>
		/// Array with user time formats for milestones
		/// </summary>
		internal static readonly string[] timeFormats = { "dd.MM-HH:mm", "dd,MM-HH,mm", "dd.MM.HH.mm", "dd,MM,HH,mm" };
	}
}

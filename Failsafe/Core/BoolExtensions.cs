namespace Failsafe.Core
{
	public static class BoolExtensions
	{
		public static bool IsA<T>(this object obj) =>
			obj is T;
	}
}

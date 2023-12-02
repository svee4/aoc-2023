
// Day2.Part1.Solve();
Day2.Part2.Solve();


static class Extensions
{
	public static IEnumerable<T> Log<T>(this IEnumerable<T> @this, Func<T, string>? formatter = null)
	{
		foreach (T element in @this)
		{
			Console.WriteLine(formatter is null ? element : formatter(element));
			yield return element;
		}
	}
}
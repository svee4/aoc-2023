using System.IO.Pipes;
using System.Text;

namespace Day1;

public class Part2
{

	public static void Solve()
	{
		const string INPUT_FILE_PATH = "/home/alex/code/aoc/23/day1/input1.txt";

		long total = 0;
		ReadOnlySpan<string> substrings = ["one", "two", "three", "four", "five", "six", "seven", "eight", "nine"];

		string? line;
		using StreamReader reader = new(File.OpenRead(INPUT_FILE_PATH));

		while (!string.IsNullOrEmpty(line = reader.ReadLine()))
		{
			int num1, num2;

			int firstIndex = line.AsSpan().FirstIndexOf(ch => ch >= '0' && ch <= '9');
			int lastIndex = line.AsSpan().LastIndexOf(ch => ch >= '0' && ch <= '9');

			num1 = int.Parse([line[firstIndex]]);
			num2 = int.Parse([line[lastIndex]]);


			foreach (string substring in substrings)
			{
				int firstStrIndex = line.IndexOf(substring);
				int lastStrIndex = line.LastIndexOf(substring);

				if (firstStrIndex > -1 && firstStrIndex < firstIndex)
				{
					firstIndex = firstStrIndex;
					num1 = substrings.IndexOf(substring) + 1;
				}

				if (lastStrIndex > -1 && lastStrIndex > lastIndex)
				{
					lastIndex = lastStrIndex;
					num2 = substrings.IndexOf(substring) + 1;
				}
			}

			total += int.Parse($"{num1}{num2}");
		}

		Console.WriteLine(total);
	}
}

static class Extensions
{
	public static int FirstIndexOf<T>(this ReadOnlySpan<T> sequence, Predicate<T> predicate)
	{
		for (int i = 0; i < sequence.Length; i++)
		{
			if (predicate(sequence[i])) return i;
		}
		return -1;
	}

	public static int LastIndexOf<T>(this ReadOnlySpan<T> sequence, Predicate<T> predicate)
	{
		for (int i = sequence.Length - 1; i >= 0; i--)
		{
			if (predicate(sequence[i])) return i;
		}
		return -1;
	}
}
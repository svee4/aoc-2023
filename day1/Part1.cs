
namespace Day1;

public class Part1
{
	public static void Solve()
	{
		const string INPUT_FILE_PATH = "/home/alex/code/aoc/23/day1/input1.txt";

		long total = 0;

		string? line;
		using StreamReader reader = new(File.OpenRead(INPUT_FILE_PATH));
		while ((line = reader.ReadLine()) != null)
		{
			char first = line.First(ch => ch >= '0' && ch <= '9');
			char last = line.Last(ch => ch >= '0' && ch <= '9');
			int num = int.Parse([first, last]);
			total += num;
		}

		Console.WriteLine(total);
	}
}

namespace Day2;

class Part2
{
	public static void Solve()
	{

		const string MOCK_INPUT = @"Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green";

		const string INPUT_FILE_PATH = "/home/alex/code/aoc/23/day2/input1.txt";

		long accumulator = 0;

		string[] input = File.ReadAllLines(INPUT_FILE_PATH);
		// string[] input = MOCK_INPUT.Split(Environment.NewLine);

		foreach (string line in input)
		{

			var sets = line
				.Split(':')[1]
				.Split(';')
				.Log(s => $"set: {s}")
				.Select(set =>
					set.Split(',')
						.Select(pull => pull.Trim().Split(' '))
						.Log(arr => string.Join(" | ", arr))
						.Select(arr => new { color = arr[1], count = int.Parse(arr[0]) })
			);

			Dictionary<string, int> maxValuePerColor = new()
			{
				["red"] = 0,
				["green"] = 0,
				["blue"] = 0
			};

			foreach (var set in sets)
			{

				foreach (var pull in set)
				{
					if (maxValuePerColor[pull.color] < pull.count)
					{
						maxValuePerColor[pull.color] = pull.count;
					}
				}
			}

			accumulator += maxValuePerColor.Aggregate(1, (cum, cur) => cum * cur.Value);
		}

		Console.WriteLine(accumulator);
	}
}
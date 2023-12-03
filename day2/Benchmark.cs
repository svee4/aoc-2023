using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Day2;

[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
unsafe public class Benchmark
{

	string? InputData;
	char* MallocInputData;

	[GlobalSetup]
	public void GlobalSetup()
	{
		InputData = File.ReadAllText("/home/alex/code/aoc/23/day2/input1.txt");

		MallocInputData = (char*)Marshal.AllocHGlobal(sizeof(char) * InputData.Length);
		InputData.CopyTo(new Span<char>(MallocInputData, InputData.Length));

		AssertResults();
	}

	void AssertResults()
	{
		long part1 = 2771;
		long part2 = 70924;
		if (Part1Basic() != part1) throw new Exception(nameof(Part1Basic));
		if (Part1Inline() != part1) throw new Exception(nameof(Part1Inline));
		if (Part2Basic() != part2) throw new Exception(nameof(Part2Basic));
		if (Part2Inline() != part2) throw new Exception(nameof(Part2Inline));
		if (Part2Fixed() != part2) throw new Exception(nameof(Part2Fixed));
		if (Part2Malloced() != part2) throw new Exception(nameof(Part2Malloced));
	}


	[Benchmark] public long Part1Basic() => Part1.Solve(InputData.AsSpan());
	[Benchmark] public long Part2Basic() => Part2.Solve(InputData.AsSpan());

	[Benchmark]
	public long Part1Inline()
	{
		const int
			MAX_RED = 12,
			MAX_GREEN = 13,
			MAX_BLUE = 14;

		string input = InputData!;

		long accumulator = 0;

		int index = 0;
		while (true)
		{
			// get game id

			index += 5; //"Game ".Length;
			int gameId = input[index] - '0';
			index++;

			while (input[index] != ':')
			{
				gameId = (10 * gameId) + (input[index] - '0');
				index++;
			}

			index += 2;

			// get the value for a color
			int current;
			bool invalid = false;

		READ_COLOR:
			current = 0;

			while (input[index] != ' ')
			{
				current = (10 * current) + (input[index] - '0');
				index++;
			}

			index++;

			// check if value is out of range for this color
			switch (input[index])
			{
				case 'r':
					{
						if (current > MAX_RED) invalid = true;
						index += 3; //"ed".Length + 1;
						; break;
					}
				case 'g':
					{
						if (current > MAX_GREEN) invalid = true;
						index += 5; // "reen".Length + 1;
						break;
					}
				case 'b':
					{
						if (current > MAX_BLUE) invalid = true;
						index += 4; // "lue".Length + 1;
						break;
					}
			}

			// if one value in game is invalid, the whole game is invalid
			if (invalid) goto NEXT_LINE;

			if (input[index] == ',' || input[index] == ';')
			{
				index += 2; // ", ".Length
				goto READ_COLOR;
			}

			accumulator += gameId;

		NEXT_LINE:
			while (input[index++] != '\n') { }
			if (index >= input.Length) break;
		}


		return accumulator;
	}

	[Benchmark]
	public long Part2Inline()
	{
		// Part2.Solve(InputData.AsSpan());
		string input = InputData!;
		long accumulator = 0;
		int gameIdLength = 1;
		int iteration = 0;
		int index = 0;
		while (true)
		{
			iteration++;
			if (iteration == 10) gameIdLength = 2;
			else if (iteration == 100) gameIdLength = 3;

			// skip game id
			// format: "Game xx: "
			// 5 == "Game ".Length
			// gameId == the game id length in chars
			// 2 == ": ".Length
			index += 5 + gameIdLength + 2;

			// get max values for all colors
			int red = 0, green = 0, blue = 0, current;

		READ_COLOR:
			current = 0;

			while (input[index] != ' ')
			{
				current = (10 * current) + (input[index] - '0');
				index++;
			}

			index++;

			switch (input[index])
			{
				case 'r':
					{
						if (current > red) red = current;
						index += 3; //"ed".Length + 1;
						; break;
					}
				case 'g':
					{
						if (current > green) green = current;
						index += 5; //"reen".Length + 1;
						break;
					}
				case 'b':
					{
						if (current > blue) blue = current;
						index += 4; //"lue".Length + 1;
						break;
					}
			}

			if (input[index] == ',' || input[index] == ';')
			{
				index += 2;
				goto READ_COLOR;
			}

			accumulator += red * green * blue;

			while (input[index++] != '\n') { }
			if (index >= input.Length) break;
		}

		return accumulator;
	}

	[Benchmark]
	public long Part2Fixed()
	{
		long accumulator = 0;
		int gameIdLength = 1;
		int iteration = 0;
		int index = 0;
		int inputLength = InputData!.Length;

		fixed (char* input = InputData!)
		{

			while (true)
			{
				iteration++;
				if (iteration == 10) gameIdLength = 2;
				else if (iteration == 100) gameIdLength = 3;

				// skip game id
				// format: "Game xx: "
				// 5 == "Game ".Length
				// gameId == the game id length in chars
				// 2 == ": ".Length
				index += 5 + gameIdLength + 2;

				// get max values for all colors
				int red = 0, green = 0, blue = 0, current;

			READ_COLOR:
				current = 0;

				while (input[index] != ' ')
				{
					current = (10 * current) + (input[index] - '0');
					index++;
				}

				index++;

				switch (input[index])
				{
					case 'r':
						{
							if (current > red) red = current;
							index += 3; //"ed".Length + 1;
							break;
						}
					case 'g':
						{
							if (current > green) green = current;
							index += 5; //"reen".Length + 1;
							break;
						}
					case 'b':
						{
							if (current > blue) blue = current;
							index += 4; //"lue".Length + 1;
							break;
						}
				}

				if (input[index] == ',' || input[index] == ';')
				{
					index += 2;
					goto READ_COLOR;
				}

				accumulator += red * green * blue;

				while (input[index++] != '\n') { }
				if (index >= inputLength) break;
			}
		}
		return accumulator;
	}

	[Benchmark]
	public long Part2Malloced()
	{
		long accumulator = 0;
		int gameIdLength = 1;
		int iteration = 0;
		int index = 0;
		int inputLength = InputData!.Length;

		char* input = MallocInputData;
		while (true)
		{
			iteration++;
			if (iteration == 10) gameIdLength = 2;
			else if (iteration == 100) gameIdLength = 3;

			// skip game id
			// format: "Game xx: "
			// 5 == "Game ".Length
			// gameId == the game id length in chars
			// 2 == ": ".Length
			index += 5 + gameIdLength + 2;

			// get max values for all colors
			int red = 0, green = 0, blue = 0, current;

		READ_COLOR:
			current = 0;

			while (input[index] != ' ')
			{
				current = (10 * current) + (input[index] - '0');
				index++;
			}

			index++;

			switch (input[index])
			{
				case 'r':
					{
						if (current > red) red = current;
						index += 3; //"ed".Length + 1;
						break;
					}
				case 'g':
					{
						if (current > green) green = current;
						index += 5; //"reen".Length + 1;
						break;
					}
				case 'b':
					{
						if (current > blue) blue = current;
						index += 4; //"lue".Length + 1;
						break;
					}
			}

			if (input[index] == ',' || input[index] == ';')
			{
				index += 2;
				goto READ_COLOR;
			}

			accumulator += red * green * blue;

			while (input[index++] != '\n') { }
			if (index >= inputLength) break;
		}

		return accumulator;
	}
}
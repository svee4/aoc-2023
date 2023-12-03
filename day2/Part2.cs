
namespace Day2;

class Part2
{
	public static long Solve(ReadOnlySpan<char> input)
	{

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
}
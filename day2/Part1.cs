
namespace Day2;

public class Part1
{
	public static long Solve(ReadOnlySpan<char> input)
	{
		const int
			MAX_RED = 12,
			MAX_GREEN = 13,
			MAX_BLUE = 14;

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

}



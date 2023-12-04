
using System.Runtime.Intrinsics;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.Symbol;

#if DEBUG
new Benchmark().GlobalSetup();
#else
BenchmarkDotNet.Running.BenchmarkRunner.Run<Benchmark>();
#endif


[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class Benchmark
{

	string InputData = null!;
	int WinningNumbersCount;
	int MyNumbersCount;

	[GlobalSetup]
	public void GlobalSetup()
	{
		// i added whitespaces after "Card", to make it the same as the real input
		InputData = @"Card   1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
Card   2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19
Card   3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1
Card   4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83
Card   5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36
Card   6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11
";

		InputData = File.ReadAllText("/home/alex/code/aoc/23/day4/input1.txt");
		InputData = InputData.Replace("\r", "");

		string[] splits = InputData.Split(Environment.NewLine)[0].Split(':')[1].Split('|');
		string wins = splits[0];
		string mine = splits[1];

		WinningNumbersCount = new Regex(@"\d+").Count(wins);
		MyNumbersCount = new Regex(@"\d+").Count(mine);
		Console.WriteLine($"Winning numbers count: {WinningNumbersCount}\nMy numbers count: {MyNumbersCount}");

		AssertResults();
	}

	public void AssertResults()
	{
		if (Part1Solution1() != 21568) throw new Exception(nameof(Part1Solution1));
		if (Part1Solution2() != 21568) throw new Exception(nameof(Part1Solution2));
		if (Part1Solution3() != 21568) throw new Exception(nameof(Part1Solution3));
		if (Part1Solution4() != 21568) throw new Exception(nameof(Part1Solution4));
		Part2Solution1();
	}


	[Benchmark]
	public int Part1Solution1()
	{
		int accumulator = 0;

		Span<int> winningNumbers = stackalloc int[WinningNumbersCount];
		Span<int> myNumbers = stackalloc int[MyNumbersCount];

		int iteration = 0;

		int index = 0;
		string input = InputData;
		while (true)
		{
			iteration++;
			int windex = 0,
				mydex = 0,
				current;

			bool readingWinning = true;

			// after "Card", there is either number or filling whitespace
			index += "Card xxx:".Length;


		READ_NUMBER:
			current = 0;
			index++;


			if (input[index] == '|')
			{
				readingWinning = false;
				index += 2;
			}
			if (input[index] == ' ')
			{
				index++;
				current = input[index] - '0';
			}
			else
			{
				current = input[index] - '0';
				index++;
				current = (current * 10) + input[index] - '0';
			}

			if (readingWinning) winningNumbers[windex++] = current;
			else myNumbers[mydex++] = current;
			index++;

			if (input[index] == '\n')
				goto END_CARD;

			goto READ_NUMBER;

		END_CARD:
			// line done
			// for each number in myNumbers, check if winningNumbers contains it. if true, increment counter
			int num = 0;
			for (int i = 0; i < myNumbers.Length; i++)
				for (int j = 0; j < winningNumbers.Length; j++)
					if (winningNumbers[j] == myNumbers[i]) num = Math.Max(num * 2, 1);

			accumulator += num;

			index++;
			if (index == input.Length) break;
		}

		return accumulator;
	}


	[Benchmark]
	public int Part1Solution2()
	{
		int accumulator = 0;

		Span<int> winningNumbers = stackalloc int[WinningNumbersCount];
		Span<int> myNumbers = stackalloc int[MyNumbersCount];

		int index = 0;
		string input = InputData;

		while (true)
		{
			int windex = 0,
				mydex = 0,
				current;

			bool readingWinning = true;

			// after "Card", there is either number or filling whitespace
			index += 9; // "Card xxx:".Length;


		READ_NUMBER:
			current = 0;
			index++;


			if (input[index] == '|')
			{
				readingWinning = false;
				index += 2;
			}

			if (input[index] == ' ')
			{
				index++;
				current = input[index] - '0';
			}
			else
			{
				int temp = (input[index] - '0') * 10;
				index++;
				current = temp + input[index] - '0';
			}

			if (readingWinning) winningNumbers[windex++] = current;
			else myNumbers[mydex++] = current;
			index++;

			if (input[index] == '\n')
				goto END_CARD;

			goto READ_NUMBER;

		END_CARD:
			// line done
			// somehow this is the hardest part
			int num = 0;
			bool oneResult = false;
			for (int i = 0; i < myNumbers.Length; i++)
			{
				for (int j = 0; j < winningNumbers.Length; j++)
				{
					if (winningNumbers[j] == myNumbers[i])
					{
						if (!oneResult)
						{
							oneResult = true;
							num = 1;
						}
						else num *= 2;
					}
				}
			}

			accumulator += num;

			index++;
			if (index == input.Length) break;
		}

		return accumulator;
	}


	[Benchmark]
	public int Part1Solution3()
	{
		int accumulator = 0;

		Span<int> numbers = stackalloc int[WinningNumbersCount + MyNumbersCount];
		ReadOnlySpan<int> winningNumbers = numbers[..WinningNumbersCount];
		ReadOnlySpan<int> myNumbers = numbers.Slice(WinningNumbersCount, MyNumbersCount);

		int index = 0;
		string input = InputData;

		while (true)
		{
			int numbersIndex = 0,
				current;

			// after "Card", there is either number or filling whitespace
			index += 9; // "Card xxx:".Length;

		READ_NUMBER:
			current = 0;
			index++;

			if (input[index] == '|')
			{
				index += 2;
			}

			if (input[index] == ' ')
			{
				index++;
				current = input[index] - '0';
			}
			else
			{
				int temp = (input[index] - '0') * 10;
				index++;
				current = temp + input[index] - '0';
			}

			numbers[numbersIndex++] = current;
			index++;

			if (input[index] == '\n')
				goto END_CARD;

			goto READ_NUMBER;

		END_CARD:
			// line done
			// somehow this is the hardest part
			int num = 0;
			bool oneResult = false;
			for (int i = 0; i < myNumbers.Length; i++)
			{
				for (int j = 0; j < winningNumbers.Length; j++)
				{
					if (winningNumbers[j] == myNumbers[i])
					{
						if (!oneResult)
						{
							oneResult = true;
							num = 1;
						}
						else num *= 2;
					}
				}
			}

			accumulator += num;

			index++;
			if (index == input.Length) break;
		}

		return accumulator;
	}


	[Benchmark]
	public int Part1Solution4()
	{
		int accumulator = 0;

		Span<int> numbers = stackalloc int[WinningNumbersCount + MyNumbersCount];
		ReadOnlySpan<int> winningNumbers = numbers[..WinningNumbersCount];
		ReadOnlySpan<int> myNumbers = numbers.Slice(WinningNumbersCount, MyNumbersCount);

		int index = 0;
		string input = InputData;

		while (true)
		{
			int numbersIndex = 0,
				current;

			// after "Card", there is either number or filling whitespace
			index += 9; // "Card xxx:".Length;

		READ_NUMBER:
			current = 0;
			index++;


			switch (input[index])
			{
				case '|':
					{
						index += 2;
						if (input[index] == ' ')
						{
							index++;
							current = input[index] - '0';
						}
						else
						{
							int temp = (input[index] - '0') * 10;
							index++;
							current = temp + input[index] - '0';
						}
						break;
					}
				case ' ':
					{
						index++;
						current = input[index] - '0';
						break;
					}
				default:
					{
						int temp = (input[index] - '0') * 10;
						index++;
						current = temp + input[index] - '0';
						break;
					}
			}
			/* 
			if (input[index] == '|')
			{
				index += 2;
			}

			if (input[index] == ' ')
			{
				index++;
				current = input[index] - '0';
			}
			else
			{
				int temp = (input[index] - '0') * 10;
				index++;
				current = temp + input[index] - '0';
			} */

			numbers[numbersIndex++] = current;
			index++;

			if (input[index] == '\n')
				goto END_CARD;

			goto READ_NUMBER;

		END_CARD:
			// line done
			// somehow this is the hardest part
			int num = 0;
			bool oneResult = false;
			for (int i = 0; i < myNumbers.Length; i++)
			{
				for (int j = 0; j < winningNumbers.Length; j++)
				{
					if (winningNumbers[j] == myNumbers[i])
					{
						if (!oneResult)
						{
							oneResult = true;
							num = 1;
						}
						else num *= 2;
					}
				}
			}

			accumulator += num;

			index++;
			if (index == input.Length) break;
		}

		return accumulator;
	}


	// [Benchmark]
	public void Part2Solution1() { }

}
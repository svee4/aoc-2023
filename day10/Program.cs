
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Transactions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Characteristics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftAntimalwareEngine;
using static Benchmark;


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

	string Input = null!;

	[GlobalSetup]
	public void GlobalSetup() {
		Input = File.ReadAllText("/home/alex/code/aoc/23/day10/input1.txt");
		//Input = ".....\r\n.S-7.\r\n.|.|.\r\n.L-J.\r\n.....";
		Input = Input.Replace("\r", "");

		//Console.WriteLine(Input);
		//Console.WriteLine("---------");

        AssertResults(); 
	}

	public void AssertResults() 
	{
		Part1Solution1();
	}

    public enum Tiletype
	{
		Unknown = 0,
		Pipe,
		Ground,
		Start
	}

	public enum Pipetype : byte
	{
		Unknown = 0,
		NorthSouth,
		WestEast,
		NorthEast,
		NorthWest,
		SouthWest,
		SouthEast,
		StartingPoint = 255
	}

	// index is same as Pipetype value
	public static readonly char[] PipeChars = ['\0', '|', '-', 'L', 'J', '7', 'F'];

	public abstract class Tile(int x, int y)
	{
		public int X { get; } = x;
		public int Y { get; } = y;

		public char GetChar() => this switch
		{
			Ground => '.',
			Pipe t => t.Type == Pipetype.StartingPoint ? 'S' : PipeChars[(int)t.Type],
			_ => throw new InvalidOperationException()
		};
    }

	public class Ground(int x, int y) : Tile(x, y)
	{
		public override string ToString() => $"Ground({X}, {Y})";
    }

    public class Pipe(int x, int y, Pipetype type) : Tile(x, y)
	{
		public Pipetype Type { get; } = type;
		
		public Pipe? First { get; set;}
		public Pipe? Second { get; set; }

		public char Pipechar => Type == Pipetype.StartingPoint ? 'S' : PipeChars[(int)Type];

		public void ConnectTo(Pipe pipe)
		{
			if (!CanConnectTo(pipe)) throw new InvalidOperationException("Cannot connect incompatible pipes");

			if (First is null) First = pipe;
			else if (Second is null) Second = pipe;
			else throw new InvalidOperationException("Pipe already has both connections");
		}

		public bool CanConnectTo(Pipe pipe)
		{

			bool North() => pipe.AcceptsNorth() && Y < pipe.Y;
            bool South() => pipe.AcceptsSouth() && Y > pipe.Y;
            bool East() =>	pipe.AcceptsEast()	&& X > pipe.X;
            bool West() =>	pipe.AcceptsWest()	&& X < pipe.X;

            return Type switch
            {
                Pipetype.NorthSouth => North() || South(),

                Pipetype.NorthEast => South() || West(),
                Pipetype.NorthWest => South() || East(),

                Pipetype.SouthEast => North() || West(),
                Pipetype.SouthWest => North() || East(),

                Pipetype.WestEast => West() || East(),

                Pipetype.StartingPoint => false,
                _ => throw new InvalidOperationException("Invalid pipe Type"),
            };
		}

        public bool AcceptsNorth() => Type is Pipetype.NorthSouth or Pipetype.NorthEast or Pipetype.NorthWest;
        public bool AcceptsSouth() => Type is Pipetype.NorthSouth or Pipetype.SouthEast or Pipetype.SouthWest;
        public bool AcceptsEast() => Type is Pipetype.NorthEast or Pipetype.SouthEast or Pipetype.WestEast;
        public bool AcceptsWest() => Type is Pipetype.NorthWest or Pipetype.SouthWest or Pipetype.WestEast;

        public override string ToString() => $"Pipe({X}, {Y}, {Type})";

        public static void Connect(Pipe first, Pipe second)
		{
			first.ConnectTo(second);
			second.ConnectTo(first);
		}

    }


    [Benchmark]
	public int Part1Solution1() 
	{
		string[] lines = Input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		char[][] pieces = lines.Select(line => line.ToCharArray()).ToArray();

		Tile[][] map = new Tile[pieces.Length][];

		(int y, int x) size = (pieces.Length, pieces[0].Length);
		Pipe? start = null;

		for (int y = 0; y < size.y; y++)
		{
			map[y] = new Tile[size.x];
            for (int x = 0; x < size.x; x++)
			{
				char c = pieces[y][x];

                Tile t = Helpers.CharToTiletype(c) switch
				{
					Tiletype.Ground => new Ground(x, y),
					Tiletype.Start => new Pipe(x, y, Pipetype.StartingPoint),
					Tiletype.Pipe => new Pipe(x, y, Helpers.CharToPipetype(pieces[y][x])),
					_ => throw new NotSupportedException()
				};

				if (t is Pipe tt && tt.Type == Pipetype.StartingPoint) start = tt;
				map[y][x] = t;
            }
        }

		if (start is null) throw new Exception("Could not find start tile");

        for (int y = 0; y < size.y; y++)
		{
			for (int x = 0; x < size.x; x++)
			{
				Tile tile = map[y][x];

				if (tile is Pipe pipe)
				{
					if (y > 0 && map[y - 1][x]			is Pipe above) 
						if (pipe.CanConnectTo(above)) 
							if (above != pipe.First && above != pipe.Second)
								Pipe.Connect(pipe, above);

                    if (y + 1 < size.y && map[y + 1][x] is Pipe below) 
						if (pipe.CanConnectTo(below)) 
							if (below != pipe.First && below != pipe.Second) 
								Pipe.Connect(pipe, below);

                    if (x > 0 && map[y][x - 1]			is Pipe left ) 
						if (pipe.CanConnectTo(left))  
							if ( left  != pipe.First && left  != pipe.Second)
								Pipe.Connect(pipe, left);

                    if (x + 1 < size.x && map[y][x + 1] is Pipe right) 
						if (pipe.CanConnectTo(right)) 
							if (right != pipe.First && right != pipe.Second) 
								Pipe.Connect(pipe, right);

                }
			}

        }

        // pipes that connect to the starting point
        Pipe? first = null;
        Pipe? second = null;

        Pipe[] candidates = new Pipe?[] {
			start.Y > 0             ? map[start.Y - 1][start.X] as Pipe : null, // above
            start.Y + 1 < size.y    ? map[start.Y + 1][start.X] as Pipe : null, // below
            start.X > 0             ? map[start.Y][start.X - 1] as Pipe : null, // left
            start.X + 1 < size.x    ? map[start.Y][start.X + 1] as Pipe : null  // right
        }.Where(c => c is not null).Select(c => c!).ToArray();

		if (candidates.Length == 1) throw new Exception("Found less than 2 pipes near start point");

        foreach (char c in PipeChars.Skip(1))
		{
			Pipe test = new(start.X, start.Y, Helpers.CharToPipetype(c));

            foreach (Pipe candidate in candidates)
			{
				if (test.CanConnectTo(candidate))
				{
					if (first is null) first = candidate;
					else if (second is null) second = candidate;
					else throw new InvalidOperationException("Found more than 2 possible connections for starting point");
				}
			}

			if (first is not null && second is not null) break;
		}

		if (first is null || second is null) throw new Exception("Could not find 2 connections for starting point");


		using IEnumerator<Pipe> firstEnumerator = Helpers.NonCircularPipeEnumerator(first).GetEnumerator();
		using IEnumerator<Pipe> secondEnumerator = Helpers.NonCircularPipeEnumerator(second).GetEnumerator();

		int stepcount = 0;

        while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext())
		{
			stepcount++;
            if (firstEnumerator.Current == secondEnumerator.Current) break;
        }

		return stepcount;
    }

	//[Benchmark]
	public void Part2Solution1() { }

}

static class Helpers
{

	public static Tiletype CharToTiletype(char c) => c switch
	{
		'.' => Tiletype.Ground,
		'S' => Tiletype.Start,
		_ when PipeChars.Contains(c) => Tiletype.Pipe,
		_ => throw new InvalidOperationException()
	};


	public static Pipetype CharToPipetype(char c)
	{
		int index = Array.IndexOf(PipeChars, c);
        if (index == -1) throw new InvalidOperationException();
		return (Pipetype)(index);
	}

	public static IEnumerable<Pipe> NonCircularPipeEnumerator(Pipe start)
	{
        Pipe? current = start;
		Pipe? prev = null;

        while (true)
        {
            yield return current;

            Pipe? next = current.First;

			if (next is null) break; // only one connection, which means last node
            if (next == prev) next = current.Second;
			if (next is null) break; // again, only one connection

            prev = current;
            current = next;

            if (current == start) break;
        }
    }


    public static IEnumerable<IEnumerable<T>> Enumerate<T>(this T[][] @this)
	{
		for (int i = 0; i < @this.Length; i++)
		{
			yield return @this[i];
		}
	}

}

using BenchmarkDotNet.Attributes;
using static Helpers;


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

	public static bool IsPart2 { get; private set; } = false;

	string Input = null!;

	[GlobalSetup]
	public void GlobalSetup()
	{
		Input = @"32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483
";
		Input = File.ReadAllText("/home/alex/code/aoc/23/day7/input1.txt");
		AssertResults();
	}

	public void AssertResults()
	{
		Part1Solution1();
		Part2Solution1();
	}


	public static readonly char[] Part1Labels = ['2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'];
	public static readonly char[] Part2Labels = ['J', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'Q', 'K', 'A'];


	[Benchmark]
	public void Part1Solution1()
	{
		IsPart2 = false;
		if (Solve() != 251216224) throw new Exception();
		// Solve();
		// Console.WriteLine(Solve());
	}

	[Benchmark]
	public void Part2Solution1()
	{
		IsPart2 = true;
		if (Solve() != 250825971) throw new Exception();
		// Solve();
		// Console.WriteLine(Solve());
	}

	long Solve()
	{
		string input = Input;
		var hands = Input
					.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
					.Select(Hand.FromString)
					.OrderBy(Self, new Hand.Comparer());

		long accumulator = 0;
		int rank = 1;
		foreach (Hand hand in hands)
		{
			// Console.WriteLine($"{rank,4}: {hand}");
			accumulator += rank * hand.Bid;
			rank++;
		}

		return accumulator;
	}

}


record struct Card(char Label) : IComparable<Card>
{
	public readonly int CompareTo(Card other) =>
		CompareCards(this, other, Benchmark.IsPart2 ? Benchmark.Part2Labels : Benchmark.Part1Labels, false);

	public readonly bool IsJoker() => Label == 'J';
	public override readonly string ToString() => Label.ToString();
}




enum Strength
{
	High,
	OnePair,
	TwoPair,
	ThreeKind,
	FullHouse,
	FourKind,
	FiveKind
}


record Hand(Card First, Card Second, Card Third, Card Fourth, Card Fifth, int Bid)
{


	public Card[] Cards { get; } = [First, Second, Third, Fourth, Fifth];


	public Strength GetStrength(bool useJoker)
	{
		Card[] cards = [.. Cards];

		if (useJoker && cards.Any(c => c.IsJoker()) && cards.Any(c => !c.IsJoker()))
		{
			// get most common card and replace joker with it
			Card mostCommon = cards.Where(card => !card.IsJoker()).GroupBy(c => c).MaxBy(g => g.Count())!.Key;
			for (int i = 0; i < cards.Length; i++)
				if (cards[i].IsJoker()) cards[i] = mostCommon;
		}

		// cards = [.. cards.OrderBy(card => Benchmark.Part1Labels.IndexOf(card.Label))];
		// cards are ordered by occurrence count
		cards = [.. cards.GroupBy(Self).OrderByDescending(g => g.Count()).SelectMany(g => g.AsEnumerable())];
		int distincts = cards.Distinct().Count();

		if (distincts == 1) return Strength.FiveKind;
		if (distincts == 2)
		{
			return cards.Where(c => c == cards[0]).Count() == 4 ? Strength.FourKind : Strength.FullHouse;
			// Card first = cards[0];
			// Card second = cards.First(c => c != first);
			// int count = cards.Where(c => c == first).Count();
			// if (count == 4 || count == 1) return Strength.FourKind;
			// return Strength.FullHouse;
		}

		if (distincts == 3)
		{
			// AAABC
			// AABBC

			return cards.GroupBy(Self).Select(g => g.Count()).Any(count => count == 3) ? Strength.ThreeKind : Strength.TwoPair;

			// Dictionary<Card, int> counts = cards.Distinct().ToDictionary(c => c, _ => 0);
			// foreach (Card c in cards)
			// 	counts[c]++;

			// return counts.Any(kvp => kvp.Value == 3) ? Strength.ThreeKind : Strength.TwoPair;
		}

		if (distincts == 4)
		{
			// AABCD
			return Strength.OnePair;
		}

		return Strength.High;
	}


	public static Hand FromString(string fullstring) =>
		fullstring[..5].With(labels => new Hand(
			new(labels[0]),
			new(labels[1]),
			new(labels[2]),
			new(labels[3]),
			new(labels[4]),
			int.Parse(fullstring[6..])
		));


	public override string ToString()
	{

		var ss = Cards.OrderByDescending(c => c);
		return
			$"{string.Join(string.Empty, Cards)} " +
			$"(Cards: {string.Join(string.Empty, ss)}, Strength: {GetStrength(Benchmark.IsPart2)}, Bid: {Bid,4})";

	}


	public int CompareTo(Hand other, bool useJoker)
	{
		int result = GetStrength(useJoker).CompareTo(other.GetStrength(useJoker));
		if (result != 0) return result;

		foreach ((Card myCard, Card compareCard) in Cards.Zip(other.Cards))
		{
			int result2 = CompareCards(myCard, compareCard, useJoker ? Benchmark.Part2Labels : Benchmark.Part1Labels, useJoker: false);
			if (result2 != 0) return result2;
		}

		return 0;
	}


	public class Comparer : IComparer<Hand>
	{
		int IComparer<Hand>.Compare(Hand? x, Hand? y)
		{
			ArgumentNullException.ThrowIfNull(x);
			ArgumentNullException.ThrowIfNull(y);
			return x.CompareTo(y, Benchmark.IsPart2);
		}
	}
}

static class Helpers
{

	public static bool CardsAreEqual(Card first, Card second, bool useJoker) =>
		!useJoker
			? first == second
			: first == second || first.IsJoker() || second.IsJoker();


	public static int CompareCards(Card first, Card second, char[] order, bool useJoker) =>
		CardsAreEqual(first, second, useJoker)
			? 0
			: order.IndexOf(first.Label).CompareTo(order.IndexOf(second.Label));


	public static int IndexOf<T>(this T[] @this, T value) where T : IComparable<T>
	{
		for (int i = 0; i < @this.Length; i++)
			if (@this[i].CompareTo(value) == 0) return i;
		return -1;
	}

	public static T Self<T>(T self) => self;

	public static E With<T, E>(this T @this, Func<T, E> func) => func(@this);
}

class ShouldBeUnreachableException : Exception { }
#define PART2

using System.Diagnostics;
using BenchmarkDotNet.Attributes;


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
	}


	[Benchmark]
	public void Part1Solution1()
	{
		string input = Input;
		List<Hand> hands = [];
		foreach (string line in Input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
		{
			string cards = line[..5].Trim();
			string bidstr = line[6..].Trim();
			int bid = int.Parse(bidstr);
			Hand hand = Hand.FromString(cards, bid);
			hands.Add(hand);
		}

		var ordered = hands.OrderBy(hand => hand);

		long accumulator = 0;
		int rank = 1;
		foreach (var hand in ordered)
		{
			if (hand.Cards.Any(card => card == Card.J)) Console.WriteLine($"{rank,4}: {hand}");
			accumulator += rank * hand.Bid;
			rank++;
		}

		Console.WriteLine(accumulator);
	}

	[Benchmark]
	public void Part2Solution1() { }

}

enum Card
{
	Unknown = 0,
#if PART2
	J, // PART 2
#endif
	Two,
	Three,
	Four,
	Five,
	Six,
	Seven,
	Eight,
	Nine,
	T,
#if !PART2
	J, // PART 1
#endif
	Q,
	K,
	A
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


class Hand(Card first, Card second, Card third, Card fourth, Card fifth, int bid) : IComparable<Hand>
{

	public Card First { get; } = first;
	public Card Second { get; } = second;
	public Card Third { get; } = third;
	public Card Fourth { get; } = fourth;
	public Card Fifth { get; } = fifth;
	public int Bid { get; } = bid;

	public Card[] Cards { get; } = [first, second, third, fourth, fifth];


	public Strength GetStrength()
	{

#if PART2
		Card firstThatsNotJoker = Cards.FirstOrDefault(card => card != Card.J);
		if (firstThatsNotJoker == Card.Unknown) return Strength.FiveKind; // five jokers
		if (Cards.All(c => Helpers.CompareCard(firstThatsNotJoker, c))) return Strength.FiveKind;
#else
		if (Cards.Skip(1).All(c => Helpers.CompareCard(First, c))) return Strength.FiveKind;
#endif


		// get unique labels
		List<Card> labels = [];

		bool hasJoker = Cards.Any(card => card == Card.J);
		// Card firstThatsNotJ = Cards.FirstOrDefault(card => card != Card.J);
		// if (firstThatsNotJ == Card.Unknown) return Strength.FiveKind; // all jokers

		foreach (Card card in Cards)
			if (!labels.Any(label => card == label)) labels.Add(card);

		// if (hasJoker) labels.Add(Card.J);

		int totalDiffererentLabels = labels.Count;

		if (totalDiffererentLabels == 2)
		{
			// labelCounts include joker matches
			int firstLabelCount = Cards.Where(card => Helpers.CompareCard(card, labels[0])).Count();
			int secondLabelCount = Cards.Where(card => Helpers.CompareCard(card, labels[1])).Count();

			// any variation of AAAAJ - AJJJJ, this should already be matched by the first check
			// if (firstLabelCount == 5 && secondLabelCount == 5)
			// 	return Strength.FiveKind;

			// these will never match because totalDifferentLabels includes joker
			// one joker - AABBJ
			// if (firstLabelCount == 3 && secondLabelCount == 3)
			// 	return Strength.FullHouse;

			// // one joker - AAABJ
			// if (firstLabelCount == 4 && secondLabelCount == 2 || firstLabelCount == 2 && secondLabelCount == 4)
			// 	return Strength.FourKind;

			// // two jokers - AABJJ
			// if (firstLabelCount == 4 && secondLabelCount == 3 || firstLabelCount == 3 && secondLabelCount == 4)
			// 	return Strength.FourKind;

			// // three jokers - ABJJJ
			// if (firstLabelCount == 4 && secondLabelCount == 4)
			// 	return Strength.FourKind;

			// no jokers - AAAAB
			if (firstLabelCount == 1 || secondLabelCount == 1)
				return Strength.FourKind;

			// no jokers - AAABB
			if (firstLabelCount == 3 && secondLabelCount == 2 || firstLabelCount == 2 && secondLabelCount == 3)
				return Strength.FullHouse;

			throw new UnreachableException();
		}

		if (totalDiffererentLabels == 3)
		{
			// AABBC
			// AAABC
			// AABBJ
			// AAABJ
			// AABJJ

			if (labels.Any(label => Cards.Where(card => Helpers.CompareCard(card, label)).Count() == 4))
			{
				// AAABJ
				// AABJJ
				return Strength.FourKind;
			}

			if (labels.Any(label => Cards.Where(card => Helpers.CompareCard(card, label)).Count() == 3))
			{
				// AABBJ
				// AAABC
				return Strength.ThreeKind;
			}

			return Strength.TwoPair;
		}

		if (totalDiffererentLabels == 4)
		{
			// AABCD
			// AABCJ
			if (hasJoker) return Strength.ThreeKind;
			return Strength.OnePair;
		}

		// 5 different labels
		// ABCDJ
		if (hasJoker) return Strength.OnePair;
		// ABCDE
		return Strength.High;
	}


	public static Hand FromString(string cardString, int bid)
	{
		if (cardString.Length != 5) throw new ArgumentException("argument length property must be 5", nameof(cardString));

		Span<Card> cards = stackalloc Card[5];
		for (int i = 0; i < 5; i++)
		{
			cards[i] = cardString[i] switch
			{
				'2' => Card.Two,
				'3' => Card.Three,
				'4' => Card.Four,
				'5' => Card.Five,
				'6' => Card.Six,
				'7' => Card.Seven,
				'8' => Card.Eight,
				'9' => Card.Nine,
				'T' => Card.T,
				'J' => Card.J,
				'Q' => Card.Q,
				'K' => Card.K,
				'A' => Card.A,
				_ => throw new InvalidOperationException("cardstring was invalid")
			};
		}

		return new Hand(
			cards[0],
			cards[1],
			cards[2],
			cards[3],
			cards[4],
			bid
		);
	}


	public override string ToString()
	{
		var s = Helpers.CardToString;
		return
			$"{string.Join(string.Empty, Cards.Select(s))} " +
			$"(Cards: {string.Join(string.Empty, Cards.OrderByDescending(c => c).Select(s))}, Strength: {GetStrength()}, Bid: {Bid,4})";
	}

	public int CompareTo(Hand? other)
	{
		ArgumentNullException.ThrowIfNull(other);

		Strength myStrength = GetStrength();
		Strength otherStrength = other.GetStrength();

		int result = myStrength.CompareTo(otherStrength);
		if (result != 0) return result;

		ReadOnlySpan<(Card, Card)> compareCards = [
			(First, other.First),
				(Second, other.Second),
				(Third, other.Third),
				(Fourth, other.Fourth),
				(Fifth, other.Fifth)
		];

		foreach ((Card myCard, Card compareCard) in compareCards)
		{
			if (myCard > compareCard) return 1;
			else if (myCard < compareCard) return -1;
		}

		return 0;
	}
}

static class Helpers
{

	public static char CardToString(Card card) => card switch
	{
		Card.Two => '2',
		Card.Three => '3',
		Card.Four => '4',
		Card.Five => '5',
		Card.Six => '6',
		Card.Seven => '7',
		Card.Eight => '8',
		Card.Nine => '9',
		_ => card.ToString()[0]
	};


	public static bool CompareCard(Card first, Card second)
	{
		// compare cards while taking into account part 2's joker card that can be equal to any card
#if PART1
		return first == second;
#else
		return first == second || first == Card.J || second == Card.J;
#endif
	}

}
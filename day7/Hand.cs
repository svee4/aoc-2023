using System.Diagnostics;
using BenchmarkDotNet.Characteristics;
using static Day7.Helpers;


namespace Day7;


record Hand(Card First, Card Second, Card Third, Card Fourth, Card Fifth, int Bid, bool IsPart2)
{

	public Card[] Cards { get; } = [First, Second, Third, Fourth, Fifth];


	public Strength GetStrength(bool useJoker) =>
		(useJoker
		? Cards
			.Where(card => !card.IsJoker)
			.GroupBy(Self)
			.MaxBy(Enumerable.Count)
			?.Key.Pipe(mostCommon => Cards.Select(card => card.IsJoker ? mostCommon : card)) 
			?? Cards
		: Cards)
			.GroupBy(Self)
			.Select(Enumerable.Count)
			.OrderByDescending(Self)
			.ToList() 
			switch
		{
			[5] => Strength.FiveKind,
			[4, 1] => Strength.FourKind,
			[3, 2] => Strength.FullHouse,
			[3, 1, 1] => Strength.ThreeKind,
			[2, 2, 1] => Strength.TwoPair,
			[2, 1, 1, 1] => Strength.OnePair,
			[1, 1, 1, 1, 1] => Strength.High,
			_ => throw new UnreachableException()
		};


	public static Hand FromString(string line, bool isPart2) =>
        line[..5].Pipe(labels => new Hand(
			new(labels[0], isPart2),
			new(labels[1], isPart2),
			new(labels[2], isPart2),
			new(labels[3], isPart2),
			new(labels[4], isPart2),
			int.Parse(line[6..]),
			isPart2
		));


	public override string ToString() =>
		$"{string.Join(string.Empty, Cards)} " +
		$"(Labels: {string.Join(string.Empty, Cards.OrderByDescending(c => c))}, Strength: {GetStrength(IsPart2)}, Bid: {Bid,4})";


	public int CompareTo(Hand other, char[] order, bool useJoker) =>
		this.GetStrength(useJoker).CompareTo(other.GetStrength(useJoker)) switch
		{
			int x when x != 0 => x,
			_ => Cards.Zip(other.Cards)
					.Select(cards => CompareCards(cards.First, cards.Second, order, useJoker: false))
					.First(result => result != 0)
		};


	public class Comparer(char[] order, bool isPart2) : IComparer<Hand>
	{
		int IComparer<Hand>.Compare(Hand? x, Hand? y)
		{
			ArgumentNullException.ThrowIfNull(x);
			ArgumentNullException.ThrowIfNull(y);
			return x.CompareTo(y, order, isPart2);
		}
	}
}

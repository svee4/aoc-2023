using static Day7.Helpers;


namespace Day7;


record struct Card(char Label, bool IsPart2) : IComparable<Card>
{

	public readonly int CompareTo(Card other) =>
		CompareCards(this, other, IsPart2 ? Part2Labels : Part1Labels, false);


	public readonly bool IsJoker => Label == 'J';

	public override readonly string ToString() => Label.ToString();
}

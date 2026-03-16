using GreClient.CardData;

namespace Wotc.Mtga.Cards.Text;

internal interface ITableTextEntry : ICardTextEntry
{
	string Preamble { get; }

	CardUtilities.RollTableEntry[] Rows { get; }
}

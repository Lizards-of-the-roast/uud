using System.Collections.Generic;
using System.Linq;
using Wizards.Unification.Models.Cards;
using Wotc.Mtga.Cards.Database;

namespace Core.Shared.Code.Providers;

public class CardNicknameProvider : ICardNicknamesProvider
{
	private readonly Dictionary<string, HashSet<uint>> _nicknames = new Dictionary<string, HashSet<uint>>();

	public static CardNicknameProvider Create()
	{
		return new CardNicknameProvider();
	}

	public void SetData(List<CardNickname> nicknames)
	{
		_nicknames.Clear();
		foreach (CardNickname nickname in nicknames)
		{
			string key = LocalizationManagerUtils.NormalizeLocalizedText(nickname.Nickname).ToUpperInvariant();
			_nicknames[key] = nickname.TitleIds;
		}
	}

	public IEnumerable<uint> GetTitleIdsForNickname(string nickname)
	{
		if (!_nicknames.TryGetValue(LocalizationManagerUtils.NormalizeLocalizedText(nickname).ToUpperInvariant(), out var value))
		{
			return Enumerable.Empty<uint>();
		}
		return value;
	}
}

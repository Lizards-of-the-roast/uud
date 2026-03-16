using System.Collections.Generic;
using Wizards.Unification.Models.Cards;

namespace Core.Shared.Code.Providers;

public interface ICardNicknamesProvider
{
	void SetData(List<CardNickname> cardNicknames);

	IEnumerable<uint> GetTitleIdsForNickname(string nickname);
}

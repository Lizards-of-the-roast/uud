using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Models.Network;
using Wotc.Mtga.Providers;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class TitlesDataProvider : ITitlesDataProvider
{
	private readonly CosmeticsProvider _cosmeticsProvider;

	private List<string> _allTitles;

	public TitlesDataProvider(CosmeticsProvider cosmeticsProvider)
	{
		_cosmeticsProvider = cosmeticsProvider;
	}

	public IReadOnlyList<string> GetAllTitles()
	{
		return _allTitles ?? (_allTitles = new List<string>(LoadAllTitles(_cosmeticsProvider)));
	}

	private static IReadOnlyList<string> LoadAllTitles(CosmeticsProvider cosmeticsProvider)
	{
		return (from dto in cosmeticsProvider.AvailableTitles
			select dto.Id into id
			orderby id
			select id).Prepend("NoTitle").ToList();
	}
}

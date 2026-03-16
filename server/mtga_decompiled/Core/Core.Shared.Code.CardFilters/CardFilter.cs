using System;
using System.Collections.Generic;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wotc.Mtga.Cards.Database;

namespace Core.Shared.Code.CardFilters;

public class CardFilter : IReadOnlyCardFilter
{
	private List<CardFilterType> _filters = new List<CardFilterType>();

	private string _searchText = string.Empty;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IGreLocProvider _locManager;

	private readonly SetMetadataProvider _setMetadataProvider;

	public IReadOnlyList<CardFilterType> Filters => _filters;

	public string SearchText
	{
		get
		{
			return _searchText;
		}
		set
		{
			_searchText = value ?? string.Empty;
		}
	}

	public CardFilter(ICardDatabaseAdapter cardDatabase, IGreLocProvider locManager, ISetMetadataProvider setMetadataProvider)
	{
		_cardDatabase = cardDatabase;
		_locManager = locManager;
		_setMetadataProvider = setMetadataProvider as SetMetadataProvider;
	}

	public bool IsSet(CardFilterType filter)
	{
		return _filters.Contains(filter);
	}

	public void Set(CardFilterType filter, bool value = true)
	{
		if (value)
		{
			if (!_filters.Contains(filter))
			{
				_filters.Add(filter);
			}
		}
		else
		{
			_filters.Remove(filter);
		}
	}

	public List<Func<CardFilterGroup, CardFilterGroup>> GetFilterFunctions(CardMatcher.CardMatcherMetadata metadata = null)
	{
		List<Func<CardFilterGroup, CardFilterGroup>> list = new List<Func<CardFilterGroup, CardFilterGroup>>();
		CardMatcher searcher = new CardMatcher(_searchText, _cardDatabase);
		if (searcher.Successful)
		{
			searcher.SetMetadata(metadata);
			list.Add((CardFilterGroup c) => searcher.Matches(c));
		}
		bool flag = _filters.Contains(CardFilterType.Multicolor);
		AndReqs and = new AndReqs();
		foreach (List<FilterElement> group in _setMetadataProvider.Groups)
		{
			OrReqs orReqs = new OrReqs();
			foreach (FilterElement item in group)
			{
				if (_filters.Contains(item.Filter))
				{
					if (flag && group == _setMetadataProvider.ColorGroup)
					{
						OrReqs orReqs2 = new OrReqs();
						orReqs2.Reqs.AddRange(item.Reqs);
						and.Reqs.Add(orReqs2);
					}
					else
					{
						orReqs.Reqs.AddRange(item.Reqs);
					}
				}
			}
			if (orReqs.Reqs.Count > 0)
			{
				and.Reqs.Add(orReqs);
			}
		}
		if (and.Reqs.Count > 0)
		{
			list.Add((CardFilterGroup c) => and.Evaluate(c, metadata));
		}
		return list;
	}

	public CardFilter Copy()
	{
		return new CardFilter(_cardDatabase, _locManager, _setMetadataProvider)
		{
			_searchText = _searchText,
			_filters = new List<CardFilterType>(_filters)
		};
	}
}

using System.Collections.Generic;

namespace Core.Shared.Code.CardFilters;

public interface IReadOnlyCardFilter
{
	IReadOnlyList<CardFilterType> Filters { get; }

	string SearchText { get; }

	bool IsSet(CardFilterType filter);

	CardFilter Copy();
}

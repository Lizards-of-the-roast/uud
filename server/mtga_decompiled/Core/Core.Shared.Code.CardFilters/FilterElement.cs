namespace Core.Shared.Code.CardFilters;

public class FilterElement
{
	public CardFilterType Filter { get; private set; }

	public ReqTerm[] Reqs { get; private set; }

	public FilterElement(CardFilterType filter, params ReqTerm[] reqs)
	{
		Filter = filter;
		Reqs = reqs;
	}
}

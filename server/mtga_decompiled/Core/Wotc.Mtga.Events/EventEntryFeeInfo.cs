using System;
using Wizards.MDN;

namespace Wotc.Mtga.Events;

[Serializable]
public class EventEntryFeeInfo
{
	public EventEntryCurrencyType CurrencyType = EventEntryCurrencyType.None;

	public bool UsesRemaining = true;

	public int Quantity;

	public string ReferenceId;

	public EventEntryFeeInfo(EventEntryCurrencyType currencyType, bool usesRemaining, int quantity, string referenceId)
	{
		CurrencyType = currencyType;
		UsesRemaining = usesRemaining;
		Quantity = quantity;
		ReferenceId = referenceId;
	}

	public EventEntryFeeInfo()
	{
		CurrencyType = EventEntryCurrencyType.None;
		UsesRemaining = true;
	}
}

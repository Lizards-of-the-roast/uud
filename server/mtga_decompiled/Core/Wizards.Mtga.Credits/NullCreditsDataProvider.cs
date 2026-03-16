using System;
using System.Collections.Generic;

namespace Wizards.Mtga.Credits;

public class NullCreditsDataProvider : ICreditsDataProvider
{
	public static readonly ICreditsDataProvider Default = new NullCreditsDataProvider();

	public IReadOnlyCollection<CreditSectionData> GetCredits()
	{
		return (IReadOnlyCollection<CreditSectionData>)(object)Array.Empty<CreditSectionData>();
	}
}

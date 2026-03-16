using System.Collections.Generic;

namespace Wizards.Mtga.Credits;

public interface ICreditsDataProvider
{
	IReadOnlyCollection<CreditSectionData> GetCredits();
}

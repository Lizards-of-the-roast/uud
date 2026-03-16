using System.Collections.Generic;
using Wizards.Arena.Promises;

namespace Wizards.Mtga.PreferredPrinting;

public interface IPreferredPrintingDataProvider
{
	IReadOnlyDictionary<int, PreferredPrintingWithStyle> GetAllPreferredPrintings();

	PreferredPrintingWithStyle GetPreferredPrintingForTitleId(int titleId);

	Promise<bool> SetPreferredPrintingForTitleId(int titleId, int grpId, string styleCode);

	Promise<bool> RemovePreferredPrintingForTitleId(int titleId);

	Promise<Dictionary<int, PreferredPrintingWithStyle>> ForceRefreshPreferredPrintings();
}

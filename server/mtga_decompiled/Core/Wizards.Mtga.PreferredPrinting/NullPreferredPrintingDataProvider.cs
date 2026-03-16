using System;
using System.Collections.Generic;
using Wizards.Arena.Promises;

namespace Wizards.Mtga.PreferredPrinting;

public class NullPreferredPrintingDataProvider : IPreferredPrintingDataProvider
{
	public static IPreferredPrintingDataProvider Create()
	{
		return new NullPreferredPrintingDataProvider();
	}

	public IReadOnlyDictionary<int, PreferredPrintingWithStyle> GetAllPreferredPrintings()
	{
		throw new NotImplementedException();
	}

	public PreferredPrintingWithStyle GetPreferredPrintingForTitleId(int titleId)
	{
		throw new NotImplementedException();
	}

	public Promise<bool> SetPreferredPrintingForTitleId(int titleId, int grpId, string styleCode)
	{
		throw new NotImplementedException();
	}

	public Promise<bool> RemovePreferredPrintingForTitleId(int titleId)
	{
		throw new NotImplementedException();
	}

	public Promise<Dictionary<int, PreferredPrintingWithStyle>> ForceRefreshPreferredPrintings()
	{
		throw new NotImplementedException();
	}
}

using System.Collections.Generic;
using Wizards.Arena.Models;
using Wizards.Arena.Promises;

namespace Wotc.Mtga.Network.ServiceWrappers;

public interface IPreferredPrintingServiceWrapper
{
	Promise<Dictionary<int, DTO_PreferredPrintingWithStyle>> GetAllPreferredPrintings();

	Promise<bool> SetPreferredPrinting(int titleId, int grpId, string styleCode);

	Promise<bool> RemovePreferredPrinting(int titleId);
}

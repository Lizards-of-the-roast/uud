using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.PlayBlade;

namespace Wotc.Mtga.Network.ServiceWrappers;

public interface IPlayBladeConfigServiceWrapper
{
	Promise<List<PlayBladeQueueEntry>> GetPlayBladeConfig();
}

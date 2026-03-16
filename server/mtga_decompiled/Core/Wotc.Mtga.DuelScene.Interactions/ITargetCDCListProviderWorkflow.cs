using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface ITargetCDCListProviderWorkflow
{
	List<DuelScene_CDC> GetTargetCDCs();
}

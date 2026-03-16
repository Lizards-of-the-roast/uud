using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Resolution;

public class Data : IPayload
{
	public bool IgnoreDamageEvents;

	public bool IgnoreDestructionEvents;

	public bool IgnoreCoinFlipEvents;

	public bool BlocksWorkflows;

	public bool BlocksEvents = true;

	public bool RedirectDamageFromParent;

	public bool SuppressProjectileDamageEffects;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}

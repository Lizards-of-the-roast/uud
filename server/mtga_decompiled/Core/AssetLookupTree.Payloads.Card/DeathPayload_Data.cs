using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class DeathPayload_Data : IPayload
{
	public float DissolveDelay;

	public float DissolveSpeed;

	public float DissolveWait;

	public float HideEffectsTime = 1f;

	public bool BypassMovementWait;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}

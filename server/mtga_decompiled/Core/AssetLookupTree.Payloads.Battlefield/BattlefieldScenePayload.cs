using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Battlefield;

public class BattlefieldScenePayload : IPayload
{
	public string SceneName;

	public string BattlefieldID;

	public bool InRandomPool;

	public string ScenePath;

	public string AudioBankName;

	public IEnumerable<string> GetFilePaths()
	{
		yield return ScenePath;
	}
}

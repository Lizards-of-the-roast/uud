using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Scenes;

public class ScenePayloads : IPayload
{
	public string SceneName;

	public string ScenePath;

	public IEnumerable<string> GetFilePaths()
	{
		yield return ScenePath;
	}
}

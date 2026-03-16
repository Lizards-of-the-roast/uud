using System.Collections.Generic;
using AssetLookupTree.Payloads.Ability.Metadata;

namespace AssetLookupTree.Payloads.Ability;

public class ParameterizedHangerEntry : IPayload
{
	public readonly ParameterizedHangerEntryData Data = new ParameterizedHangerEntryData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return Data.SpriteRef.RelativePath;
	}
}

using System.Collections.Generic;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class ManaPaymentButtonTextOverride : IPayload
{
	public class ParamNameContentPair
	{
		public string Name { get; set; }

		public string Contents { get; set; }

		public ParamNameContentPair(string name, string contents)
		{
			Name = name;
			Contents = contents;
		}
	}

	public string Key = string.Empty;

	public List<ParamNameContentPair> ParameterNameContentPairs = new List<ParamNameContentPair>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}

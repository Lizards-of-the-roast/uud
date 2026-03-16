using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Browser.OrderWorkflow;

public class BrowserOverrides : IPayload
{
	public string LeftOrderIndicatorLocKey;

	public string RightOrderIndicatorLocKey;

	public bool ReverseIdsOnSubmit;

	public bool ReverseDisplayOrder;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}

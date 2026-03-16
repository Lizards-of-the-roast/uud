using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads;

public abstract class KeyCaseLocParameterProvider : ILocParameterProvider
{
	public enum KeyCasing
	{
		NormalCase,
		LowerCase,
		UpperCase
	}

	public KeyCasing KeyCase { get; set; }

	public string GetKey()
	{
		return KeyCase switch
		{
			KeyCasing.LowerCase => GetParamKey().ToLower(), 
			KeyCasing.UpperCase => GetParamKey().ToUpper(), 
			_ => GetParamKey(), 
		};
	}

	protected abstract string GetParamKey();

	public abstract bool TryGetValue(IBlackboard filledBB, out string paramValue);
}

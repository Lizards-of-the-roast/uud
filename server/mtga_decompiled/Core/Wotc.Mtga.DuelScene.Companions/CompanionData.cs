namespace Wotc.Mtga.DuelScene.Companions;

public readonly struct CompanionData
{
	public readonly string Id;

	public readonly string Variant;

	public readonly GREPlayerNum OwnerType;

	public bool IsEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(Id))
			{
				return string.IsNullOrEmpty(Variant);
			}
			return false;
		}
	}

	public CompanionData(string id, string variant, GREPlayerNum ownerType)
	{
		Id = id ?? string.Empty;
		Variant = variant ?? string.Empty;
		OwnerType = ownerType;
	}
}

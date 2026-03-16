namespace Wotc.Mtga.DuelScene.Companions;

public class NullCompanionDataProvider : ICompanionDataProvider
{
	public static readonly ICompanionDataProvider Default = new NullCompanionDataProvider();

	public CompanionData GetCompanionDataForPlayer(uint instanceId)
	{
		return default(CompanionData);
	}
}

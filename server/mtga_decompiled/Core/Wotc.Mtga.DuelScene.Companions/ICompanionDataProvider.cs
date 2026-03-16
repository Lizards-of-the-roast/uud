namespace Wotc.Mtga.DuelScene.Companions;

public interface ICompanionDataProvider
{
	CompanionData GetCompanionDataForPlayer(uint instanceId);

	bool TryGetCompanionDataForPlayer(uint instanceId, out CompanionData companionData)
	{
		companionData = GetCompanionDataForPlayer(instanceId);
		return !companionData.IsEmpty;
	}
}

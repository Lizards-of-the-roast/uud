namespace Wotc.Mtga.DuelScene.Companions;

public interface ICompanionBuilder
{
	AccessoryController Create(CompanionData companionData);
}

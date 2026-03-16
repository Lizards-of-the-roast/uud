namespace Wotc.Mtga.DuelScene.Companions;

public class NullCompanionBuilder : ICompanionBuilder
{
	public static readonly ICompanionBuilder Default = new NullCompanionBuilder();

	public AccessoryController Create(CompanionData companionData)
	{
		return null;
	}
}

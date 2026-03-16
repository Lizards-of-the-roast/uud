namespace Wotc.Mtga.DuelScene.Interactions;

public class NullChooseXBuilder : IChooseXInterfaceBuilder
{
	public static readonly IChooseXInterfaceBuilder Default = new NullChooseXBuilder();

	public IChooseXInterface CreateInterface(string assetTrackingKey)
	{
		return new NullChooseXInterface();
	}

	public void DestroyInterface(IChooseXInterface chooseXInterface, string assetTrackingKey)
	{
	}
}

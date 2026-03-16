using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Companions;

public class NullCompanionViewController : ICompanionViewController
{
	public static readonly ICompanionViewController Default = new NullCompanionViewController();

	public AccessoryController CreateCompanionForPlayer(MtgPlayer player)
	{
		return null;
	}
}

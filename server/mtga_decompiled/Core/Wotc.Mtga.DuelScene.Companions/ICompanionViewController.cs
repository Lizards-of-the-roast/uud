using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Companions;

public interface ICompanionViewController
{
	AccessoryController CreateCompanionForPlayer(MtgPlayer player);
}

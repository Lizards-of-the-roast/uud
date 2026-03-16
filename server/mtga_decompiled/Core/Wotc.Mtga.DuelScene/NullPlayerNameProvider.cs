using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullPlayerNameProvider : IEntityNameProvider<MtgPlayer>
{
	public static readonly IEntityNameProvider<MtgPlayer> Default = new NullPlayerNameProvider();

	public string GetName(MtgPlayer entity, bool formatted = true)
	{
		return string.Empty;
	}
}

using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullCardNameProvider : IEntityNameProvider<MtgCardInstance>
{
	public static readonly IEntityNameProvider<MtgCardInstance> Default = new NullCardNameProvider();

	public string GetName(MtgCardInstance entity, bool formatted = true)
	{
		return string.Empty;
	}
}

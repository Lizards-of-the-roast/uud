using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullEntityNameProvider : IEntityNameProvider<MtgEntity>
{
	public static readonly IEntityNameProvider<MtgEntity> Default = new NullEntityNameProvider();

	public string GetName(MtgEntity entity, bool formatted = true)
	{
		return string.Empty;
	}
}

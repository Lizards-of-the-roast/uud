using Wotc.Mtga.DuelScene;

namespace GreClient.Test;

public class EchoEntityNameProvider : IEntityNameProvider<uint>
{
	public string GetName(uint entity, bool formatted = true)
	{
		return entity.ToString();
	}
}

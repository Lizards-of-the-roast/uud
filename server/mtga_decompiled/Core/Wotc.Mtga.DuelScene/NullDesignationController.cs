using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullDesignationController : IDesignationController
{
	public static readonly IDesignationController Default = new NullDesignationController();

	public void AddDesignation(DesignationData designation)
	{
	}

	public void RemoveDesignation(DesignationData designation)
	{
	}

	public void UpdateDesignation(DesignationData designation)
	{
	}
}

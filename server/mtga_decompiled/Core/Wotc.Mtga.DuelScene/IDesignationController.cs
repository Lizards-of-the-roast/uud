using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IDesignationController
{
	void AddDesignation(DesignationData designation);

	void RemoveDesignation(DesignationData designation);

	void UpdateDesignation(DesignationData designation);
}

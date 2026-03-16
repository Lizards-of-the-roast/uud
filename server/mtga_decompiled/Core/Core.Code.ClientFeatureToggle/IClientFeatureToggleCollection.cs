namespace Core.Code.ClientFeatureToggle;

public interface IClientFeatureToggleCollection
{
	bool GetFeatureToggleValue(string name);
}

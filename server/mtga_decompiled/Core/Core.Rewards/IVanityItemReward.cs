namespace Core.Rewards;

public interface IVanityItemReward
{
	string VanityItemPrefix { get; }

	void AddVanityItem(string name);
}

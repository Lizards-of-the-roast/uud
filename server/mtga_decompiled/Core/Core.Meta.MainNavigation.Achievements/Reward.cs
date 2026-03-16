using Wizards.Mtga;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Achievements;

public abstract class Reward
{
	protected readonly CampaignGraphManager _campaignGraphManager = Pantry.Get<CampaignGraphManager>();

	protected readonly IClientLocProvider _localizationProvider = Pantry.Get<IClientLocProvider>();

	public abstract string TitleLocKey { get; }

	public abstract string DescriptionLocKey { get; }

	public abstract string Title { get; }

	public abstract string Description { get; }

	public abstract int Amount { get; }

	public abstract string RewardIconPrefab { get; }

	public abstract string CosmeticRewardReferenceID { get; }
}

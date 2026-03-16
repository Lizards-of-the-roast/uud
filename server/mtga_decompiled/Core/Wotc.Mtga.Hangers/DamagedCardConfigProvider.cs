using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public class DamagedCardConfigProvider : IHangerConfigProvider
{
	private const string PARAM_NAME_AMOUNT = "ammount";

	private readonly IClientLocProvider _locProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	public DamagedCardConfigProvider(IClientLocProvider locProvider, AssetLookupSystem assetLookupSystem)
	{
		_locProvider = locProvider ?? NullLocProvider.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (model.Damaged)
		{
			AbilityBadgeData badgeDataForCondition = AbilityBadgeUtil.GetBadgeDataForCondition(_assetLookupSystem, ConditionType.Damaged, model);
			if (badgeDataForCondition != null)
			{
				string localizedText = _locProvider.GetLocalizedText(badgeDataForCondition.LocTitle);
				string localizedText2 = _locProvider.GetLocalizedText(badgeDataForCondition.LocTerm, ("ammount", model.Damage.ToString()));
				yield return new HangerConfig(localizedText, localizedText2, null, badgeDataForCondition.IconSpritePath);
			}
		}
	}
}

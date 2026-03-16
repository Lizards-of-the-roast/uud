using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class OnAnAdventureHangerProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locManager;

	private readonly IPathProvider<string> _iconPathProvider;

	private readonly IEntityNameProvider<uint> _entityNameProvider;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly AssetLookupSystem _assetLookupSystem;

	public OnAnAdventureHangerProvider(IClientLocProvider locManager, IPathProvider<string> iconPathProvider, IEntityNameProvider<uint> entityNameProvider, ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem)
	{
		_locManager = locManager ?? NullLocProvider.Default;
		_iconPathProvider = iconPathProvider ?? NullPathProvider<string>.Default;
		_entityNameProvider = entityNameProvider ?? NullIdNameProvider.Default;
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter cardData)
	{
		if (OnAnAdventure(cardData))
		{
			string localizedText = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/OnAnAdventure_Title");
			string name = _entityNameProvider.GetName(cardData.Controller?.InstanceId ?? 0);
			string localizedText2 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ExiledByPlayer", ("PlayerName", name));
			string localizedText3;
			if (cardData.CardTypes.Contains(CardType.Land))
			{
				localizedText3 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/OnAnAdventure_Land_Body");
			}
			else
			{
				string localizedTextForEnumValue = _cardDatabase.GreLocProvider.GetLocalizedTextForEnumValue(cardData.CardTypes[0]);
				localizedText3 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/OnAnAdventure_Body", ("PermanentType", localizedTextForEnumValue));
			}
			string path = _iconPathProvider.GetPath("OnAnAdventure");
			string format = string.Empty;
			if (_assetLookupSystem.Blackboard != null && _assetLookupSystem.TreeLoader != null)
			{
				format = TargetingColorer.GetHangerTextTargetingFormat(1, _assetLookupSystem, cardData);
			}
			string details = string.Format(format, localizedText2) + "\n" + Environment.NewLine + localizedText3;
			yield return new HangerConfig(localizedText, details, null, path);
		}
	}

	public bool OnAnAdventure(ICardDataAdapter cardData)
	{
		if (cardData?.Instance != null && cardData.AffectedByQualifications.Exists((QualificationData x) => x.AbilityId == 196))
		{
			return true;
		}
		return false;
	}
}

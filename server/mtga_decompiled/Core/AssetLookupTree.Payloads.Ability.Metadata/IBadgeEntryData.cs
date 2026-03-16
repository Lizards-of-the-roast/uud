using System;
using UnityEngine;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace AssetLookupTree.Payloads.Ability.Metadata;

public interface IBadgeEntryData : IComparable<IBadgeEntryData>, IEquatable<IBadgeEntryData>
{
	AltAssetReference<Sprite> SpriteRef { get; }

	INumericBadgeCalculator NumberCalculator { get; }

	IBadgeActivationCalculator ActivationCalculator { get; }

	AltAssetReference<Sprite> ActivatedSpriteRef { get; }

	BadgeEntryCategory Category { get; }

	int Priority { get; }

	bool ValidOnTTP { get; }

	bool ValidOnBattlefield { get; }

	bool ValidOnHanger { get; }

	bool UseSpecialEntryStatusOnBattlefield { get; }

	string GetActivationWord();
}

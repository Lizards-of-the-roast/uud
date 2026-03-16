using System.Collections.Generic;
using Wotc.Mtga.Client.Models.Mercantile;

public interface IStoreManager
{
	IReadOnlyList<StoreItem> Avatars { get; }

	IReadOnlyList<StoreItem> Sleeves { get; }

	IReadOnlyList<StoreItem> CardSkins { get; }

	IReadOnlyList<StoreItem> Bundles { get; }

	IReadOnlyList<StoreItem> Boosters { get; }

	IReadOnlyList<StoreItem> Gems { get; }

	IReadOnlyList<StoreItem> ProgressionTracks { get; }

	IReadOnlyList<StoreItem> Featured { get; }

	IReadOnlyList<StoreItem> Pets { get; }

	IReadOnlyList<StoreItem> Sales { get; }

	IReadOnlyList<StoreItem> Decks { get; }

	IReadOnlyList<StoreItem> PrizeWall { get; }
}

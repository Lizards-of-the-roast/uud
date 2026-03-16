using System.Collections.Generic;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Browsers;

public interface ICardDragRegionProvider : ICardBrowser, IBrowser, IGroupedCardProvider
{
	IEnumerable<Vector2> GetDragZones();

	int DetermineDragZoneIndex(DuelScene_CDC card);

	bool CanChangeZones(DuelScene_CDC card);
}

using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface IPetDataProvider
{
	IReadOnlyList<(string petId, string variantId)> GetAllPetData();
}

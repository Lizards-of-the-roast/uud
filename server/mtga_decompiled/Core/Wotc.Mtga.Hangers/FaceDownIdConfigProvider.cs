using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public class FaceDownIdConfigProvider : IHangerConfigProvider
{
	private readonly IFaceDownIdProvider _faceDownIdProvider;

	private readonly IClientLocProvider _locProvider;

	public FaceDownIdConfigProvider(IFaceDownIdProvider faceDownIdProvider, IClientLocProvider clientLocProvider)
	{
		_faceDownIdProvider = faceDownIdProvider ?? NullFaceDownIdProvider.Default;
		_locProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (_faceDownIdProvider.TryGetFaceDownId(model?.InstanceId ?? 0, out var faceDownId))
		{
			yield return new HangerConfig(string.Empty, GetHangerText(faceDownId));
		}
	}

	private string GetHangerText(uint faceDownId)
	{
		return _locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/FaceDownIdentifier", ("faceDownId", faceDownId.ToString("N0")));
	}
}

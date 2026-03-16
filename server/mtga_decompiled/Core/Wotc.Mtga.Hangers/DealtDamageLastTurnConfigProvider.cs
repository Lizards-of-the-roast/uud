using System.Collections.Generic;
using System.Text;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

internal class DealtDamageLastTurnConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locManager;

	private readonly IGreLocProvider _greLocProvider;

	private StringBuilder _bodySB = new StringBuilder();

	private const string GOLD_FONT_COLOR_START = "<#FF9C01>";

	private const string COLOR_END = "</color>";

	public DealtDamageLastTurnConfigProvider(IClientLocProvider locManager, IGreLocProvider greLocProvider)
	{
		_locManager = locManager ?? NullLocProvider.Default;
		_greLocProvider = greLocProvider ?? NullGreLocManager.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (model.Instance.ReferencedCardTitleIds.Count <= 0)
		{
			yield break;
		}
		_bodySB.Clear();
		_bodySB.AppendLine(_locManager.GetLocalizedText("AbilityHanger/SpecialHangers/PermanentsDealtDamageLastTurn_Body"));
		_bodySB.Append("<#FF9C01>");
		foreach (uint referencedCardTitleId in model.Instance.ReferencedCardTitleIds)
		{
			_bodySB.AppendLine(_greLocProvider.GetLocalizedText(referencedCardTitleId));
		}
		_bodySB.Append("</color>");
		yield return new HangerConfig(string.Empty, _bodySB.ToString());
	}
}

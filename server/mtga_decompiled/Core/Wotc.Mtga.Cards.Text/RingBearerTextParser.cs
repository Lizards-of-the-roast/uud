using System.Collections.Generic;
using System.Text;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Duel;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Cards.Text;

public class RingBearerTextParser : ITextEntryParser
{
	private const string RING_BEARER_LOC_KEY_PREFIX = "Card/Textbox/RingBearer_Body_";

	private readonly IClientLocProvider _clientLocManager;

	private readonly IObjectPool _genericPool;

	private readonly AssetLookupSystem _assetLookupSystem;

	public RingBearerTextParser(IClientLocProvider clientLocManager, IObjectPool genericPool, AssetLookupSystem assetLookupSystem)
	{
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_genericPool = genericPool ?? NullObjectPool.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		foreach (LayeredEffectData item in card?.Instance?.LayeredEffects)
		{
			if (item.Type == "Ring-Bearer")
			{
				yield return CreateTextEntry(item, colorSettings);
			}
		}
	}

	private ICardTextEntry CreateTextEntry(LayeredEffectData layeredEffect, CardTextColorSettings colorSettings)
	{
		StringBuilder stringBuilder = _genericPool.PopObject<StringBuilder>();
		for (int i = 1; i <= layeredEffect.PromptId; i++)
		{
			stringBuilder.AppendLine(_clientLocManager.GetLocalizedText("Card/Textbox/RingBearer_Body_" + i));
		}
		string arg = stringBuilder.ToString();
		bool useDarkHangerColors = colorSettings.DefaultFormat == CardTextColorSettings.INVERTED.DefaultFormat;
		arg = string.Format(TargetingColorer.GetFormatForIndex(1, _assetLookupSystem, useDarkHangerColors), arg);
		stringBuilder.Clear();
		_genericPool.PushObject(stringBuilder, tryClear: false);
		return new BasicTextEntry(arg);
	}
}

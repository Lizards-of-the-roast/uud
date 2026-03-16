using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class InjectControllersGraveyardPermanentTypes : IParameterizedInjector
{
	private const string PARAM_TO_REPLACE = "{permanentTypes}";

	private const string COLORIZED_FORMAT = "<#FF9C01>{0}</color>";

	private readonly IGreLocProvider _greLocProvider;

	private readonly IObjectPool _objPool;

	private readonly IGameStateProvider _gameStateProvider;

	public InjectControllersGraveyardPermanentTypes(IGreLocProvider greLocProvider, IObjectPool objPool, IGameStateProvider gameStateProvider)
	{
		_greLocProvider = greLocProvider ?? NullGreLocManager.Default;
		_objPool = objPool ?? NullObjectPool.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		return value.Replace("{permanentTypes}", $"<#FF9C01>{ReplacementString(model)}</color>");
	}

	private string ReplacementString(ICardDataAdapter model)
	{
		return LocalizedPermanentTypes(ToPermanentCardTypes(GetAllCardTypesInZone(GetAssociatedGraveyard(model))));
	}

	private MtgZone GetAssociatedGraveyard(ICardDataAdapter model)
	{
		foreach (KeyValuePair<uint, MtgZone> zone in ((MtgGameState)_gameStateProvider.CurrentGameState).Zones)
		{
			MtgZone value = zone.Value;
			if (value.Type == ZoneType.Graveyard && value.Owner?.InstanceId == model.Controller?.InstanceId)
			{
				return value;
			}
		}
		return new MtgZone();
	}

	private IEnumerable<CardType> GetAllCardTypesInZone(MtgZone zone)
	{
		HashSet<CardType> cardTypes = _objPool.PopObject<HashSet<CardType>>();
		cardTypes.UnionWith(zone.VisibleCards.SelectMany((MtgCardInstance x) => x.CardTypes));
		foreach (CardType item in cardTypes)
		{
			if (item.IsPermanent())
			{
				yield return item;
			}
		}
		cardTypes.Clear();
		_objPool.PushObject(cardTypes);
	}

	private IEnumerable<CardType> ToPermanentCardTypes(IEnumerable<CardType> cardTypes)
	{
		foreach (CardType item in cardTypes ?? Array.Empty<CardType>())
		{
			if (item.IsPermanent())
			{
				yield return item;
			}
		}
	}

	private string LocalizedPermanentTypes(IEnumerable<CardType> cardTypes)
	{
		StringBuilder stringBuilder = _objPool.PopObject<StringBuilder>();
		stringBuilder.AppendLine();
		foreach (CardType item in cardTypes ?? Array.Empty<CardType>())
		{
			stringBuilder.Append(_greLocProvider.GetLocalizedTextForEnumValue(item) + ", ");
		}
		stringBuilder.Length -= 2;
		string result = stringBuilder.ToString();
		stringBuilder.Clear();
		_objPool.PushObject(stringBuilder);
		return result;
	}
}

using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Counter;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class CounterConfigProvider : IHangerConfigProvider
{
	private const string PARAM_NAME_AMOUNT = "amount";

	private const string PARAM_NAME_KEYWORD = "keyword";

	private const string PARAM_NAME_TYPE = "type";

	private readonly CounterSortingDataComparer _counterSortingDataComparer = new CounterSortingDataComparer();

	private readonly IClientLocProvider _clientLocManager;

	private readonly IGreLocProvider _greLocProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly List<CDCPart_Counters.CounterSortingData> _sortedCounters = new List<CDCPart_Counters.CounterSortingData>();

	private readonly List<HangerConfig> _cache = new List<HangerConfig>();

	public CounterConfigProvider(IClientLocProvider clientLocProvider, IGreLocProvider greLocProvider, AssetLookupSystem assetLookupSystem)
	{
		_clientLocManager = clientLocProvider ?? NullLocProvider.Default;
		_greLocProvider = greLocProvider ?? NullGreLocManager.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		_cache.Clear();
		if (model.Counters.Count == 0)
		{
			return _cache;
		}
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<HangerEntry> loadedTree))
		{
			return _cache;
		}
		IReadOnlyDictionary<CounterType, int> counters = model.Counters;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<CounterVisuals> loadedTree2))
		{
			_sortedCounters.Clear();
			foreach (CounterType key in counters.Keys)
			{
				_assetLookupSystem.Blackboard.CounterType = key;
				_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
				if (counters[key] > 0)
				{
					CounterVisuals payload = loadedTree2.GetPayload(_assetLookupSystem.Blackboard);
					if (payload != null)
					{
						_sortedCounters.Add(new CDCPart_Counters.CounterSortingData(payload.counterCategory, key));
					}
				}
			}
			_sortedCounters.Sort(_counterSortingDataComparer);
		}
		foreach (CDCPart_Counters.CounterSortingData sortedCounter in _sortedCounters)
		{
			int counterCount = counters[sortedCounter.CounterType];
			CounterAssetData counterAsset = CounterAssetUtil.GetCounterAsset(_assetLookupSystem, sortedCounter.CounterType, model);
			if (counterAsset != null && !string.IsNullOrEmpty(counterAsset.UiSpritePath))
			{
				_assetLookupSystem.Blackboard.Clear();
				_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
				_assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
				_assetLookupSystem.Blackboard.CounterType = sortedCounter.CounterType;
				HangerEntry payload2 = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload2 != null)
				{
					HangerEntry.HeaderFormat format = payload2.Format;
					string header = ((format == HangerEntry.HeaderFormat.Default || format != HangerEntry.HeaderFormat.Keyword_Counter) ? GetDefaultCounterHeader(sortedCounter.CounterType, counterCount) : GetKeywordCounterHeader(payload2.Keyword.GetText(_clientLocManager, _greLocProvider), counterCount));
					string details = payload2.Body.GetText(_clientLocManager, _greLocProvider);
					_cache.Add(new HangerConfig(header, details, null, counterAsset.UiSpritePath));
				}
			}
		}
		return _cache;
	}

	private string GetDefaultCounterHeader(CounterType counterType, int counterCount)
	{
		string localizedTextForEnumValue = _greLocProvider.GetLocalizedTextForEnumValue(counterType);
		return _clientLocManager.GetLocalizedText((counterCount == 1) ? "AbilityHanger/Counter_Singular" : "AbilityHanger/Counter_Plural", ("amount", counterCount.ToString()), ("type", localizedTextForEnumValue));
	}

	private string GetKeywordCounterHeader(string keywordText, int counterCount)
	{
		string key = ((counterCount > 1) ? "AbilityHanger/KeywordCounter_Plural" : "AbilityHanger/KeywordCounter_Singular");
		return _clientLocManager.GetLocalizedText(key, ("amount", counterCount.ToString()), ("keyword", keywordText));
	}
}

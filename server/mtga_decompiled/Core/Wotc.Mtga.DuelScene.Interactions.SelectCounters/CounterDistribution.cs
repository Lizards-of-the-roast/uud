using System;
using System.Collections.Generic;
using Pooling;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public class CounterDistribution
{
	private readonly SpinnerController _spinnerController;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly IObjectPool _pool;

	private StackCardHolder _stackCache;

	private ICardHolder _battlefieldCache;

	private uint _max;

	private StackCardHolder Stack => _stackCache ?? (_stackCache = _cardHolderProvider.GetCardHolder<StackCardHolder>(GREPlayerNum.Invalid, CardHolderType.Stack));

	private ICardHolder Battlefield => _battlefieldCache ?? (_battlefieldCache = _cardHolderProvider.GetCardHolder(GREPlayerNum.Invalid, CardHolderType.Battlefield));

	public bool Active { get; private set; }

	public event Action<uint, uint> ValueChanged
	{
		add
		{
			_spinnerController.ValueChanged += value;
		}
		remove
		{
			_spinnerController.ValueChanged -= value;
		}
	}

	public CounterDistribution(SpinnerController spinnerController, ICardHolderProvider cardHolderProvider, IObjectPool objPool)
	{
		_spinnerController = spinnerController;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_pool = objPool ?? NullObjectPool.Default;
	}

	public void SetMax(uint max)
	{
		_max = max;
	}

	public void Apply(IReadOnlyDictionary<uint, List<CounterPair>> pairsById)
	{
		Active = true;
		List<SpinnerData> list = _pool.PopObject<List<SpinnerData>>();
		foreach (KeyValuePair<uint, List<CounterPair>> item in pairsById)
		{
			uint num = 0u;
			foreach (CounterPair item2 in item.Value)
			{
				num += item2.Count;
			}
			uint max = ((num > _max) ? _max : num);
			list.Add(new SpinnerData(item.Key, 0, 0, (int)max));
		}
		_spinnerController.Open(list);
		list.Clear();
		_pool.PushObject(list);
		Stack.TryAutoDock(pairsById.Keys);
		ValueChanged += OnValueChanged;
	}

	private void OnValueChanged(uint id, uint value)
	{
		Battlefield.LayoutNow();
	}

	public void CleanUp()
	{
		ValueChanged -= OnValueChanged;
		if (_stackCache != null)
		{
			_stackCache.ResetAutoDock();
			_stackCache = null;
		}
		_battlefieldCache = null;
		_spinnerController.Close();
		Active = false;
	}
}

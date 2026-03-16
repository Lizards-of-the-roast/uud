using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public readonly struct WeightedSelectionState
{
	private readonly bool _hasWeight;

	private readonly int _minWeight;

	private readonly int _maxWeight;

	private readonly int _minSel;

	private readonly uint _maxSel;

	public WeightedSelectionState(bool hasWeight, int minWeight, int maxWeight, int minSel, uint maxSel)
	{
		_hasWeight = hasWeight;
		_minWeight = minWeight;
		_maxWeight = maxWeight;
		_minSel = minSel;
		_maxSel = maxSel;
	}

	public WeightedSelectionState(SelectNRequest selectNRequest)
		: this(selectNRequest.Weights.Count > 0, selectNRequest.MinWeight, selectNRequest.MaxWeight, selectNRequest.MinSel, selectNRequest.MaxSel)
	{
	}

	public bool CanSelect(int selections, int weight)
	{
		if (_hasWeight)
		{
			if (_minWeight != int.MinValue || _maxWeight != int.MaxValue)
			{
				if (weight <= _maxWeight)
				{
					return selections <= _maxSel;
				}
				return false;
			}
			return weight <= _maxSel;
		}
		return selections <= _maxSel;
	}

	public bool CanSubmit(int selections, int weight)
	{
		if (_hasWeight)
		{
			if (_minWeight != int.MinValue || _maxWeight != int.MaxValue)
			{
				if (weight >= _minWeight && weight <= _maxWeight && selections >= _minSel)
				{
					return selections <= _maxSel;
				}
				return false;
			}
			if (weight >= _minSel)
			{
				return weight <= _maxSel;
			}
			return false;
		}
		if (selections >= _minSel)
		{
			return selections <= _maxSel;
		}
		return false;
	}
}

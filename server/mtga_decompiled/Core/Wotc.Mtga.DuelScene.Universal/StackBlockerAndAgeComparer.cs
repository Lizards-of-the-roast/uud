using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Universal;

public class StackBlockerAndAgeComparer : StackAgeComparer
{
	private MtgGameState _gameState;

	private IReadOnlyCollection<CardLayoutData> _solvedLayoutDatas;

	public void Init(MtgGameState gameState, IReadOnlyCollection<CardLayoutData> solvedLayoutDatas)
	{
		_gameState = gameState;
		_solvedLayoutDatas = solvedLayoutDatas;
	}

	public override int Compare(UniversalBattlefieldStack a, UniversalBattlefieldStack b)
	{
		bool isBlockStack = a.IsBlockStack;
		bool isBlockStack2 = b.IsBlockStack;
		if (!isBlockStack && !isBlockStack2)
		{
			return base.Compare(a, b);
		}
		if (isBlockStack != isBlockStack2)
		{
			return isBlockStack2.CompareTo(isBlockStack);
		}
		ICardDataAdapter aModel = a.StackParentModel;
		ICardDataAdapter bModel = b.StackParentModel;
		uint aBlockingId = aModel.Instance.BlockingIds.FirstOrDefault();
		uint bBlockingId = bModel.Instance.BlockingIds.FirstOrDefault();
		float num = _solvedLayoutDatas.FirstOrDefault((CardLayoutData aLayoutData) => aLayoutData.Card.InstanceId == aBlockingId)?.Position.x ?? 0f;
		float value = _solvedLayoutDatas.FirstOrDefault((CardLayoutData bLayoutData) => bLayoutData.Card.InstanceId == bBlockingId)?.Position.x ?? 0f;
		int num2 = num.CompareTo(value);
		if (num2 != 0)
		{
			return -num2;
		}
		int num3 = -1;
		int value2 = -1;
		if (_gameState.AttackInfo.TryGetValue(aBlockingId, out var value3))
		{
			OrderedDamageAssignment orderedDamageAssignment = value3.OrderedBlockers.Where((OrderedDamageAssignment x) => x.InstanceId == aModel.InstanceId).FirstOrDefault();
			if (orderedDamageAssignment != null)
			{
				num3 = value3.OrderedBlockers.IndexOf(orderedDamageAssignment);
			}
		}
		if (_gameState.AttackInfo.TryGetValue(bBlockingId, out var value4))
		{
			OrderedDamageAssignment orderedDamageAssignment2 = value4.OrderedBlockers.Where((OrderedDamageAssignment x) => x.InstanceId == bModel.InstanceId).FirstOrDefault();
			if (orderedDamageAssignment2 != null)
			{
				value2 = value4.OrderedBlockers.IndexOf(orderedDamageAssignment2);
			}
		}
		num2 = num3.CompareTo(value2);
		if (num2 != 0)
		{
			return num2;
		}
		return base.Compare(a, b);
	}
}

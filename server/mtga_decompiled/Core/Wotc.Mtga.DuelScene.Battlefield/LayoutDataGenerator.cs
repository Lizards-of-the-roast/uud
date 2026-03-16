using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Battlefield;

public class LayoutDataGenerator
{
	private readonly BattlefieldRegionType _regionType;

	private readonly PageHandler _pageHandler;

	private readonly Dictionary<BattlefieldCardHolder.BattlefieldStack, float> _widthsByStack;

	private readonly IReadOnlyDictionary<ScaffoldShape, List<BattlefieldCardHolder.BattlefieldStack>> _stacksByShape;

	private readonly List<(BattlefieldRegionDefinition.SubRegion, IReadOnlyList<BattlefieldCardHolder.BattlefieldStack>)> _subRegionStacks = new List<(BattlefieldRegionDefinition.SubRegion, IReadOnlyList<BattlefieldCardHolder.BattlefieldStack>)>();

	private readonly List<BattlefieldCardHolder.BattlefieldStack> _combinedStackList = new List<BattlefieldCardHolder.BattlefieldStack>();

	public LayoutDataGenerator(BattlefieldRegionType regionType, PageHandler pageHandler, Dictionary<BattlefieldCardHolder.BattlefieldStack, float> widthsByStack, IReadOnlyDictionary<ScaffoldShape, List<BattlefieldCardHolder.BattlefieldStack>> stacksByShape)
	{
		_regionType = regionType;
		_pageHandler = pageHandler;
		_widthsByStack = widthsByStack;
		_stacksByShape = stacksByShape;
	}

	public void GenerateLayoutDatas(Rect cardBounds, BattlefieldRegionDefinition.LayoutVariant variant, List<BattlefieldCardHolder.BattlefieldStack> stacks, List<CardLayoutData> layoutData, Vector3 attackOffset, Vector3 tappedRotation, List<BattlefieldCardHolder.StackCounterData> counterData, float battlefieldHeight)
	{
		if (variant.TryGetSingleLayout(out var layout))
		{
			_pageHandler.SetupGroups(1, !variant.UsesPaging, 0);
			GenerateDataForSubRegion(cardBounds, variant, layout, _pageHandler.CurrentGroup, stacks, layoutData, attackOffset, tappedRotation, counterData, battlefieldHeight);
			return;
		}
		_subRegionStacks.Clear();
		foreach (BattlefieldRegionDefinition.SubRegion subRegion in variant.SubRegions)
		{
			IReadOnlyList<BattlefieldCardHolder.BattlefieldStack> stacksByShape = GetStacksByShape(subRegion.Shape, variant.SupportedShapes);
			if (stacksByShape.Count > 0)
			{
				_subRegionStacks.Add((subRegion, stacksByShape));
			}
		}
		int num = ((!variant.TreatSubRegionsAsPages) ? 1 : _subRegionStacks.Count);
		_pageHandler.SetupGroups(num, !variant.UsesPaging, num - 1);
		for (int i = 0; i < _subRegionStacks.Count; i++)
		{
			(BattlefieldRegionDefinition.SubRegion, IReadOnlyList<BattlefieldCardHolder.BattlefieldStack>) tuple = _subRegionStacks[i];
			int index = (variant.TreatSubRegionsAsPages ? i : 0);
			GenerateDataForSubRegion(cardBounds, variant, tuple.Item1, _pageHandler.Get(index), tuple.Item2, layoutData, attackOffset, tappedRotation, counterData, battlefieldHeight);
		}
	}

	private IReadOnlyList<BattlefieldCardHolder.BattlefieldStack> GetStacksByShape(ScaffoldShape shape, ICollection<ScaffoldShape> supported)
	{
		if (shape == ScaffoldShape.None)
		{
			_combinedStackList.Clear();
			foreach (ScaffoldShape key in _stacksByShape.Keys)
			{
				if (!supported.Contains(key))
				{
					_combinedStackList.AddRange(_stacksByShape[key]);
				}
			}
			return _combinedStackList;
		}
		if (_stacksByShape.TryGetValue(shape, out var value))
		{
			return value;
		}
		return Array.Empty<BattlefieldCardHolder.BattlefieldStack>();
	}

	private void GenerateDataForSubRegion(Rect cardBounds, BattlefieldRegionDefinition.LayoutVariant variant, BattlefieldRegionDefinition.SubRegion subRegion, PageGroup pageGroup, IReadOnlyList<BattlefieldCardHolder.BattlefieldStack> stacks, List<CardLayoutData> layoutData, Vector3 attackOffset, Vector3 tappedRotation, List<BattlefieldCardHolder.StackCounterData> counterData, float battlefieldHeight)
	{
		Rect rect = subRegion.CalcBounds(variant.Bounds);
		Rect regionBounds = subRegion.CalcBounds(cardBounds);
		CalculateStackWidths(subRegion, stacks);
		float num = stacks.Max(subRegion.CardScale, (BattlefieldCardHolder.BattlefieldStack stack, float cardScale) => stack.CalcHeight(cardScale));
		float num2 = subRegion.VerticalGutter + num;
		float num3 = ((subRegion.RowCount == 1) ? rect.center.y : (rect.center.y - num * 0.5f * (float)(subRegion.RowCount - 1) - subRegion.VerticalGutter * 0.5f * (float)(subRegion.RowCount - 1)));
		int startStackIdx = 0;
		for (int num4 = 0; num4 < subRegion.RowCount; num4++)
		{
			int num5 = Mathf.CeilToInt((float)(stacks.Count - startStackIdx) / (float)(subRegion.RowCount - num4));
			Vector3 rowStartingPosition = GetRowStartingPosition(stacks, num5, ref startStackIdx, regionBounds, subRegion, battlefieldHeight, num3);
			pageGroup.AddRow(num4);
			rowStartingPosition = AdjustPositionForPaging(subRegion, rowStartingPosition, pageGroup, num4, num5, stacks, startStackIdx);
			for (int num6 = startStackIdx; num6 < startStackIdx + num5; num6++)
			{
				BattlefieldCardHolder.BattlefieldStack battlefieldStack = stacks[num6];
				float num7 = _widthsByStack[battlefieldStack];
				List<CardLayoutData> list = battlefieldStack.GenerateData(subRegion.CardScale, subRegion.StackWidth, subRegion.MinStackWidth, subRegion.StackLimit, _regionType == BattlefieldRegionType.Creature);
				AddStackToPageGroup(battlefieldStack, num4, pageGroup, variant.UsesPaging, rowStartingPosition, regionBounds, out var outOfBounds);
				rowStartingPosition += Vector3.right * (num7 * 0.5f);
				Vector3 pageOffset = GetPageOffset(variant, pageGroup, regionBounds);
				if (pageOffset != Vector3.zero)
				{
					outOfBounds = true;
				}
				if (!outOfBounds)
				{
					PositionAttachments(battlefieldStack, rowStartingPosition, subRegion, counterData);
				}
				foreach (CardLayoutData item in list)
				{
					Vector3 vector = rowStartingPosition + pageOffset;
					if (battlefieldStack.IsAttackStack)
					{
						vector += attackOffset;
					}
					if (outOfBounds)
					{
						item.IsVisibleInLayout = false;
						vector.y = -5f;
					}
					item.Position += vector;
					if (item.Card.Model.IsTapped)
					{
						item.Rotation *= Quaternion.Euler(tappedRotation);
					}
					layoutData.Add(item);
				}
				DrawDebugBounds(rowStartingPosition, num7, num, outOfBounds);
				rowStartingPosition += Vector3.right * (num7 * 0.5f);
				rowStartingPosition += Vector3.right * subRegion.HorizontalGutter;
			}
			startStackIdx += num5;
			num3 += num2;
		}
	}

	private Vector3 GetRowStartingPosition(IReadOnlyList<BattlefieldCardHolder.BattlefieldStack> stacks, int stackCount, ref int startStackIdx, Rect regionBounds, BattlefieldRegionDefinition.SubRegion subRegion, float battlefieldHeight, float nextRowZ)
	{
		float num = 0f;
		for (int i = startStackIdx; i < startStackIdx + stackCount; i++)
		{
			num += _widthsByStack[stacks[i]];
		}
		float num2 = num + subRegion.HorizontalGutter * (float)(stackCount - 1);
		Vector3 result = Vector3.zero;
		switch (subRegion.Alignment)
		{
		case BattlefieldRegionDefinition.EAlignment.Center:
			result = new Vector3(regionBounds.center.x - num2 * 0.5f, battlefieldHeight, nextRowZ);
			break;
		case BattlefieldRegionDefinition.EAlignment.Left:
			result = new Vector3(regionBounds.xMax - num2, battlefieldHeight, nextRowZ);
			break;
		case BattlefieldRegionDefinition.EAlignment.Right:
			result = new Vector3(regionBounds.xMin, battlefieldHeight, nextRowZ);
			break;
		}
		return result;
	}

	private Vector3 AdjustPositionForPaging(BattlefieldRegionDefinition.SubRegion subRegion, Vector3 startPos, PageGroup pageGroup, int rowIdx, int stackCount, IReadOnlyList<BattlefieldCardHolder.BattlefieldStack> stacks, int startStackIdx)
	{
		if (pageGroup.IndexOffsetByRow[rowIdx] > 0)
		{
			pageGroup.IndexOffsetByRow[rowIdx] = Mathf.Min(pageGroup.IndexOffsetByRow[rowIdx], stackCount - 1);
			for (int i = 0; i < pageGroup.IndexOffsetByRow[rowIdx]; i++)
			{
				BattlefieldCardHolder.BattlefieldStack key = stacks[startStackIdx + i];
				startPos += Vector3.right * _widthsByStack[key];
				startPos += Vector3.right * subRegion.HorizontalGutter;
			}
		}
		else if (pageGroup.IndexOffsetByRow[rowIdx] < 0)
		{
			pageGroup.IndexOffsetByRow[rowIdx] = Mathf.Max(pageGroup.IndexOffsetByRow[rowIdx], 1 - stackCount);
			for (int num = 0; num > pageGroup.IndexOffsetByRow[rowIdx]; num--)
			{
				BattlefieldCardHolder.BattlefieldStack key2 = stacks[startStackIdx + stackCount + num - 1];
				startPos -= Vector3.right * _widthsByStack[key2];
				startPos -= Vector3.right * subRegion.HorizontalGutter;
			}
		}
		return startPos;
	}

	private Vector3 GetPageOffset(BattlefieldRegionDefinition.LayoutVariant variant, PageGroup pageGroup, Rect regionBounds)
	{
		if (variant.UsesPaging && variant.TreatSubRegionsAsPages && pageGroup != _pageHandler.CurrentGroup)
		{
			int num = ((pageGroup.GroupIndex >= _pageHandler.CurrentPageGroupIndex) ? 1 : (-1));
			int num2 = Math.Abs(pageGroup.GroupIndex - _pageHandler.CurrentPageGroupIndex);
			float num3 = regionBounds.width * (float)num2;
			return Vector3.right * num3 * num;
		}
		return Vector3.zero;
	}

	private void AddStackToPageGroup(BattlefieldCardHolder.BattlefieldStack stack, int rowIdx, PageGroup pageGroup, bool usesPaging, Vector3 position, Rect regionBounds, out bool outOfBounds)
	{
		outOfBounds = false;
		if (usesPaging)
		{
			if (position.x + 0.1f < regionBounds.xMin)
			{
				pageGroup.AddToPageRight(stack, rowIdx);
				outOfBounds = true;
			}
			else if (position.x + _widthsByStack[stack] - 0.1f > regionBounds.xMax)
			{
				pageGroup.AddToPageLeft(stack, rowIdx);
				outOfBounds = true;
			}
		}
		if (!outOfBounds)
		{
			pageGroup.AddToVisible(stack, rowIdx);
		}
	}

	private void PositionAttachments(BattlefieldCardHolder.BattlefieldStack stack, Vector3 stackPos, BattlefieldRegionDefinition.SubRegion subRegion, List<BattlefieldCardHolder.StackCounterData> counterData)
	{
		int attachmentCount = stack.AttachmentCount;
		int exileCount = stack.ExileCount;
		if (attachmentCount > 0 || exileCount > 0)
		{
			if (stack.AllCards.Count > subRegion.StackLimit)
			{
				if (attachmentCount > 0)
				{
					counterData.Add(new BattlefieldCardHolder.StackCounterData
					{
						positon = stack.GetAttachmentCounterPosition(subRegion.CardScale, subRegion.StackWidth, subRegion.MinStackWidth, subRegion.StackLimit) + stackPos,
						count = attachmentCount,
						parentInstanceId = stack.StackParentModel.InstanceId
					});
				}
				if (exileCount > 0)
				{
					counterData.Add(new BattlefieldCardHolder.StackCounterData
					{
						positon = stack.GetExileCounterPosition(subRegion.CardScale, subRegion.StackWidth, subRegion.MinStackWidth, subRegion.StackLimit) + stackPos,
						count = exileCount,
						parentInstanceId = stack.StackParentModel.InstanceId
					});
				}
			}
		}
		else if (stack.AllCards.Count > subRegion.StackCountDisplay)
		{
			counterData.Add(new BattlefieldCardHolder.StackCounterData
			{
				positon = stack.GetCopyCounterPosition(subRegion.CardScale, subRegion.StackWidth, subRegion.MinStackWidth, subRegion.StackLimit) + stackPos,
				count = stack.CardCount,
				parentInstanceId = stack.StackParentModel.InstanceId
			});
		}
	}

	private void CalculateStackWidths(BattlefieldRegionDefinition.SubRegion subRegion, IReadOnlyList<BattlefieldCardHolder.BattlefieldStack> stacks)
	{
		foreach (BattlefieldCardHolder.BattlefieldStack stack in stacks)
		{
			if (!_widthsByStack.ContainsKey(stack))
			{
				float value = stack.CalcWidth(subRegion.CardScale, subRegion.StackWidth, subRegion.MinStackWidth);
				_widthsByStack[stack] = value;
			}
		}
	}

	private void DrawDebugBounds(Vector3 stackPos, float stackWidth, float cardHeight, bool outOfBounds)
	{
		if (outOfBounds)
		{
			BattlefieldCardHolder.DebugDraw.Square(new Rect(stackPos, new Vector2(stackWidth, cardHeight)), Color.red, 3f);
		}
		else
		{
			BattlefieldCardHolder.DebugDraw.Square(new Rect(stackPos, new Vector2(stackWidth, cardHeight)), Color.green, 3f);
		}
	}
}

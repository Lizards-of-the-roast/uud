using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public static class ListCardUtility
{
	public class FlattenedDisplayInfoList : List<ListMetaCardViewDisplayInformation>
	{
	}

	private class InfoAndIndex
	{
		public ListMetaCardViewDisplayInformation Info;

		public int Index;

		public InfoAndIndex(ListMetaCardViewDisplayInformation info, int index)
		{
			Info = info;
			Index = index;
		}
	}

	public static void SetManaCost(this ListMetaCardView_Expanding cardView, CardData cardData)
	{
		string text = string.Empty;
		if (cardData.CardTypes.Count != 1 || cardData.CardTypes[0] != CardType.Land)
		{
			text = cardData.OldSchoolManaText;
		}
		cardView.SetManaCost(ManaUtilities.ConvertManaSymbols(text));
	}

	public static Sprite GetFrameSprite(CardData cardData, FrameSpriteData frameSprites)
	{
		IReadOnlyList<CardColor> getFrameColors = cardData.GetFrameColors;
		Dictionary<CardColor, bool> dictionary = new Dictionary<CardColor, bool>();
		dictionary[CardColor.White] = getFrameColors.Contains(CardColor.White);
		dictionary[CardColor.Blue] = getFrameColors.Contains(CardColor.Blue);
		dictionary[CardColor.Black] = getFrameColors.Contains(CardColor.Black);
		dictionary[CardColor.Red] = getFrameColors.Contains(CardColor.Red);
		dictionary[CardColor.Green] = getFrameColors.Contains(CardColor.Green);
		int num = dictionary.Count((KeyValuePair<CardColor, bool> kvp) => kvp.Value);
		switch (num)
		{
		case 0:
			if (cardData.CardTypes.Contains(CardType.Artifact))
			{
				return frameSprites.Artifact;
			}
			return frameSprites.Colorless;
		case 1:
			if (dictionary[CardColor.White])
			{
				return frameSprites.White;
			}
			if (dictionary[CardColor.Blue])
			{
				return frameSprites.Blue;
			}
			if (dictionary[CardColor.Black])
			{
				return frameSprites.Black;
			}
			if (dictionary[CardColor.Red])
			{
				return frameSprites.Red;
			}
			if (dictionary[CardColor.Green])
			{
				return frameSprites.Green;
			}
			Debug.LogErrorFormat("Unable to GetFrameSprite for card grpId {0} (should not reach this line of code)", cardData.GrpId);
			break;
		}
		if (num == 2)
		{
			if (dictionary[CardColor.White] && dictionary[CardColor.Blue])
			{
				return frameSprites.WhiteBlue;
			}
			if (dictionary[CardColor.White] && dictionary[CardColor.Black])
			{
				return frameSprites.WhiteBlack;
			}
			if (dictionary[CardColor.Blue] && dictionary[CardColor.Black])
			{
				return frameSprites.BlueBlack;
			}
			if (dictionary[CardColor.Blue] && dictionary[CardColor.Red])
			{
				return frameSprites.BlueRed;
			}
			if (dictionary[CardColor.Black] && dictionary[CardColor.Red])
			{
				return frameSprites.BlackRed;
			}
			if (dictionary[CardColor.Black] && dictionary[CardColor.Green])
			{
				return frameSprites.BlackGreen;
			}
			if (dictionary[CardColor.Red] && dictionary[CardColor.Green])
			{
				return frameSprites.RedGreen;
			}
			if (dictionary[CardColor.Red] && dictionary[CardColor.White])
			{
				return frameSprites.RedWhite;
			}
			if (dictionary[CardColor.Green] && dictionary[CardColor.White])
			{
				return frameSprites.GreenWhite;
			}
			if (dictionary[CardColor.Green] && dictionary[CardColor.Blue])
			{
				return frameSprites.GreenBlue;
			}
			Debug.LogErrorFormat("Unable to GetFrameSprite for card grpId {0} (should not reach this line of code)", cardData.GrpId);
		}
		return frameSprites.Multicolor;
	}

	public static ListMetaCardView_Expanding CreateListCardView(ListMetaCardView_Expanding listCardViewPrefab, Transform cardParent, CardData cardData, CardDatabase cardDatabase)
	{
		ListMetaCardView_Expanding listMetaCardView_Expanding = UnityEngine.Object.Instantiate(listCardViewPrefab);
		Transform transform = listMetaCardView_Expanding.transform;
		transform.SetParent(cardParent);
		transform.localScale = Vector3.one;
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		Vector3 localPosition = transform.localPosition;
		localPosition.z = 0f;
		transform.localPosition = localPosition;
		listMetaCardView_Expanding.Card = cardData;
		listMetaCardView_Expanding.SetName(cardDatabase.GreLocProvider.GetLocalizedText(cardData.TitleId));
		return listMetaCardView_Expanding;
	}

	public static void SetButtonClickCallbacks(this ListMetaCardView_Expanding listCardView, Action<MetaCardView> onTagClickedCallback, Action<MetaCardView> onTileClickedCallback)
	{
		listCardView.OnAddClicked = (Action<MetaCardView>)Delegate.Combine(listCardView.OnAddClicked, (Action<MetaCardView>)delegate(MetaCardView cardView)
		{
			onTagClickedCallback?.Invoke(cardView);
		});
		listCardView.OnRemoveClicked = (Action<MetaCardView>)Delegate.Combine(listCardView.OnRemoveClicked, (Action<MetaCardView>)delegate(MetaCardView cardView)
		{
			onTileClickedCallback?.Invoke(cardView);
		});
	}

	public static void SetButtonEnabled(this ListMetaCardView_Expanding listCardView, bool isAddButtonEnabled, bool isRemoveButtonEnabled)
	{
		listCardView.TagButton.enabled = isAddButtonEnabled;
		listCardView.TileButton.enabled = isRemoveButtonEnabled;
	}

	public static void SetFrameSprite(this ListMetaCardView_Expanding listCardView, string skinCode, CardData cardData, FrameSpriteData[] frameSpriteData)
	{
		FrameSpriteData frameSpriteData2 = (string.IsNullOrEmpty(skinCode) ? frameSpriteData[0] : frameSpriteData[1]);
		listCardView.SetFrameSprite(GetFrameSprite(cardData, frameSpriteData2));
		listCardView.SetNameColor(frameSpriteData2.TitleTextColor);
	}

	public static void SetAnimationData(this ListMetaCardView_Expanding listCardView, ListCardViewAnimationData listCardAnimationData)
	{
		Sequence s = DOTween.Sequence();
		s.Append(listCardView.LayoutElement.DOMinSize(new Vector2(listCardView.LayoutElement.minWidth, 0f), listCardAnimationData.GrowDuration).From().SetEase(listCardAnimationData.GrowEase));
		s.Append(listCardView.CanvasGroup.DOFade(0f, listCardAnimationData.FadeInDuration).From().SetEase(listCardAnimationData.FadeInEase));
	}

	public static bool IsEqualTo(this IEnumerable<ListMetaCardViewDisplayInformation> thisDisplayDataSet, IEnumerable<ListMetaCardViewDisplayInformation> thatDisplayDataSet)
	{
		if (thisDisplayDataSet.Count() != thatDisplayDataSet.Count())
		{
			return false;
		}
		List<ListMetaCardViewDisplayInformation> list = thatDisplayDataSet.ToList();
		foreach (ListMetaCardViewDisplayInformation thisDisplayData in thisDisplayDataSet)
		{
			ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation = list.FirstOrDefault((ListMetaCardViewDisplayInformation data) => data.Equals(thisDisplayData));
			if (listMetaCardViewDisplayInformation == null)
			{
				return false;
			}
			list.Remove(listMetaCardViewDisplayInformation);
		}
		return list.Count == 0;
	}

	public static (List<T> Added, List<T> Removed) GetAddedRemoved<T>(IEnumerable<T> oldSet, IEnumerable<T> newSet, Func<T, T, bool> equalityChecker)
	{
		List<T> list = new List<T>(newSet);
		List<T> list2 = new List<T>(oldSet);
		foreach (T oldItem in oldSet)
		{
			T val = list.Find((T data) => equalityChecker(data, oldItem));
			if (val != null)
			{
				list.Remove(val);
			}
		}
		foreach (T newItem in newSet)
		{
			T val2 = list2.Find((T data) => equalityChecker(data, newItem));
			if (val2 != null)
			{
				list2.Remove(val2);
			}
		}
		return (Added: list, Removed: list2);
	}

	public static FlattenedDisplayInfoList Flatten(IEnumerable<ListMetaCardViewDisplayInformation> displayDataSet)
	{
		FlattenedDisplayInfoList flattenedDisplayInfoList = new FlattenedDisplayInfoList();
		foreach (ListMetaCardViewDisplayInformation item in displayDataSet)
		{
			if (item != null)
			{
				ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation = item.Clone();
				listMetaCardViewDisplayInformation.Quantity = 1u;
				for (int i = 0; i < item.Quantity; i++)
				{
					flattenedDisplayInfoList.Add(listMetaCardViewDisplayInformation);
				}
			}
		}
		return flattenedDisplayInfoList;
	}

	public static List<ListMetaCardViewDisplayInformation> Unflatten(IEnumerable<ListMetaCardViewDisplayInformation> displayDataSet)
	{
		List<ListMetaCardViewDisplayInformation> list = new List<ListMetaCardViewDisplayInformation>();
		foreach (ListMetaCardViewDisplayInformation displayData in displayDataSet)
		{
			int num = list.FindIndex((ListMetaCardViewDisplayInformation data) => data.IsInstanceOf(displayData));
			if (num == -1)
			{
				list.Add(displayData.Clone());
				continue;
			}
			ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation = list[num];
			listMetaCardViewDisplayInformation.Quantity += displayData.Quantity;
			list[num] = listMetaCardViewDisplayInformation;
		}
		return list;
	}

	public static bool TryGetDisplayData(this List<ListMetaCardViewDisplayInformation> displayDataList, MetaCardView cardView, out ListMetaCardViewDisplayInformation displayData)
	{
		displayData = displayDataList.Find((ListMetaCardViewDisplayInformation data) => data.Card.GrpId == cardView.Card.GrpId);
		if (displayData.Card != null)
		{
			return displayData.Card.GrpId != 0;
		}
		return false;
	}

	public static bool TryGetDisplayDataIndex(this List<ListMetaCardViewDisplayInformation> displayDataList, MetaCardView cardView, out int index)
	{
		ListMetaCardView_Expanding listCardView = cardView as ListMetaCardView_Expanding;
		if ((object)listCardView != null)
		{
			index = displayDataList.FindIndex((ListMetaCardViewDisplayInformation data) => data.IsInstanceOf(listCardView.DisplayInformation));
			if (index != -1 && displayDataList[index].Card != null)
			{
				return displayDataList[index].Card.GrpId != 0;
			}
			return false;
		}
		index = -1;
		return false;
	}

	public static List<ListMetaCardViewDisplayInformation> Clean(List<ListMetaCardViewDisplayInformation> displayDataSet)
	{
		List<InfoAndIndex> list = new List<InfoAndIndex>();
		int i = 0;
		while (i < displayDataSet.Count)
		{
			ListMetaCardViewDisplayInformation displayData = displayDataSet[i];
			if (displayData.Quantity != 0)
			{
				List<InfoAndIndex> list2 = list.FindAll((InfoAndIndex data) => data.Info.IsInstanceOf(displayData));
				if (list2.Count == 0)
				{
					list.Add(new InfoAndIndex(displayData.Clone(), i));
				}
				else
				{
					InfoAndIndex infoAndIndex = list2.Find(delegate(InfoAndIndex iai)
					{
						int index = iai.Index;
						int start = Math.Min(i, index);
						int length = Math.Abs(i - index) + 1;
						return displayDataSet.Slice(start, length)?.All((ListMetaCardViewDisplayInformation item) => item.IsInstanceOf(displayData)) ?? false;
					});
					if (infoAndIndex != null)
					{
						infoAndIndex.Info.Quantity += displayData.Quantity;
					}
					else
					{
						list.Add(new InfoAndIndex(displayData.Clone(), i));
					}
				}
			}
			int num = i + 1;
			i = num;
		}
		return list.Select((InfoAndIndex pair) => pair.Info).ToList();
	}

	public static List<ListMetaCardViewDisplayInformation> GetDisplayDataSubset(this List<ListMetaCardViewDisplayInformation> displayDataSuperset, uint startIndex, uint maxCount)
	{
		List<ListMetaCardViewDisplayInformation> list = new List<ListMetaCardViewDisplayInformation>();
		uint num = 0u;
		uint num2 = 0u;
		foreach (ListMetaCardViewDisplayInformation item in displayDataSuperset)
		{
			if (startIndex > num)
			{
				num += item.Quantity;
				if (startIndex < num)
				{
					ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation = item.Clone();
					listMetaCardViewDisplayInformation.Quantity = num - startIndex;
					list.Add(listMetaCardViewDisplayInformation);
					num2 = listMetaCardViewDisplayInformation.Quantity;
				}
			}
			else if (num2 <= maxCount)
			{
				ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation2 = item.Clone();
				if (num2 + item.Quantity > maxCount)
				{
					listMetaCardViewDisplayInformation2.Quantity = maxCount - num2;
				}
				list.Add(listMetaCardViewDisplayInformation2);
				num2 += listMetaCardViewDisplayInformation2.Quantity;
			}
		}
		return list;
	}

	public static List<ListMetaCardViewDisplayInformation> GetDisplayDataInclusive(this List<ListMetaCardViewDisplayInformation> displayDataSuperset, List<ListMetaCardViewDisplayInformation> displayDataInclusionSet)
	{
		List<ListMetaCardViewDisplayInformation> list = new List<ListMetaCardViewDisplayInformation>();
		foreach (ListMetaCardViewDisplayInformation displayData in displayDataSuperset)
		{
			ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation = displayDataInclusionSet.Find((ListMetaCardViewDisplayInformation data) => data.IsInstanceOf(displayData));
			if (listMetaCardViewDisplayInformation != null && listMetaCardViewDisplayInformation.Quantity != 0)
			{
				ListMetaCardViewDisplayInformation item = ((listMetaCardViewDisplayInformation.Quantity >= displayData.Quantity) ? displayData : listMetaCardViewDisplayInformation);
				list.Add(item);
			}
		}
		return list;
	}

	public static List<ListMetaCardViewDisplayInformation> CombineDisplayData(this List<ListMetaCardViewDisplayInformation> setDisplayData, List<ListMetaCardViewDisplayInformation> appendDisplayData)
	{
		foreach (ListMetaCardViewDisplayInformation appendDisplayDatum in appendDisplayData)
		{
			if (appendDisplayDatum.Quantity != 0)
			{
				setDisplayData.Add(appendDisplayDatum);
			}
		}
		return setDisplayData;
	}

	public static List<ListMetaCardViewDisplayInformation> GetDisplayDataExclusive(this List<ListMetaCardViewDisplayInformation> displayDataSuperset, List<ListMetaCardViewDisplayInformation> displayDataExclusionSet)
	{
		List<ListMetaCardViewDisplayInformation> list = new List<ListMetaCardViewDisplayInformation>();
		foreach (ListMetaCardViewDisplayInformation displayDataFromSuperset in displayDataSuperset)
		{
			uint num = displayDataExclusionSet.Find((ListMetaCardViewDisplayInformation displayInformation) => displayInformation.IsInstanceOf(displayDataFromSuperset))?.Quantity ?? 0;
			if (num == 0)
			{
				list.Add(displayDataFromSuperset.Clone());
			}
			else if (num != displayDataFromSuperset.Quantity)
			{
				ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation = displayDataFromSuperset.Clone();
				listMetaCardViewDisplayInformation.Quantity = displayDataFromSuperset.Quantity - num;
				list.Add(listMetaCardViewDisplayInformation);
			}
		}
		return list;
	}
}

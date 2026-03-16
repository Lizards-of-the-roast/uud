using System.Collections.Generic;
using System.Linq;
using Core.Meta.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN.Objectives;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace ProfileUI;

public class RewardBar : MonoBehaviour
{
	[SerializeField]
	private Image Badge;

	[SerializeField]
	private List<Sprite> BadgeSprites;

	[SerializeField]
	private Localize HeaderText;

	[SerializeField]
	private List<Localize> RewardsText;

	[SerializeField]
	private List<Image> RewardsImages;

	private readonly List<AssetLoader.AssetTracker<Sprite>> _rewardsImageSpriteTrackers = new List<AssetLoader.AssetTracker<Sprite>>();

	public void Initialize(RewardLevels level, Client_ChestData cd)
	{
		for (int i = 0; i < RewardsImages.Count; i++)
		{
			_rewardsImageSpriteTrackers.Add(new AssetLoader.AssetTracker<Sprite>($"RewardBarImageSprite{i}"));
		}
		foreach (Localize item in RewardsText)
		{
			item.gameObject.UpdateActive(active: false);
		}
		foreach (Image rewardsImage in RewardsImages)
		{
			rewardsImage.transform.parent.gameObject.UpdateActive(active: false);
		}
		string text = (string.IsNullOrWhiteSpace(cd.Image1) ? null : ServerRewardUtils.FormatAssetFromServerReference(cd.Image1, ServerRewardFileExtension.PNG));
		string text2 = (string.IsNullOrWhiteSpace(cd.Image2) ? null : ServerRewardUtils.FormatAssetFromServerReference(cd.Image2, ServerRewardFileExtension.PNG));
		string text3 = (string.IsNullOrWhiteSpace(cd.Image3) ? null : ServerRewardUtils.FormatAssetFromServerReference(cd.Image3, ServerRewardFileExtension.PNG));
		Badge.sprite = BadgeSprites[(int)level];
		HeaderText.TextTarget.locKey = cd.HeaderLocKey;
		string[] array = cd.LocParams?.Values.ToArray() ?? new string[0];
		string[] array2 = cd.DescriptionLocKey.Split(' ');
		for (int j = 0; j < array2.Length; j++)
		{
			RewardsText[j].gameObject.SetActive(value: true);
			if (array.Length > j)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("number1", array[j]);
				dictionary.Add("quantity", array[j]);
				RewardsText[j].SetText(array2[j], dictionary);
			}
			else
			{
				RewardsText[j].SetText(array2[j]);
			}
		}
		int quantity = cd.Quantity;
		if (!string.IsNullOrEmpty(text))
		{
			for (int k = 0; k < quantity; k++)
			{
				RewardsImages[k].transform.parent.gameObject.SetActive(value: true);
				AssetLoaderUtils.TrySetSprite(RewardsImages[k], _rewardsImageSpriteTrackers[k], text);
				FormatImage(RewardsImages[k].gameObject, cd.Image1);
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			RewardsImages[quantity].transform.parent.gameObject.SetActive(value: true);
			AssetLoaderUtils.TrySetSprite(RewardsImages[quantity], _rewardsImageSpriteTrackers[quantity], text2);
			FormatImage(RewardsImages[quantity].gameObject, cd.Image2);
		}
		if (string.IsNullOrEmpty(text3) || array.Length <= 2)
		{
			return;
		}
		int num = quantity + 1;
		if (!int.TryParse(array[2], out var result))
		{
			return;
		}
		for (int l = 0; l < result; l++)
		{
			int num2 = num + l;
			if (num2 < RewardsImages.Count())
			{
				RewardsImages[num2].transform.parent.gameObject.SetActive(value: true);
				AssetLoaderUtils.TrySetSprite(RewardsImages[num2], _rewardsImageSpriteTrackers[num2], text3);
				FormatImage(RewardsImages[num2].gameObject, cd.Image3);
				continue;
			}
			Debug.LogError("Attempting to display too many season rewards (attepting to display " + (num2 + 1) + " of a possible " + RewardsImages.Count() + " entries)");
		}
	}

	private void FormatImage(GameObject go, string name)
	{
		LayoutElement component = go.transform.parent.GetComponent<LayoutElement>();
		RectTransform component2 = go.GetComponent<RectTransform>();
		switch (name)
		{
		case "ObjectiveIcon_Pack_M20":
			if ((bool)component)
			{
				component.preferredWidth = 60f;
			}
			if ((bool)component2)
			{
				component2.localPosition = new Vector3(-2f, 4f, 0f);
				component2.sizeDelta = new Vector2(140f, 140f);
				component2.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			}
			break;
		case "ObjectiveIcon_CoinsLarge":
		case "ObjectiveIcon_CoinsSmall":
			if ((bool)component)
			{
				component.preferredWidth = 89.2f;
			}
			if ((bool)component2)
			{
				component2.localPosition = new Vector3(0f, 13f, 0f);
				component2.sizeDelta = new Vector2(140f, 140f);
				component2.localScale = new Vector3(0.85f, 0.85f, 0.85f);
			}
			break;
		case "ObjectiveIcon_PremiumSkin":
			if ((bool)component)
			{
				component.preferredWidth = 70f;
			}
			if ((bool)component2)
			{
				component2.localPosition = new Vector3(0f, 4f, 0f);
				component2.sizeDelta = new Vector2(120f, 180f);
				component2.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			}
			break;
		case "ObjectiveIcon_Cards_A1B0":
		case "ObjectiveIcon_Cards_A0B1":
			if ((bool)component)
			{
				component.preferredWidth = 83.6f;
			}
			if ((bool)component2)
			{
				component2.localPosition = new Vector3(0f, -2f, 0f);
				component2.sizeDelta = new Vector2(140f, 140f);
				component2.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			}
			break;
		}
	}

	public void OnDestroy()
	{
		foreach (Image rewardsImage in RewardsImages)
		{
			rewardsImage.sprite = null;
		}
		foreach (AssetLoader.AssetTracker<Sprite> rewardsImageSpriteTracker in _rewardsImageSpriteTrackers)
		{
			rewardsImageSpriteTracker.Cleanup();
		}
		_rewardsImageSpriteTrackers.Clear();
	}
}

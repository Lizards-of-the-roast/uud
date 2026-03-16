using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace MTGA.Loc;

[Serializable]
public class MTGALocalizable
{
	[Serializable]
	public enum TargetType
	{
		None = -1,
		Text = 1,
		Font = 2
	}

	[Serializable]
	public class LocParam
	{
		public string key;

		public string value;
	}

	private static readonly Dictionary<string, string> Parameters = new Dictionary<string, string>();

	[SerializeField]
	public bool active = true;

	[SerializeField]
	public TargetType targetType;

	[SerializeField]
	public string locKey;

	[SerializeField]
	public Component serializedCmp;

	[SerializeField]
	public List<LocParam> locParams;

	[NonSerialized]
	public MTGALocalizedString LocalizedString;

	public void Localize(IClientLocProvider locProvider, IFontProvider fontProvider, string fallbackText = "", string materialKey = null)
	{
		if (!active || !(serializedCmp is TMP_Text tMP_Text))
		{
			return;
		}
		switch (targetType)
		{
		case TargetType.Text:
			LocalizeText(locProvider, tMP_Text, fallbackText);
			break;
		case TargetType.Font:
		{
			FontMaterialMap localizedFont = fontProvider.GetLocalizedFont(locKey);
			if (localizedFont == null)
			{
				break;
			}
			tMP_Text.font = localizedFont.font;
			if (!string.IsNullOrEmpty(materialKey))
			{
				Material material = localizedFont.GetMaterial(materialKey);
				if ((bool)material)
				{
					tMP_Text.fontSharedMaterial = material;
				}
			}
			break;
		}
		default:
			Debug.LogError($"unsupported localization target type: {targetType}");
			break;
		}
	}

	private void LocalizeText(IClientLocProvider clientLocManager, TMP_Text label, string fallbackText)
	{
		if (LocalizedString != null)
		{
			label.SetText(LocalizedString);
			return;
		}
		Parameters.Clear();
		if (locParams != null)
		{
			foreach (LocParam locParam in locParams)
			{
				Parameters[locParam.key] = locParam.value;
			}
		}
		if (!clientLocManager.TryGetLocalizedTextForLanguage(locKey, Languages.CurrentLanguage, Parameters.AsTuples(), out var loc) && !string.IsNullOrEmpty(fallbackText))
		{
			OverrideText(fallbackText);
		}
		else
		{
			label.SetText(loc);
		}
	}

	public void OverrideText(string text)
	{
		if (targetType == TargetType.Text)
		{
			if (serializedCmp is TMP_Text tMP_Text)
			{
				tMP_Text.SetText(text);
			}
		}
		else
		{
			Debug.LogWarning("Trying to Override Localized text on non Text Object");
		}
	}
}

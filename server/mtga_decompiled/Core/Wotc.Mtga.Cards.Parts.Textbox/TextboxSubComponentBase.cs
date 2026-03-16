using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public abstract class TextboxSubComponentBase : MonoBehaviour
{
	[SerializeField]
	protected TMP_Text _textLabel;

	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	protected GameObject _backgroundStripe;

	[SerializeField]
	private bool _ignoreSetAlignment;

	[SerializeField]
	protected float _minimumHeight = 0.2f;

	public RectTransform RectTransform => _rectTransform;

	public ICardTextEntry Content { get; private set; }

	private void Awake()
	{
		_rectTransform = (RectTransform)base.transform;
	}

	public virtual void CleanUp()
	{
		foreach (CDCMaterialFiller cdcFillersOnNonLabelVisual in GetCdcFillersOnNonLabelVisuals())
		{
			cdcFillersOnNonLabelVisual.Cleanup();
		}
		_textLabel.text = " ";
		_textLabel.font = null;
	}

	public virtual void SetContent(ICardTextEntry content)
	{
		Content = content;
	}

	public virtual void SetFont(TMP_FontAsset fontAsset)
	{
		_textLabel.font = fontAsset;
	}

	public virtual void SetAlignment(TextAlignmentOptions textAlignment)
	{
		if (!_ignoreSetAlignment)
		{
			_textLabel.alignment = textAlignment;
		}
	}

	public virtual void SetFontSize(float newSize)
	{
		_textLabel.fontSize = newSize;
		_textLabel.enableAutoSizing = false;
		_textLabel.ForceMeshUpdate();
	}

	public virtual void SetMaterial(Action<TMP_Text, string> updateMaterial, string overrideLanguage)
	{
		updateMaterial(_textLabel, overrideLanguage);
	}

	public virtual IEnumerable<CDCMaterialFiller> GetCdcFillersOnNonLabelVisuals()
	{
		yield break;
	}

	public virtual void UpdateVisibility(RectTransform viewportTransform)
	{
	}

	public virtual float GetPreferredHeight()
	{
		return Mathf.Max(_minimumHeight, _textLabel.preferredHeight);
	}

	public virtual void SetStripeEnabled(bool stripeEnabled)
	{
		_backgroundStripe.UpdateActive(stripeEnabled);
	}

	public virtual void SetLineSpacing(float lineSpacing)
	{
		_textLabel.lineSpacing = lineSpacing;
	}
}

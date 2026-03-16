using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class LoyaltyTableAbilityTextbox : TextboxSubComponentBase
{
	[SerializeField]
	private TMP_Text _loyaltyCost;

	[SerializeField]
	private GameObject _iconBadge;

	[SerializeField]
	private CDCMaterialFiller _materialFiller;

	[SerializeField]
	private GameObject _rowTemplate;

	private List<TableAbilityTextbox.Row> _rows = new List<TableAbilityTextbox.Row>();

	private float _preferredHeight;

	public override void SetContent(ICardTextEntry content)
	{
		if (!(content is ILoyaltyTextEntry loyaltyTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		if (!(content is ITableTextEntry tableTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		base.SetContent(content);
		_loyaltyCost.text = loyaltyTextEntry.GetCost();
		_iconBadge.UpdateActive(active: true);
		_textLabel.text = tableTextEntry.Preamble;
		for (int i = 0; i < tableTextEntry.Rows.Length; i++)
		{
			if (i >= _rows.Count)
			{
				_rows.Add(new TableAbilityTextbox.Row(UnityEngine.Object.Instantiate(_rowTemplate, _rowTemplate.transform.parent, worldPositionStays: false)));
			}
			TableAbilityTextbox.Row row = _rows[i];
			if ((bool)row.Textfield)
			{
				row.Textfield.text = tableTextEntry.Rows[i].FormattedText;
			}
			if ((bool)row.Stripe)
			{
				row.Stripe.enabled = i % 2 == 0;
			}
			if ((bool)row.Root)
			{
				row.Root.SetActive(value: true);
			}
		}
		for (int j = tableTextEntry.Rows.Length; j < _rows.Count; j++)
		{
			if ((bool)_rows[j].Root)
			{
				_rows[j].Root.SetActive(value: false);
			}
		}
		LayoutManually();
	}

	private void LayoutManually()
	{
		float y = _textLabel.transform.localPosition.y;
		float num = 0f + Mathf.Max(LayoutUtility.GetMinHeight(_textLabel.rectTransform), LayoutUtility.GetPreferredHeight(_textLabel.rectTransform));
		for (int i = 0; i < _rows.Count; i++)
		{
			TableAbilityTextbox.Row row = _rows[i];
			if ((bool)row.Root && row.Root.activeSelf && (bool)row.Textfield)
			{
				row.Root.transform.localPosition = new Vector3(row.Root.transform.localPosition.x, y - num, row.Root.transform.localPosition.z);
				num += row.Textfield.preferredHeight;
			}
		}
		_preferredHeight = num;
	}

	public override void CleanUp()
	{
		base.CleanUp();
		foreach (TableAbilityTextbox.Row row in _rows)
		{
			if ((bool)row.Root)
			{
				row.Root.SetActive(value: false);
			}
		}
	}

	public virtual void OnDestroy()
	{
		_rows.Clear();
	}

	public override float GetPreferredHeight()
	{
		return _preferredHeight;
	}

	public override void SetFont(TMP_FontAsset fontAsset)
	{
		base.SetFont(fontAsset);
		foreach (TableAbilityTextbox.Row row in _rows)
		{
			if ((bool)row.Textfield)
			{
				row.Textfield.font = fontAsset;
			}
		}
	}

	public override void SetFontSize(float newSize)
	{
		base.SetFontSize(newSize);
		foreach (TableAbilityTextbox.Row row in _rows)
		{
			if ((bool)row.Textfield)
			{
				row.Textfield.fontSize = newSize;
				row.Textfield.enableAutoSizing = false;
				row.Textfield.ForceMeshUpdate();
			}
		}
		LayoutManually();
	}

	public override void SetLineSpacing(float lineSpacing)
	{
		base.SetLineSpacing(lineSpacing);
		foreach (TableAbilityTextbox.Row row in _rows)
		{
			if ((bool)row.Textfield)
			{
				row.Textfield.lineSpacing = lineSpacing;
			}
		}
	}

	public override void SetAlignment(TextAlignmentOptions textAlignment)
	{
		base.SetAlignment(textAlignment);
		foreach (TableAbilityTextbox.Row row in _rows)
		{
			if ((bool)row.Textfield)
			{
				row.Textfield.alignment = textAlignment;
			}
		}
	}

	public override void SetMaterial(Action<TMP_Text, string> updateMaterial, string overrideLanguage)
	{
		updateMaterial(_textLabel, overrideLanguage);
		foreach (TableAbilityTextbox.Row row in _rows)
		{
			if ((bool)row.Textfield)
			{
				updateMaterial(row.Textfield, overrideLanguage);
			}
		}
	}

	public override void UpdateVisibility(RectTransform viewportTransform)
	{
		Rect rect = viewportTransform.rect;
		Vector3 vector = viewportTransform.InverseTransformPoint(_iconBadge.transform.position);
		bool active = vector.y <= rect.yMax && vector.y >= rect.yMin;
		_iconBadge.UpdateActive(active);
	}

	public override IEnumerable<CDCMaterialFiller> GetCdcFillersOnNonLabelVisuals()
	{
		if ((bool)_materialFiller)
		{
			yield return _materialFiller;
		}
	}
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Text;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class TableAbilityTextbox : TextboxSubComponentBase
{
	public struct Row
	{
		public GameObject Root;

		public TMP_Text Textfield;

		public Image Stripe;

		public Row(GameObject root)
		{
			Root = root;
			Textfield = root.GetComponentInChildren<TMP_Text>();
			Stripe = root.GetComponentInChildren<Image>();
		}
	}

	[SerializeField]
	private GameObject _rowTemplate;

	private List<Row> _rows = new List<Row>();

	private float _preferredHeight;

	public override void SetContent(ICardTextEntry content)
	{
		if (!(content is TableTextEntry tableTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		base.SetContent(content);
		_textLabel.text = tableTextEntry.Preamble;
		_backgroundStripe.SetActive(value: false);
		for (int i = 0; i < tableTextEntry.Rows.Length; i++)
		{
			if (i >= _rows.Count)
			{
				_rows.Add(new Row(UnityEngine.Object.Instantiate(_rowTemplate, _rowTemplate.transform.parent, worldPositionStays: false)));
			}
			Row row = _rows[i];
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

	public override void CleanUp()
	{
		base.CleanUp();
		foreach (Row row in _rows)
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
		foreach (Row row in _rows)
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
		foreach (Row row in _rows)
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
		foreach (Row row in _rows)
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
		foreach (Row row in _rows)
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
		foreach (Row row in _rows)
		{
			if ((bool)row.Textfield)
			{
				updateMaterial(row.Textfield, overrideLanguage);
			}
		}
	}

	private void LayoutManually()
	{
		float y = _textLabel.transform.localPosition.y;
		float num = 0f + _textLabel.preferredHeight;
		for (int i = 0; i < _rows.Count; i++)
		{
			Row row = _rows[i];
			if ((bool)row.Root && row.Root.activeSelf && (bool)row.Textfield)
			{
				row.Root.transform.localPosition = new Vector3(row.Root.transform.localPosition.x, y - num, row.Root.transform.localPosition.z);
				num += row.Textfield.preferredHeight;
			}
		}
		_preferredHeight = num;
	}
}

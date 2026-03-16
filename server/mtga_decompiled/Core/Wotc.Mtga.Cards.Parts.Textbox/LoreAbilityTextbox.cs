using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class LoreAbilityTextbox : TextboxSubComponentBase
{
	[Space(5f)]
	[SerializeField]
	private Transform _chapterBadgeParent;

	[SerializeField]
	private ViewCounter _chapterBadgePrefab;

	[SerializeField]
	private float _minHeightPerChapter = 0.45f;

	private int _badgeCount;

	public override float GetPreferredHeight()
	{
		return Mathf.Max(_minHeightPerChapter * (float)_badgeCount, base.GetPreferredHeight());
	}

	public override void SetContent(ICardTextEntry content)
	{
		if (!(content is ChapterTextEntry chapterTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		base.SetContent(content);
		_textLabel.text = chapterTextEntry.GetText();
		UpdateBadges(chapterTextEntry.GetChapterNumbers());
	}

	private void UpdateBadges(IReadOnlyList<uint> chapterNumbers)
	{
		while (_chapterBadgeParent.childCount < chapterNumbers.Count)
		{
			GameObject obj = UnityEngine.Object.Instantiate(_chapterBadgePrefab.gameObject, _chapterBadgeParent);
			obj.SetLayer(base.gameObject.layer);
			obj.transform.ZeroOut();
		}
		_badgeCount = chapterNumbers.Count;
		for (int i = 0; i < _chapterBadgeParent.childCount; i++)
		{
			ViewCounter component = _chapterBadgeParent.GetChild(i).GetComponent<ViewCounter>();
			component.SetCount((i < chapterNumbers.Count) ? chapterNumbers[i] : 0u);
			component.gameObject.UpdateActive(i < chapterNumbers.Count);
		}
	}

	public override void UpdateVisibility(RectTransform viewportTransform)
	{
		for (int i = 0; i < _badgeCount; i++)
		{
			Transform child = _chapterBadgeParent.GetChild(i);
			Rect rect = viewportTransform.rect;
			Vector3 vector = viewportTransform.InverseTransformPoint(child.position);
			bool active = vector.y < rect.yMax && vector.y > rect.yMin;
			child.gameObject.UpdateActive(active);
		}
	}
}

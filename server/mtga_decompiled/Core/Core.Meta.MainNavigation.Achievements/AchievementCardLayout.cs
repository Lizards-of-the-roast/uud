using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementCardLayout : UIBehaviour
{
	[SerializeField]
	private GameObject _horizontalLayoutGroupPrefab;

	private List<HorizontalLayoutGroup> _collectionOfGroups;

	protected override void Start()
	{
		base.Start();
		StartCoroutine(PopulateChildren());
	}

	private IEnumerator PopulateChildren()
	{
		(Transform cardTransform, float cardWidth)[] achievementCards = new(Transform, float)[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			achievementCards[i] = (cardTransform: base.transform.GetChild(i), cardWidth: ((RectTransform)base.transform.GetChild(i)).rect.width);
		}
		HorizontalLayoutGroup curLayoutGroup = Object.Instantiate(_horizontalLayoutGroupPrefab, base.transform).GetComponent<HorizontalLayoutGroup>();
		yield return new WaitUntil(() => !CanvasUpdateRegistry.IsRebuildingLayout());
		float width = ((RectTransform)base.transform).rect.width;
		float num = 0f;
		for (int num2 = 0; num2 < achievementCards.Length; num2++)
		{
			if (num + achievementCards[num2].cardWidth >= width)
			{
				curLayoutGroup = Object.Instantiate(_horizontalLayoutGroupPrefab, base.transform).GetComponent<HorizontalLayoutGroup>();
				num = 0f;
			}
			num += achievementCards[num2].cardWidth;
			achievementCards[num2].cardTransform.SetParent(curLayoutGroup.transform);
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

public class BoosterSubLayout : MonoBehaviour
{
	[SerializeField]
	private int _maxRows = 4;

	[SerializeField]
	private float _cardScale = 30f;

	[SerializeField]
	private Vector2 _offset = new Vector2(160f, 200f);

	public Vector2 Size { get; private set; }

	public void LayoutCards(List<BoosterMetaCardView> allCards)
	{
		if (allCards.Count == 0)
		{
			Size = new Vector2(0f, 0f);
			return;
		}
		int num = Mathf.Min(allCards.Count, _maxRows);
		int num2 = Mathf.CeilToInt((float)allCards.Count / (float)_maxRows);
		Size = new Vector2(_offset.x * (float)num2, _offset.y * (float)num);
		Vector2 vector = new Vector2(_offset.x * (float)(num2 - 1) * -0.5f, _offset.y * (float)(num - 1) * 0.5f);
		int num3 = Mathf.Min(allCards.Count, _maxRows) - (num * num2 - allCards.Count);
		float num4 = _offset.y * (float)(num3 - 1) * 0.5f;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < allCards.Count; i++)
		{
			Transform obj = allCards[i].transform;
			int num5 = i % _maxRows;
			int num6 = i / _maxRows;
			bool num7 = num6 == num2 - 1;
			zero.x = vector.x + _offset.x * (float)num6;
			if (num7)
			{
				zero.y = num4 - _offset.y * (float)num5;
			}
			else
			{
				zero.y = vector.y - _offset.y * (float)num5;
			}
			obj.localPosition = zero;
			obj.localScale = _cardScale * Vector3.one;
		}
	}
}

using UnityEngine;
using UnityEngine.UI;

public class ObjectiveProgressBar : MonoBehaviour
{
	[SerializeField]
	private Image _barImage;

	[SerializeField]
	private GameObject _sparkGameObject;

	public void SetPct(float pct, bool forceVisible = false)
	{
		_barImage.fillAmount = pct;
		float localXPos = GetLocalXPos(pct);
		_sparkGameObject.transform.localPosition = new Vector3(localXPos, _sparkGameObject.transform.localPosition.y, _sparkGameObject.transform.localPosition.z);
		if (forceVisible)
		{
			Color color = _barImage.color;
			color.a = 1f;
			_barImage.color = color;
		}
		else if (pct >= 0.5f)
		{
			Color color2 = _barImage.color;
			color2.a = Mathf.Abs(1f - pct) / 0.5f;
			_barImage.color = color2;
		}
	}

	public float GetLocalXPos(float pct)
	{
		RectTransform component = GetComponent<RectTransform>();
		float width = component.rect.width;
		return component.rect.xMin + width * pct;
	}

	public float GetSparkXPos()
	{
		return _sparkGameObject.transform.position.x;
	}

	public void EnableSpark(bool isEnabled)
	{
		if (isEnabled)
		{
			Color color = _barImage.color;
			color.a = 1f;
			_barImage.color = color;
		}
		if (_sparkGameObject != null)
		{
			_sparkGameObject.SetActive(isEnabled);
		}
	}
}

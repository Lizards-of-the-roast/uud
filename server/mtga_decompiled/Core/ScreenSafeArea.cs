using UnityEngine;
using Wizards.Mtga.Platforms;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class ScreenSafeArea : MonoBehaviour
{
	private RectTransform rectTransform;

	public bool ConformX = true;

	public bool ConformY = true;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	private void OnEnable()
	{
		ScreenEventController.Instance.OnSafeAreaChanged += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		if (ScreenEventController.Instance != null)
		{
			ScreenEventController.Instance.OnSafeAreaChanged -= Refresh;
		}
	}

	public void Refresh(Rect safeArea)
	{
		ApplySafeArea(safeArea);
	}

	public void Refresh()
	{
		Rect safeArea = ScreenEventController.Instance.GetSafeArea();
		ApplySafeArea(safeArea);
	}

	private void ApplySafeArea(Rect safeArea)
	{
		if (!ConformX)
		{
			safeArea.x = 0f;
			safeArea.width = Screen.width;
		}
		if (!ConformY)
		{
			safeArea.y = 0f;
			safeArea.height = Screen.height;
		}
		if (Screen.width > 0 && Screen.height > 0)
		{
			Vector2 position = safeArea.position;
			Vector2 anchorMax = safeArea.position + safeArea.size;
			position.x /= Screen.width;
			position.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;
			rectTransform.anchorMin = position;
			rectTransform.anchorMax = anchorMax;
		}
	}
}

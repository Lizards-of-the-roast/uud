using System;
using UnityEngine;

namespace Wizards.Mtga.Platforms;

public class ScreenEventController : MonoBehaviour
{
	private Rect lastSafeArea = Rect.zero;

	private Vector2 lastResolution = Vector2.zero;

	private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

	private static ScreenEventController instance;

	private static bool quittingApp;

	public static ScreenEventController Instance
	{
		get
		{
			if (instance == null && !quittingApp)
			{
				GameObject obj = new GameObject("ScreenEventController");
				UnityEngine.Object.DontDestroyOnLoad(obj);
				instance = obj.AddComponent(typeof(ScreenEventController)) as ScreenEventController;
			}
			return instance;
		}
	}

	public event Action OnScreenChanged;

	public event Action<Rect> OnSafeAreaChanged;

	private void Update()
	{
		if (CheckSafeAreaChanged() || CheckOrientationChanged() || CheckResolutionChanged())
		{
			this.OnScreenChanged?.Invoke();
		}
	}

	public virtual Rect GetSafeArea()
	{
		return Screen.safeArea;
	}

	private bool CheckSafeAreaChanged()
	{
		Rect safeArea = GetSafeArea();
		if (safeArea != lastSafeArea)
		{
			lastSafeArea = safeArea;
			this.OnSafeAreaChanged?.Invoke(safeArea);
			return true;
		}
		return false;
	}

	private bool CheckOrientationChanged()
	{
		ScreenOrientation orientation = Screen.orientation;
		if (orientation != lastOrientation)
		{
			lastOrientation = orientation;
			return true;
		}
		return false;
	}

	private bool CheckResolutionChanged()
	{
		if ((float)Screen.width != lastResolution.x || (float)Screen.height != lastResolution.y)
		{
			lastResolution.x = Screen.width;
			lastResolution.y = Screen.height;
			return true;
		}
		return false;
	}

	private void OnApplicationQuit()
	{
		quittingApp = true;
	}
}

using UnityEngine;

public class CameraResolution : MonoBehaviour
{
	private int ScreenSizeX;

	private int ScreenSizeY;

	private void RescaleCamera()
	{
		if (Screen.width != ScreenSizeX || Screen.height != ScreenSizeY)
		{
			float num = 1.7777778f;
			float num2 = (float)Screen.width / (float)Screen.height / num;
			Camera component = GetComponent<Camera>();
			if (num2 < 1f)
			{
				Rect rect = component.rect;
				rect.width = 1f;
				rect.height = num2;
				rect.x = 0f;
				rect.y = (1f - num2) / 2f;
				component.rect = rect;
			}
			else
			{
				float num3 = 1f / num2;
				Rect rect2 = component.rect;
				rect2.width = num3;
				rect2.height = 1f;
				rect2.x = (1f - num3) / 2f;
				rect2.y = 0f;
				component.rect = rect2;
			}
			ScreenSizeX = Screen.width;
			ScreenSizeY = Screen.height;
		}
	}

	private void OnPreCull()
	{
		if (!Application.isEditor)
		{
			Rect rect = Camera.main.rect;
			Rect rect2 = new Rect(0f, 0f, 1f, 1f);
			Camera.main.rect = rect2;
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			Camera.main.rect = rect;
		}
	}

	private void Start()
	{
		RescaleCamera();
	}

	private void Update()
	{
		RescaleCamera();
	}
}

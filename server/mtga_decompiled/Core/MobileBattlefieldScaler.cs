using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Mtga.Platforms;

public class MobileBattlefieldScaler : MonoBehaviour
{
	[Serializable]
	public class AspectRatioScaleMapping
	{
		public float aspectRatio;

		public Vector3 expectedScale;

		public AspectRatioScaleMapping(float ratio, Vector3 scale)
		{
			aspectRatio = ratio;
			expectedScale = scale;
		}
	}

	[Tooltip("Any aspect ratio under this value will use a scale of 1, 1, 1. Any value above will use the mappings")]
	[SerializeField]
	private float idealAspectRatio = 1.778f;

	[Tooltip("Scales the battlefield up to compensate when the center is moved for notched devices")]
	[SerializeField]
	private float safeAreaOffsetX = 0.72f;

	[Tooltip("Moves the battlefield in the Z direction after the battlefield is scaled for notch")]
	[SerializeField]
	private float safeAreaOffsetZ;

	public AspectRatioScaleMapping[] expectedScaleMappings = new AspectRatioScaleMapping[12]
	{
		new AspectRatioScaleMapping(1.778f, Vector3.one),
		new AspectRatioScaleMapping(1.8f, new Vector3(1.03f, 1f, 1.03f)),
		new AspectRatioScaleMapping(1.87f, new Vector3(1.09f, 1f, 1.09f)),
		new AspectRatioScaleMapping(1.93f, new Vector3(1.11f, 1f, 1.11f)),
		new AspectRatioScaleMapping(2f, new Vector3(1.15f, 1f, 1.15f)),
		new AspectRatioScaleMapping(2.06f, new Vector3(1.235f, 1f, 1.235f)),
		new AspectRatioScaleMapping(2.16f, new Vector3(1.264f, 1f, 1.264f)),
		new AspectRatioScaleMapping(2.27f, new Vector3(1.374f, 1f, 1.374f)),
		new AspectRatioScaleMapping(2.5f, new Vector3(1.49f, 1f, 1.49f)),
		new AspectRatioScaleMapping(2.63f, new Vector3(1.58f, 1f, 1.58f)),
		new AspectRatioScaleMapping(2.77f, new Vector3(1.67f, 1f, 1.67f)),
		new AspectRatioScaleMapping(3f, new Vector3(1.85f, 1f, 1.85f))
	};

	private Camera _camera;

	private void Start()
	{
		ScaleBattlefield();
		SceneManager.sceneLoaded += OnSceneLoaded;
		ScreenEventController.Instance.OnSafeAreaChanged += UpdateBattlefield;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (ScreenEventController.Instance != null)
		{
			ScreenEventController.Instance.OnSafeAreaChanged -= UpdateBattlefield;
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		UpdateBattlefield(ScreenEventController.Instance.GetSafeArea());
	}

	private void UpdateBattlefield()
	{
		ScaleBattlefield();
		AdjustForSafeArea(ScreenEventController.Instance.GetSafeArea());
	}

	private void UpdateBattlefield(Rect safeArea)
	{
		ScaleBattlefield();
		AdjustForSafeArea(safeArea);
	}

	private void ScaleBattlefield()
	{
		base.transform.localScale = Vector3.one;
		float aspectRatio = (float)Screen.width / (float)Screen.height;
		base.transform.localScale = GetScaleForAspectRatio(aspectRatio);
	}

	private void AdjustForSafeArea(Rect safeArea)
	{
		if (_camera == null)
		{
			_camera = Camera.main;
		}
		if (!(_camera == null))
		{
			float num = Vector3.Distance(_camera.transform.position, Vector3.zero);
			Vector3 vector = _camera.ScreenToWorldPoint(new Vector3(safeArea.xMin, (float)Screen.height / 2f, _camera.nearClipPlane + num));
			Vector3 vector2 = _camera.ScreenToWorldPoint(new Vector3(safeArea.xMax, (float)Screen.height / 2f, _camera.nearClipPlane + num));
			Vector3 zero = Vector3.zero;
			zero.x = (vector.x + vector2.x) / 2f;
			float num2 = Mathf.Max(safeArea.xMin / (float)Screen.width, Mathf.Abs(safeArea.xMax / (float)Screen.width - 1f));
			zero = new Vector3(zero.x, zero.y, num2 * safeAreaOffsetZ);
			base.transform.position = zero;
			base.transform.localScale += num2 * safeAreaOffsetX * Vector3.one;
		}
	}

	private Vector3 GetScaleForAspectRatio(float aspectRatio)
	{
		if (expectedScaleMappings.Length == 0 || aspectRatio <= idealAspectRatio)
		{
			return Vector3.one;
		}
		if (expectedScaleMappings.Length == 1)
		{
			return expectedScaleMappings.First().expectedScale;
		}
		expectedScaleMappings.OrderBy((AspectRatioScaleMapping mapping) => mapping.aspectRatio);
		AspectRatioScaleMapping aspectRatioScaleMapping = expectedScaleMappings.Aggregate((AspectRatioScaleMapping x, AspectRatioScaleMapping y) => (!(Mathf.Abs(x.aspectRatio - aspectRatio) < Mathf.Abs(y.aspectRatio - aspectRatio))) ? y : x);
		if (Mathf.Approximately(aspectRatio, aspectRatioScaleMapping.aspectRatio))
		{
			return aspectRatioScaleMapping.expectedScale;
		}
		Vector3 vector;
		if (aspectRatio > aspectRatioScaleMapping.aspectRatio && aspectRatioScaleMapping.aspectRatio == expectedScaleMappings.Last().aspectRatio)
		{
			if (aspectRatioScaleMapping == expectedScaleMappings.Last())
			{
				AspectRatioScaleMapping aspectRatioScaleMapping2 = expectedScaleMappings[expectedScaleMappings.Length - 2];
				vector = aspectRatioScaleMapping.expectedScale - aspectRatioScaleMapping2.expectedScale;
			}
			else
			{
				AspectRatioScaleMapping aspectRatioScaleMapping2 = expectedScaleMappings[Array.IndexOf(expectedScaleMappings, aspectRatioScaleMapping) + 1];
				vector = aspectRatioScaleMapping2.expectedScale - aspectRatioScaleMapping.expectedScale;
			}
		}
		else if (aspectRatioScaleMapping.aspectRatio == expectedScaleMappings.First().aspectRatio)
		{
			AspectRatioScaleMapping aspectRatioScaleMapping2 = expectedScaleMappings[1];
			vector = aspectRatioScaleMapping2.expectedScale - aspectRatioScaleMapping.expectedScale;
		}
		else
		{
			AspectRatioScaleMapping aspectRatioScaleMapping2 = expectedScaleMappings[Array.IndexOf(expectedScaleMappings, aspectRatioScaleMapping) - 1];
			vector = aspectRatioScaleMapping.expectedScale - aspectRatioScaleMapping2.expectedScale;
		}
		return aspectRatioScaleMapping.expectedScale + vector * (aspectRatio - aspectRatioScaleMapping.aspectRatio);
	}

	private void OnDrawGizmos()
	{
		if (!(_camera == null))
		{
			DrawCameraGizmos();
		}
	}

	private void DrawCameraGizmos()
	{
		Plane plane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));
		Vector3 vector = CalculateScreenCorner(plane, new Vector3(0f, 0f, 0f));
		Vector3 to = CalculateScreenCorner(plane, new Vector3(0f, 1f, 0f));
		Vector3 vector2 = CalculateScreenCorner(plane, new Vector3(1f, 0f, 0f));
		Vector3 vector3 = CalculateScreenCorner(plane, new Vector3(1f, 1f, 0f));
		Gizmos.color = Color.red;
		Gizmos.DrawLine(vector, to);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector2, vector);
		Gizmos.DrawLine(vector3, to);
	}

	private Vector3 CalculateScreenCorner(Plane plane, Vector3 screenPos)
	{
		Ray ray = _camera.ViewportPointToRay(screenPos);
		if (plane.Raycast(ray, out var enter))
		{
			return ray.GetPoint(enter);
		}
		return Vector3.zero;
	}
}

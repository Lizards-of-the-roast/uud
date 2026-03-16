using UnityEngine;

public class BoneScaler : MonoBehaviour
{
	[SerializeField]
	private Transform targetBone;

	[SerializeField]
	private float scale = 1f;

	[SerializeField]
	private bool Adjust_Position;

	[SerializeField]
	private Vector3 position_offset = new Vector3(0f, 0f, 0f);

	private Vector3 LastFramePos = new Vector3(0f, 0f, 0f);

	private void Start()
	{
		if (targetBone != null)
		{
			ApplyScale();
			LastFramePos = targetBone.localPosition;
		}
	}

	private void OnValidate()
	{
		if (targetBone != null)
		{
			ApplyScale();
			LastFramePos = targetBone.localPosition;
		}
	}

	private void LateUpdate()
	{
		if (targetBone != null)
		{
			ApplyScale();
			if (Adjust_Position)
			{
				UpdatePos();
			}
		}
	}

	public void SetScale(float newScale)
	{
		scale = newScale;
		ApplyScale();
	}

	private void ApplyScale()
	{
		targetBone.localScale = Vector3.one * scale;
	}

	private void UpdatePos()
	{
		if (Adjust_Position)
		{
			if (LastFramePos != targetBone.localPosition)
			{
				targetBone.localPosition += position_offset;
			}
			LastFramePos = targetBone.localPosition;
		}
	}
}

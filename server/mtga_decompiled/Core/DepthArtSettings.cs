using UnityEngine;

[CreateAssetMenu(fileName = "DepthArtSettings", menuName = "ScriptableObject/CDC/Depth Art Settings", order = 0)]
public class DepthArtSettings : ScriptableObject
{
	private static DepthArtSettings _default;

	public float radiusModifier = 0.008f;

	public float maxRadius = 1.5f;

	public float minRadius = 0.2f;

	public float userOffsetModifier = 1f;

	public Vector3 userCenterPointAdjust = new Vector3(0f, 0.5f, 0f);

	public float maxUserOffset = 0.75f;

	public float dampModifier = 5f;

	public float xSpeed = 0.5f;

	public float xMag = 0.15f;

	public float ySpeed = 0.5f;

	public float yMag = 0.15f;

	public float xOffset;

	public float yOffset;

	public static DepthArtSettings Default
	{
		get
		{
			if (_default == null)
			{
				_default = ScriptableObject.CreateInstance<DepthArtSettings>();
			}
			return _default;
		}
	}
}

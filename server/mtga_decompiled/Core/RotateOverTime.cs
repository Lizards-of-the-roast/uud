using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
	public float rotationSpeedX;

	public float rotationSpeedY;

	public float rotationSpeedZ;

	private void Update()
	{
		float xAngle = rotationSpeedX * Time.deltaTime;
		float yAngle = rotationSpeedY * Time.deltaTime;
		float zAngle = rotationSpeedZ * Time.deltaTime;
		base.transform.Rotate(xAngle, yAngle, zAngle);
	}
}

using UnityEngine;

public class SpinConstant : MonoBehaviour
{
	[SerializeField]
	private Vector3 velocity = Vector3.zero;

	public Vector3 Velocity
	{
		get
		{
			return velocity;
		}
		set
		{
			velocity = value;
			base.enabled = velocity != Vector3.zero;
		}
	}

	private void Awake()
	{
		Velocity = velocity;
	}

	private void Update()
	{
		Vector3 vector = Velocity * Time.deltaTime;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		base.transform.rotation = Quaternion.Euler(eulerAngles + vector);
	}
}

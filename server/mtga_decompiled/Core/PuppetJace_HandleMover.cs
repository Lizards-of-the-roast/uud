using UnityEngine;

public class PuppetJace_HandleMover : MonoBehaviour
{
	public bool DebugMode;

	public GameObject Handle;

	public GameObject BodyRoot;

	public GameObject RestingPosition;

	public GameObject ObjectWithAnimator;

	public float HandleGrabZoneBuffer;

	private bool IsGrabbed;

	public float TriggerDistance = 6f;

	public float SmoothTime = 0.3f;

	private Vector3 Velocity = Vector3.zero;

	private float originalRadius;

	public float ColliderExpandRadius = 5f;

	private Plane plane = new Plane(Vector3.up, Vector3.zero);

	public float PlaneY = 10f;

	public float HoverWarmup = 1f;

	private Vector3 origin;

	private Animator animator;

	private float dangleCooldown;

	private void Start()
	{
		OnEnable();
	}

	private void OnEnable()
	{
		Transform transform = base.transform;
		Transform transform2 = transform;
		while (transform2.parent != null)
		{
			if (transform2.parent.name == "LocalTotemRoot")
			{
				Handle.transform.parent = transform;
				BodyRoot.transform.parent = transform;
				break;
			}
			transform2 = transform2.parent.transform;
		}
		Vector3 position = RestingPosition.transform.position;
		origin = new Vector3(position[0], PlaneY, position[2]);
		animator = ObjectWithAnimator.GetComponent<Animator>();
		originalRadius = Handle.GetComponent<CapsuleCollider>().radius;
	}

	private void LateUpdate()
	{
		dangleCooldown += Time.deltaTime;
		Vector3 mousePosition = Input.mousePosition;
		Vector3 vector = new Vector3(0f, 0f, 0f);
		if (mousePosition.x >= 0f && mousePosition.y >= 0f && mousePosition.x < (float)Screen.width && mousePosition.y < (float)Screen.height)
		{
			Ray ray = CurrentCamera.Value.ScreenPointToRay(mousePosition);
			plane.SetNormalAndPosition(Vector3.up, new Vector3(0f, PlaneY, 0f));
			if (plane.Raycast(ray, out var enter))
			{
				vector = ray.GetPoint(enter);
			}
		}
		if (new Vector3(origin[0] - vector[0], 0f, origin[2] - vector[2]).magnitude < TriggerDistance && IsGrabbed)
		{
			if ((animator.GetCurrentAnimatorStateInfo(0).IsName("idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("mouseHover")) && dangleCooldown > HoverWarmup)
			{
				Vector3 target = new Vector3(vector[0], vector[1] + PlaneY * -1f, vector[2]);
				base.transform.position = Vector3.SmoothDamp(base.transform.position, target, ref Velocity, SmoothTime);
				animator.SetBool("mouseHover", value: true);
			}
		}
		else
		{
			IsGrabbed = false;
			base.transform.position = Vector3.SmoothDamp(base.transform.position, RestingPosition.transform.position, ref Velocity, SmoothTime);
			animator.SetBool("mouseHover", value: false);
		}
	}

	public void HandleEnter()
	{
		IsGrabbed = true;
		dangleCooldown = 0f;
		Handle.GetComponent<CapsuleCollider>().radius = ColliderExpandRadius;
	}

	public void HandleExit()
	{
		IsGrabbed = false;
		base.transform.position = Vector3.SmoothDamp(base.transform.position, RestingPosition.transform.position, ref Velocity, SmoothTime);
		dangleCooldown = 0f;
		Handle.GetComponent<CapsuleCollider>().radius = originalRadius;
	}

	public void MouseClick1()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("mouseHover"))
		{
			animator.SetTrigger("mouseHoverExit");
		}
		animator.SetBool("mouseClick1", value: true);
	}

	public void MouseClick2()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("mouseHover"))
		{
			animator.SetTrigger("mouseHoverExit");
		}
		animator.SetBool("mouseClick2", value: true);
	}

	public void MouseClick3()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("mouseHover"))
		{
			animator.SetTrigger("mouseHoverExit");
		}
		animator.SetBool("mouseClick3", value: true);
	}
}

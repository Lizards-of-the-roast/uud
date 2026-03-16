using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererDynamicPoints : MonoBehaviour
{
	public GameObject ParentSpace;

	[SerializeField]
	private Transform[] points;

	private LineRenderer lineRenderer;

	private Vector3[] positions;

	private void OnEnable()
	{
		lineRenderer = base.gameObject.GetComponent<LineRenderer>();
		if (ParentSpace != null)
		{
			base.gameObject.transform.parent = ParentSpace.transform;
		}
		else
		{
			base.gameObject.transform.parent = null;
		}
		base.gameObject.transform.position = new Vector3(0f, 0f, 0f);
		base.gameObject.transform.rotation = Quaternion.identity;
	}

	private void LateUpdate()
	{
		if (points != null && points.Length != 0)
		{
			positions = points.Select((Transform s) => s.position).ToArray();
			lineRenderer.SetPositions(positions);
		}
	}
}

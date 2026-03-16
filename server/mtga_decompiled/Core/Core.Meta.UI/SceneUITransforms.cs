using UnityEngine;

namespace Core.Meta.UI;

public class SceneUITransforms : MonoBehaviour
{
	[SerializeField]
	private Transform _contentParent;

	[SerializeField]
	private Transform _popupsParent;

	[SerializeField]
	private Transform _overlayParent;

	public Transform ContentParent => _contentParent;

	public Transform PopupsParent => _popupsParent;

	public Transform OverlayParent => _overlayParent;

	public static SceneUITransforms Create()
	{
		return Object.FindObjectOfType<SceneUITransforms>();
	}
}

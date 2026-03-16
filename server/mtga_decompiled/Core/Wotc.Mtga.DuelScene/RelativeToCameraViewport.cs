using UnityEngine;

namespace Wotc.Mtga.DuelScene;

[RequireComponent(typeof(CardHolderBase))]
public class RelativeToCameraViewport : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 1f)]
	private float _relativeX = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _relativeY = 0.5f;

	[SerializeField]
	private float _depthFromCamera;

	[SerializeField]
	private Vector3 _offset = Vector3.zero;

	private ICardHolder _cardHolder = NoCardHolder.Default;

	private void Awake()
	{
		_cardHolder = GetComponent<CardHolderBase>();
	}

	private void OnDestroy()
	{
		_cardHolder = NoCardHolder.Default;
	}

	public void Reposition(ICameraAdapter cameraAdapter)
	{
		base.transform.position = cameraAdapter.ViewportToWorldPoint(new Vector2(_relativeX, _relativeY), _depthFromCamera) + _offset;
		_cardHolder.LayoutNow();
	}
}

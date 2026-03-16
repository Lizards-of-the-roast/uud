using System;
using System.Collections.Generic;
using System.IO;
using AssetLookupTree;
using AssetLookupTree.Nodes;
using AssetLookupTree.Payloads.DuelScene;
using UnityEngine;

namespace Wotc.Mtga.Unity;

public class IntentionArrowTesterBehavior : MonoBehaviour
{
	private enum Placement
	{
		FixedPosition,
		FollowTransform,
		FollowMouse
	}

	[SerializeField]
	[Tooltip("When in FollowTransform mode, the start of the intention arrow will follow this Transform if defined.")]
	private Transform _startTransformToFollow;

	[SerializeField]
	[Tooltip("When in FollowTransform mode, the start of the intention arrow will be locally-offset from the StartTransformToFollow but this amount, in meters.")]
	private Vector3 _startTransformToFollowPosOffset = Vector3.zero;

	[SerializeField]
	[Tooltip("When in FollowTransform mode, the end of the intention arrow will follow this Transform if defined.")]
	private Transform _endTransformToFollow;

	[SerializeField]
	[Tooltip("When in FollowTransform mode, the end of the intention arrow will be locally-offset from the EndTransformToFollow by this amount, in meters.")]
	private Vector3 _endTransformToFollowPosOffset = Vector3.zero;

	private Dictionary<string, string> _nameToPrefabPath;

	private DreamteckIntentionArrowBehavior _dreamteckIntentionArrowBehavior;

	private Placement _startPlacement;

	private Placement _endPlacement = Placement.FollowMouse;

	private bool _alwaysUpdate;

	private ICollection<string> _arrowIds;

	private string[] _placementNames;

	private Camera _mainCamera;

	private void Awake()
	{
		_placementNames = Enum.GetNames(typeof(Placement));
		_nameToPrefabPath = new Dictionary<string, string>();
	}

	private void Start()
	{
		AssetLookupSystem assetLookupSystem = default(AssetLookupSystem);
		HashSet<IntentionLinePrefab> hashSet = new HashSet<IntentionLinePrefab>();
		if (assetLookupSystem.TreeLoader != null && assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<IntentionLinePrefab> loadedTree))
		{
			foreach (INode<IntentionLinePrefab> item in loadedTree.EnumerateNodes())
			{
				if (item is ValueNode<IntentionLinePrefab> valueNode)
				{
					hashSet.Add(valueNode.Payload);
				}
			}
		}
		foreach (IntentionLinePrefab item2 in hashSet)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (item2 != null)
			{
				empty2 = item2.PrefabPath;
				empty = Path.GetFileNameWithoutExtension(empty2);
				if (!string.IsNullOrWhiteSpace(empty) && !string.IsNullOrEmpty(empty2))
				{
					_nameToPrefabPath.Add(empty, empty2);
				}
			}
		}
	}

	private void Update()
	{
		if (!(_dreamteckIntentionArrowBehavior != null))
		{
			return;
		}
		if (_mainCamera == null)
		{
			_mainCamera = Camera.main;
		}
		if (((_mainCamera != null && _startPlacement == Placement.FollowMouse) || _endPlacement == Placement.FollowMouse) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, 1000f, -1, QueryTriggerInteraction.Ignore))
		{
			if (_startPlacement == Placement.FollowMouse)
			{
				_dreamteckIntentionArrowBehavior.SetStart(hitInfo.point);
			}
			if (_endPlacement == Placement.FollowMouse)
			{
				_dreamteckIntentionArrowBehavior.SetEnd(hitInfo.point);
			}
		}
		if (_alwaysUpdate)
		{
			UpdateStart();
			UpdateEnd();
		}
	}

	private void OnGUI()
	{
		if (_dreamteckIntentionArrowBehavior == null)
		{
			GUILayout.Label("Click to instantiate an arrow:");
			Dictionary<string, string>.KeyCollection.Enumerator enumerator = _nameToPrefabPath.Keys.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (GUILayout.Button(enumerator.Current))
				{
					if (_dreamteckIntentionArrowBehavior != null)
					{
						UnityEngine.Object.Destroy(_dreamteckIntentionArrowBehavior.gameObject);
					}
					_dreamteckIntentionArrowBehavior = AssetLoader.Instantiate<DreamteckIntentionArrowBehavior>(_nameToPrefabPath[enumerator.Current]);
				}
			}
			return;
		}
		GUILayout.Label("Modify the arrow:");
		Placement startPlacement = _startPlacement;
		GUILayout.BeginHorizontal();
		GUILayout.Label("Start Placement:");
		int num = GUILayout.Toolbar((int)startPlacement, _placementNames);
		GUILayout.EndHorizontal();
		if (startPlacement != (Placement)num)
		{
			_startPlacement = (Placement)num;
			UpdateStart();
		}
		Placement endPlacement = _endPlacement;
		GUILayout.BeginHorizontal();
		GUILayout.Label("End Placement:");
		num = GUILayout.Toolbar((int)endPlacement, _placementNames);
		GUILayout.EndHorizontal();
		if (endPlacement != (Placement)num)
		{
			_endPlacement = (Placement)num;
			UpdateEnd();
		}
		_alwaysUpdate = GUILayout.Toggle(_alwaysUpdate, "Always Update Arrow");
		if (GUILayout.Button("Destroy Arrow"))
		{
			UnityEngine.Object.Destroy(_dreamteckIntentionArrowBehavior.gameObject);
			_dreamteckIntentionArrowBehavior = null;
		}
	}

	private void UpdateStart()
	{
		if (_dreamteckIntentionArrowBehavior != null)
		{
			switch (_startPlacement)
			{
			case Placement.FixedPosition:
				_dreamteckIntentionArrowBehavior.SetStart(Vector3.zero);
				break;
			case Placement.FollowTransform:
				_dreamteckIntentionArrowBehavior.SetStart(_startTransformToFollow, _startTransformToFollowPosOffset, DreamteckIntentionArrowBehavior.Space.Local);
				break;
			default:
				throw new NotImplementedException(_startPlacement.ToString());
			case Placement.FollowMouse:
				break;
			}
		}
	}

	private void UpdateEnd()
	{
		if (_dreamteckIntentionArrowBehavior != null)
		{
			switch (_endPlacement)
			{
			case Placement.FixedPosition:
				_dreamteckIntentionArrowBehavior.SetEnd(Vector3.zero);
				break;
			case Placement.FollowTransform:
				_dreamteckIntentionArrowBehavior.SetEnd(_endTransformToFollow, _endTransformToFollowPosOffset, DreamteckIntentionArrowBehavior.Space.Local);
				break;
			default:
				throw new NotImplementedException(_endPlacement.ToString());
			case Placement.FollowMouse:
				break;
			}
		}
	}
}

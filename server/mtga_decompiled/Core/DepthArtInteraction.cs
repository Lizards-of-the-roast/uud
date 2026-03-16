using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using UnityEngine;
using Wotc.Mtga.Cards;

public class DepthArtInteraction : CDCPart
{
	public DepthArtSettings _settings;

	public GameObject socketObject;

	private Camera _camera;

	private float _timeValue;

	private Vector2 _viewDirOffset;

	private Vector2? _mousePosOverride;

	public Vector3 MousePosition
	{
		get
		{
			if (_mousePosOverride.HasValue)
			{
				return _mousePosOverride.Value;
			}
			return Input.mousePosition;
		}
	}

	private void Awake()
	{
		Init();
	}

	public void Init(Camera cam = null, CDCMaterialFiller materialFiller = null)
	{
		_timeValue = UnityEngine.Random.Range(0f, 10f);
		if (cam != null)
		{
			_camera = cam;
		}
		else
		{
			_camera = CurrentCamera.Value;
		}
		if (_settings == null)
		{
			_settings = DepthArtSettings.Default;
		}
	}

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		if (!_cachedDestroyed && _cachedModel != null)
		{
			_camera = CurrentCamera.Value;
			FindSettingsObject();
			SetAllMaterialViewDirOffsetProperty(_viewDirOffset);
		}
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		_camera = null;
		_settings = null;
	}

	private void Update()
	{
		if (_cachedDestroyed || _cachedModel == null || _camera == null || !_materialFiller)
		{
			return;
		}
		try
		{
			SetAllMaterialViewDirOffsetProperty(CalculateSimulatedViewDirOffset(_settings, MousePosition - _camera.WorldToScreenPoint(base.transform.position + _settings.userCenterPointAdjust), ref _viewDirOffset, ref _timeValue));
		}
		catch (NullReferenceException)
		{
		}
	}

	private void FindSettingsObject()
	{
		_settings = DepthArtSettings.Default;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DepthArtAnimSettings> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
			_assetLookupSystem.Blackboard.CardHolderType = _cachedCardHolderType;
			DepthArtAnimSettings payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_settings = AssetLoader.GetObjectData(payload.SettingsRef);
			}
			_assetLookupSystem.Blackboard.Clear();
		}
	}

	public Vector2 CalculateSimulatedViewDirOffset(DepthArtSettings settings, Vector2 cardToMousePointerDelta, ref Vector2 previousViewDirOffset, ref float timeValue)
	{
		float t = Time.deltaTime * settings.dampModifier;
		timeValue += Time.deltaTime;
		float t2 = Mathf.Clamp01(settings.radiusModifier * cardToMousePointerDelta.magnitude / settings.maxRadius);
		Vector2 vector = settings.userOffsetModifier * -cardToMousePointerDelta;
		vector = Vector2.Lerp(Vector2.zero, vector.normalized * settings.maxUserOffset, t2);
		Vector2 vector2 = new Vector2(Mathf.Sin(timeValue * settings.xSpeed) * settings.xMag, Mathf.Cos(timeValue * settings.ySpeed) * settings.yMag);
		Vector2 b = ((!Mathf.Approximately(settings.userOffsetModifier, 0f)) ? Vector2.Lerp(vector, vector2, t2) : vector2);
		b += new Vector2(settings.xOffset, settings.yOffset);
		return previousViewDirOffset = Vector2.Lerp(previousViewDirOffset, b, t);
	}

	public void SetAllMaterialViewDirOffsetProperty(Vector2 viewDirOffset)
	{
		_materialFiller?.SetDepthArtVectors(viewDirOffset);
	}
}

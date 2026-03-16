using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.LifeChange;
using AssetLookupTree.Payloads.Prefab;
using Pooling;
using TMPro;
using UnityEngine;
using Wizards.Mtga;

public class FlyingText : MonoBehaviour
{
	[SerializeField]
	private Vector3 flightDirection = Vector3.up;

	[SerializeField]
	private float flySpeed = 4f;

	[SerializeField]
	private float lifetime = 1f;

	[SerializeField]
	private TMP_Text damageText;

	[SerializeField]
	private AnimationCurve fadeCurve;

	private float timeLived;

	private IUnityObjectPool _objectPool;

	public static void ShowCdcDamageText(Vector3 position, int damage, IUnityObjectPool objectPool, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		FlyingText objectData = AssetLoader.GetObjectData(assetLookupSystem.GetPrefab<FlyingTextPrefab, FlyingText>());
		ShowFlyingText(position, damage, objectData, objectPool);
	}

	public static void SpawnAvatarText(Vector3 position, int life, AssetLookupSystem assetLookupSystem, IUnityObjectPool objectPool)
	{
		if (life != 0)
		{
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.LifeChange = life;
			LifeChangeText lifeChangeText = ((life <= 0) ? ((LifeChangeText)assetLookupSystem.TreeLoader.LoadTree<LifeLossText>().GetPayload(assetLookupSystem.Blackboard)) : ((LifeChangeText)assetLookupSystem.TreeLoader.LoadTree<LifeGainText>().GetPayload(assetLookupSystem.Blackboard)));
			if (lifeChangeText != null)
			{
				string relativePath = lifeChangeText.PrefabRef.RelativePath;
				ShowFlyingText(position, Math.Abs(life), relativePath, objectPool);
			}
		}
	}

	private static void ShowFlyingText(Vector3 position, int value, string prefabPath, IUnityObjectPool objectPool)
	{
		FlyingText component = objectPool.PopObject(prefabPath).GetComponent<FlyingText>();
		component.transform.SetParent(null);
		component.Initialize(position, value);
	}

	private static void ShowFlyingText(Vector3 position, int value, FlyingText prefab, IUnityObjectPool objectPool)
	{
		FlyingText component = objectPool.PopObject(prefab.gameObject).GetComponent<FlyingText>();
		component.transform.SetParent(null);
		component.Initialize(position, value);
	}

	private void Awake()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
		flightDirection = flightDirection.normalized;
		damageText.text = string.Empty;
		damageText.alpha = 0f;
	}

	public void Initialize(Vector3 position, int damage)
	{
		timeLived = 0f;
		base.transform.localEulerAngles = Vector3.zero;
		base.transform.position = position;
		base.transform.transform.localScale = Vector3.one;
		damageText.text = damage.ToString();
		damageText.alpha = fadeCurve.Evaluate(0f);
	}

	private void Update()
	{
		if (!(timeLived < 0f))
		{
			base.transform.position += flightDirection * flySpeed;
			timeLived += Time.deltaTime;
			float time = Mathf.Clamp(timeLived / lifetime, 0f, 1f);
			damageText.alpha = fadeCurve.Evaluate(time);
			if (timeLived > lifetime)
			{
				DestroyMeNow();
			}
		}
	}

	private void DestroyMeNow()
	{
		timeLived = -1f;
		if (_objectPool != null)
		{
			_objectPool.PushObject(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}

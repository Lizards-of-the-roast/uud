using System;
using System.Collections.Generic;
using System.Reflection;
using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class AccessorySandbox : MonoBehaviour
{
	public enum ShadowQualityEnum
	{
		Low,
		Medium,
		High,
		Ultra
	}

	public ShadowQualityEnum ShadowQualityItem;

	[Header("Drag an accessory prefab from the project here")]
	public GameObject[] Accessory;

	[Header("(Optional) Drag a second accessory prefab for use on the opponent side")]
	public GameObject OpponentAccessory;

	[Space]
	[Header("Turn off auto Twitch and Fidget")]
	public bool turnOffTimers = true;

	[Header("Required Objects (No Touchy!)")]
	public GameObject LocalTotemRoot;

	[Tooltip("Animations with triggers that have following words will only play on Local Side")]
	public string[] localExclusiveAnimationKeywords;

	[Space(10f)]
	public GameObject OpponentTotemRoot;

	[Tooltip("Triggers with following words will only play on Opponent Side\neg: Opponent will choose both Mouse_ClickOpponent and Emote_Opponent")]
	public string[] opponentExclusiveAnimationKeywords;

	private AccessoryController[] _accessoryControllerLocal;

	private AccessoryController _accessoryControllerOpponent;

	private Animator[] _animatorLocal;

	private Animator _animatorOpponent;

	[Space(15f)]
	public GameObject BaseButton;

	public GameObject EvntButton;

	private Color EvntButtonColor;

	public GameObject AnimButton;

	private Color AnimButtonColor;

	public GameObject ButtonParent;

	private List<GameObject> _animBtns = new List<GameObject>();

	private List<GameObject> _evntBtns = new List<GameObject>();

	private bool _muteToggle;

	public Text ActiveAccessoryDisplay;

	private GameObject[] _accessoryLocal;

	private AssetLookupSystem _assetLookupSystem;

	[Header("Battlefield")]
	public bool _addBF = true;

	[InspectorButton("Evnt_OnClick")]
	public bool clickMe;

	private int variantIdx;

	private void Start()
	{
		SetUpBattleFieldOffsets(LocalTotemRoot.transform, MtgPlayer.DummyLocal);
		InstantiatePets(Accessory);
		if (!OpponentAccessory)
		{
			OpponentAccessory = Accessory[0];
		}
		SetUpBattleFieldOffsets(OpponentTotemRoot.transform, MtgPlayer.DummyOpponent);
		GameObject gameObject = UnityEngine.Object.Instantiate(OpponentAccessory);
		gameObject.transform.parent = OpponentTotemRoot.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.Euler(0f, 40f, 0f);
		_accessoryControllerOpponent = gameObject.GetComponent<AccessoryController>();
		if (_accessoryControllerOpponent.GetType() == typeof(AccessoryVariantDelegate))
		{
			_accessoryControllerOpponent.debugScene = true;
		}
		_accessoryControllerOpponent.Init(null, GREPlayerNum.Opponent);
		_accessoryControllerOpponent._turnOffTimers = turnOffTimers;
		_animatorOpponent = gameObject.transform.GetComponentInChildren<Animator>();
		SetUpButtons();
		ApplyShadowQualityLevel((int)ShadowQualityItem);
		AddBattlefield();
	}

	private void InstantiatePets(GameObject[] m_Accessory)
	{
		_accessoryLocal = new GameObject[m_Accessory.Length];
		_accessoryControllerLocal = new AccessoryController[m_Accessory.Length];
		_animatorLocal = new Animator[m_Accessory.Length];
		for (int i = 0; i < m_Accessory.Length; i++)
		{
			_accessoryLocal[i] = UnityEngine.Object.Instantiate(m_Accessory[i]);
			if (i != 0)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = _accessoryLocal[i].name.Replace("(Clone)", "_Offset");
				gameObject.transform.parent = LocalTotemRoot.transform;
				_accessoryLocal[i].transform.parent = gameObject.transform;
				if (i % 2 == 0)
				{
					gameObject.transform.localPosition = new Vector3(-3 * i / 2, 0f, 5 * ((i + 1) / 2));
				}
				else
				{
					gameObject.transform.localPosition = new Vector3(2 * (-(i + 1) / 2), 0f, -5 * ((i + 1) / 2));
				}
			}
			else
			{
				_accessoryLocal[i].transform.parent = LocalTotemRoot.transform;
				_accessoryLocal[i].transform.localPosition = new Vector3(0f, 0f, 0f);
			}
			_accessoryLocal[i].transform.localRotation = Quaternion.Euler(0f, 10f, 0f);
			_accessoryControllerLocal[i] = (AccessoryController)_accessoryLocal[i].GetComponent(typeof(AccessoryController));
			if (_accessoryControllerLocal[i].GetType() == typeof(AccessoryVariantDelegate))
			{
				_accessoryControllerLocal[i].debugScene = true;
			}
			_accessoryControllerLocal[i]._turnOffTimers = turnOffTimers;
			_accessoryControllerLocal[i].Init(null, GREPlayerNum.LocalPlayer);
			_animatorLocal[i] = _accessoryLocal[i].transform.GetComponentInChildren<Animator>();
			if (_accessoryControllerLocal[i].GetType() == typeof(AccessoryVariantDelegate_Phases))
			{
				AccessoryVariantDelegate_Phases accessoryVariantDelegate_Phases = (AccessoryVariantDelegate_Phases)_accessoryControllerLocal[i];
				_animatorLocal[i] = accessoryVariantDelegate_Phases.Variants[variantIdx].transform.GetComponentInChildren<Animator>();
				ActiveAccessoryDisplay.text = _accessoryLocal[i].name;
			}
			else
			{
				ActiveAccessoryDisplay.text = "";
			}
		}
	}

	private void SetUpBattleFieldOffsets(Transform root, MtgPlayer player)
	{
		bool flag = false;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (!sceneAt.name.Contains("Battlefield"))
			{
				continue;
			}
			flag = true;
			if (_assetLookupSystem.TreeLoader == null)
			{
				continue;
			}
			_assetLookupSystem.Blackboard.BattlefieldId = sceneAt.name.Substring(12, 3);
			_assetLookupSystem.Blackboard.Player = player;
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PetOffset> loadedTree))
			{
				PetOffset payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					root.localPosition += payload.Offset.PositionOffset;
				}
			}
		}
		if (!flag)
		{
			Debug.Log("<color=red><b>Error:</b></color> The Battlefield is not named to format: <i>Battlefield_BattleFieldId_Name </i>, some features may not work as expected");
		}
	}

	private void Update()
	{
	}

	public void SetUpButtons()
	{
		MethodInfo[] methods = typeof(AccessorySandbox).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		foreach (MethodInfo method in methods)
		{
			if (method.Name.StartsWith("Evnt_"))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(BaseButton);
				gameObject.name = method.Name;
				gameObject.transform.SetParent(ButtonParent.transform, worldPositionStays: false);
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = method.Name.Remove(0, 5);
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					method.Invoke(this, null);
				});
				if (method.Name.StartsWith("Evnt_"))
				{
					_evntBtns.Add(gameObject);
				}
			}
		}
		GetAnimationParameters();
		EvntButtonColor = EvntButton.GetComponent<Image>().color;
		AnimButtonColor = AnimButton.GetComponent<Image>().color;
		ActivateAnimButtons();
	}

	public void AddBattlefield()
	{
		_ = _addBF;
	}

	public void SetUpAnimButtons()
	{
		foreach (GameObject animBtn in _animBtns)
		{
			UnityEngine.Object.DestroyImmediate(animBtn);
		}
		_animBtns = new List<GameObject>();
		GetAnimationParameters();
		ActivateAnimButtons();
	}

	public void ActivateAnimButtons()
	{
		foreach (GameObject animBtn in _animBtns)
		{
			animBtn.SetActive(value: true);
		}
		foreach (GameObject evntBtn in _evntBtns)
		{
			evntBtn.SetActive(value: false);
		}
		AnimButton.GetComponent<Image>().color = Color.white;
		EvntButton.GetComponent<Image>().color = Color.green;
	}

	public void ActivateEventButtons()
	{
		foreach (GameObject animBtn in _animBtns)
		{
			animBtn.SetActive(value: false);
		}
		foreach (GameObject evntBtn in _evntBtns)
		{
			evntBtn.SetActive(value: true);
		}
		EvntButton.GetComponent<Image>().color = Color.white;
		AnimButton.GetComponent<Image>().color = Color.green;
	}

	private void SetToggle(MethodBase method, bool toggle)
	{
		GameObject gameObject = _evntBtns.Find((GameObject x) => x.name == method.Name);
		if (toggle)
		{
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = method.Name.Remove(0, 5) + " Off";
		}
		else
		{
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = method.Name.Remove(0, 5) + " On";
		}
	}

	private void CreateBoolAnimButton(AnimatorControllerParameter param, bool arg)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(BaseButton);
		gameObject.name = param.name;
		gameObject.transform.SetParent(ButtonParent.transform, worldPositionStays: false);
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = param.name + " " + arg;
		for (int i = 0; i < _animatorLocal.Length; i++)
		{
			Animator _animatorCurrentLocal = _animatorLocal[i];
			gameObject.GetComponent<Button>().onClick.AddListener(delegate
			{
				_animatorCurrentLocal.SetBool(param.name, arg);
			});
		}
		gameObject.GetComponent<Button>().onClick.AddListener(delegate
		{
			_animatorOpponent.SetBool(param.name, arg);
		});
		_animBtns.Add(gameObject);
	}

	private void CreateTriggerAnimButton(string param)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(BaseButton);
		int num = 0;
		int num2 = 0;
		gameObject.name = param;
		gameObject.transform.SetParent(ButtonParent.transform, worldPositionStays: false);
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = param;
		string[] array = localExclusiveAnimationKeywords;
		foreach (string text in array)
		{
			num += Convert.ToInt32(param.ToUpper().Contains(text.ToUpper()));
		}
		array = opponentExclusiveAnimationKeywords;
		foreach (string text2 in array)
		{
			num2 += Convert.ToInt32(param.ToUpper().Contains(text2.ToUpper()));
		}
		if (num == 0)
		{
			if (param.Equals("Reset"))
			{
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					_animatorOpponent.Rebind();
				});
			}
			else
			{
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					_animatorOpponent.SetTrigger(param);
				});
			}
		}
		for (int num3 = 0; num3 < _animatorLocal.Length; num3++)
		{
			Animator _animatorCurrentLocal = _animatorLocal[num3];
			Animator[] componentsInChildren = _animatorLocal[num3].transform.parent.gameObject.GetComponentsInChildren<Animator>();
			if (componentsInChildren.Length > 1)
			{
				Animator[] array2 = componentsInChildren;
				foreach (Animator animator in array2)
				{
					if (param.Equals("Reset"))
					{
						gameObject.GetComponent<Button>().onClick.AddListener(delegate
						{
							animator.Rebind();
						});
					}
					else
					{
						gameObject.GetComponent<Button>().onClick.AddListener(delegate
						{
							animator.SetTrigger(param);
						});
					}
				}
			}
			else if (param.Equals("Reset"))
			{
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					_animatorCurrentLocal.Rebind();
				});
			}
			else
			{
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					_animatorCurrentLocal.SetTrigger(param);
				});
			}
		}
		_animBtns.Add(gameObject);
		num = 0;
	}

	private void GetAnimationParameters()
	{
		AnimatorControllerParameter[] parameters = _animatorLocal[0].parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
			{
				CreateBoolAnimButton(animatorControllerParameter, arg: true);
				CreateBoolAnimButton(animatorControllerParameter, arg: false);
			}
			if (animatorControllerParameter.type == AnimatorControllerParameterType.Trigger)
			{
				CreateTriggerAnimButton(animatorControllerParameter.name);
				if (animatorControllerParameter.name == "Game_Start")
				{
					CreateTriggerAnimButton("Reset");
				}
			}
		}
		Animator animator = _animatorLocal[0];
		int parameterCount = animator.parameterCount;
		AnimatorControllerParameter[] array = new AnimatorControllerParameter[parameterCount];
		for (int j = 0; j < parameterCount; j++)
		{
			array[j] = animator.GetParameter(j);
			Debug.Log("Parameter Name: " + array[j].name);
			if (array[j].type == AnimatorControllerParameterType.Bool)
			{
				Debug.Log("Default Bool: " + array[j].defaultBool);
			}
			else if (array[j].type == AnimatorControllerParameterType.Float)
			{
				Debug.Log("Default Float: " + array[j].defaultFloat);
			}
			else if (array[j].type == AnimatorControllerParameterType.Int)
			{
				Debug.Log("Default Int: " + array[j].defaultInt);
			}
		}
	}

	private bool HasParameter(string paramName)
	{
		AnimatorControllerParameter[] parameters = _animatorLocal[0].parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == paramName)
			{
				return true;
			}
		}
		return false;
	}

	private void TriggerAnimation(string triggerName, Animator m_animatorLocal)
	{
		m_animatorLocal.SetTrigger(triggerName);
		_animatorOpponent.SetTrigger(triggerName);
	}

	private void TriggerAnimation(string triggerName, bool value)
	{
		_animatorLocal[0].SetBool(triggerName, value);
		_animatorOpponent.SetBool(triggerName, value);
	}

	public void Evnt_OnClick()
	{
		_accessoryControllerLocal[0].HandleClick();
		_accessoryControllerOpponent.HandleClick();
	}

	public void Evnt_OnClickNear()
	{
	}

	public void Evnt_CycleFidget()
	{
		_accessoryControllerLocal[0].HandleFidget();
		_accessoryControllerOpponent.HandleFidget();
	}

	public void Evnt_ColorChange()
	{
	}

	public void Evnt_HoverEnter()
	{
		_accessoryControllerLocal[0].HandleHoverEnter(GREPlayerNum.LocalPlayer);
		_accessoryControllerOpponent.HandleHoverEnter(GREPlayerNum.Opponent);
	}

	public void Evnt_HoverExit()
	{
		_accessoryControllerLocal[0].HandleHoverExit(GREPlayerNum.LocalPlayer);
		_accessoryControllerOpponent.HandleHoverExit(GREPlayerNum.Opponent);
	}

	public void Evnt_Mute()
	{
		_muteToggle = !_muteToggle;
		MethodBase currentMethod = MethodBase.GetCurrentMethod();
		SetToggle(currentMethod, _muteToggle);
		_accessoryControllerLocal[0].OnPlayerMuteChanged(_muteToggle);
		_accessoryControllerOpponent.OnPlayerMuteChanged(_muteToggle);
	}

	public void Evnt_LocalEmote()
	{
		_accessoryControllerLocal[0].HandleLocalEmote();
		_accessoryControllerOpponent.HandleOpponentEmote();
	}

	public void Evnt_OpponentEmote()
	{
		_accessoryControllerLocal[0].HandleOpponentEmote();
		_accessoryControllerOpponent.HandleLocalEmote();
	}

	public void Evnt_Victory()
	{
		_accessoryControllerLocal[0].HandleVictory();
		_accessoryControllerOpponent.HandleDefeat();
	}

	public void Evnt_Defeat()
	{
		_accessoryControllerLocal[0].HandleDefeat();
		_accessoryControllerOpponent.HandleVictory();
	}

	public void Evnt_HypeGoodMedium()
	{
		_accessoryControllerLocal[0].HandleHype(0.8f);
		_accessoryControllerOpponent.HandleHype(-0.8f);
	}

	public void Evnt_HypeGoodLarge()
	{
		_accessoryControllerLocal[0].HandleHype(1f);
		_accessoryControllerOpponent.HandleHype(-1f);
	}

	public void Evnt_HypeBadMedium()
	{
		_accessoryControllerLocal[0].HandleHype(-0.8f);
		_accessoryControllerOpponent.HandleHype(0.8f);
	}

	public void Evnt_HypeBadLarge()
	{
		_accessoryControllerLocal[0].HandleHype(-1f);
		_accessoryControllerOpponent.HandleHype(1f);
	}

	public void Anim_Fidget1()
	{
		TriggerAnimation("Fidget1", _animatorLocal[0]);
	}

	public void Anim_ClickOn1()
	{
		TriggerAnimation("Mouse_ClickOn1", _animatorLocal[0]);
	}

	public void Anim_ClickOn2()
	{
		TriggerAnimation("Mouse_ClickOn2", _animatorLocal[0]);
	}

	public void Anim_Idle_Fidget1()
	{
		TriggerAnimation("Idle_Fidget1", _animatorLocal[0]);
	}

	public void Anim_HoverStart()
	{
		TriggerAnimation("Mouse_Hover", value: true);
	}

	public void Anim_HoverStop()
	{
		TriggerAnimation("Mouse_Hover", value: false);
	}

	public void Anim_ClickOpponent()
	{
		TriggerAnimation("Mouse_ClickOpponent", _animatorLocal[0]);
	}

	public void Anim_Victory()
	{
		TriggerAnimation("React_Victory", _animatorLocal[0]);
	}

	public void Anim_Defeat()
	{
		TriggerAnimation("React_Defeat", _animatorLocal[0]);
	}

	public void Anim_React_GoodLarge()
	{
		TriggerAnimation("React_GoodLarge", _animatorLocal[0]);
	}

	public void Anim_React_GoodMedium()
	{
		TriggerAnimation("React_GoodMedium", _animatorLocal[0]);
	}

	public void Anim_React_BadLarge()
	{
		TriggerAnimation("React_BadLarge", _animatorLocal[0]);
	}

	public void Anim_React_BadMedium()
	{
		TriggerAnimation("React_BadMedium", _animatorLocal[0]);
	}

	public void Anim_Sleep()
	{
		TriggerAnimation("Sleep", _animatorLocal[0]);
	}

	public void Anim_Wake()
	{
		TriggerAnimation("Wake", _animatorLocal[0]);
	}

	public void Anim_Emote_Player()
	{
		TriggerAnimation("Emote_Player", _animatorLocal[0]);
	}

	public void Anim_Emote_Opponent()
	{
		TriggerAnimation("Emote_Opponent", _animatorLocal[0]);
	}

	public void SwapVariants()
	{
		Debug.Log("Swapping...");
		if (_accessoryControllerLocal[0].GetType() == typeof(AccessoryVariantDelegate))
		{
			AccessoryVariantDelegate accessoryVariantDelegate = (AccessoryVariantDelegate)_accessoryControllerLocal[0];
			variantIdx++;
			variantIdx %= accessoryVariantDelegate.Variants.Length;
			accessoryVariantDelegate.SetAccessoryVariant(variantIdx);
			_animatorLocal[0] = accessoryVariantDelegate.Variants[variantIdx].transform.GetComponentInChildren<Animator>();
			Debug.Log("Current Variant:" + accessoryVariantDelegate.Variants[variantIdx]);
			ActiveAccessoryDisplay.text = accessoryVariantDelegate.Variants[variantIdx].name;
			SetUpAnimButtons();
		}
		if (_accessoryControllerLocal[0].GetType() == typeof(AccessoryVariantDelegate_Phases))
		{
			AccessoryVariantDelegate_Phases accessoryVariantDelegate_Phases = (AccessoryVariantDelegate_Phases)_accessoryControllerLocal[0];
			variantIdx++;
			variantIdx %= accessoryVariantDelegate_Phases.Variants.Length;
			accessoryVariantDelegate_Phases.SetAccessoryVariant(variantIdx);
			_animatorLocal[0] = accessoryVariantDelegate_Phases.Variants[variantIdx].transform.GetComponentInChildren<Animator>();
			Debug.Log("Current Variant:" + accessoryVariantDelegate_Phases.Variants[variantIdx]);
			ActiveAccessoryDisplay.text = accessoryVariantDelegate_Phases.Variants[variantIdx].name;
			SetUpAnimButtons();
		}
	}

	private static void ApplyShadowQualityLevel(int q)
	{
		UniversalRenderPipelineAsset asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
		switch (q)
		{
		case 0:
			asset.SetMainLightShadowsEnabled(value: false);
			asset.SetAdditionalLightShadowsEnabled(value: false);
			asset.SetSoftShadowsSupported(value: false);
			asset.SetShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._256);
			break;
		default:
			asset.SetMainLightShadowsEnabled(value: true);
			asset.SetAdditionalLightShadowsEnabled(value: true);
			asset.SetSoftShadowsSupported(value: false);
			asset.SetShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._512);
			break;
		case 2:
			asset.SetMainLightShadowsEnabled(value: true);
			asset.SetAdditionalLightShadowsEnabled(value: true);
			asset.SetSoftShadowsSupported(value: true);
			asset.SetShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._1024);
			break;
		case 3:
			asset.SetMainLightShadowsEnabled(value: true);
			asset.SetAdditionalLightShadowsEnabled(value: true);
			asset.SetSoftShadowsSupported(value: true);
			asset.SetShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._2048);
			break;
		}
	}
}

using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.Interactions;

public class test : MonoBehaviour
{
	public AvatarInput PortraitInput;

	[Space(10f)]
	[Header("Avatar Interaction System")]
	[SerializeField]
	private ClickAndHoldButton PortraitButton;

	public EmoteOptionsView LocalEmoteOptions;

	public GameObject _dump;

	private void Start()
	{
		PlatformUtils.IsHandheld();
	}

	private void Awake()
	{
	}

	public void Foo(ClickAndHoldButton input)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("<color=green>Hi </color>");
		LocalEmoteOptions.Open();
	}
}

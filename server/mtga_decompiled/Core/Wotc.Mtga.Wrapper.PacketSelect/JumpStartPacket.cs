using System.Collections.Generic;
using Core.Meta.Cards;
using UnityEngine;
using UnityEngine.Serialization;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class JumpStartPacket : MonoBehaviour
{
	[SerializeField]
	private PacketInput _input;

	[SerializeField]
	private Localize _packTitle;

	[SerializeField]
	private MeshRenderer _cardBack;

	[SerializeField]
	private GameObject _hoverHighlight;

	[SerializeField]
	private ColorDisplayView _colorDisplayView;

	[FormerlySerializedAs("_firstPickTab")]
	[SerializeField]
	private GameObject _bluePickTab;

	[FormerlySerializedAs("_firstPickTabText")]
	[SerializeField]
	private Localize _bluePickTabText;

	[FormerlySerializedAs("_secondPickTab")]
	[SerializeField]
	private GameObject _orangePickTab;

	[FormerlySerializedAs("_secondPickTabText")]
	[SerializeField]
	private Localize _orangePickTabText;

	public PacketInput Input => _input;

	public Transform Root => base.transform;

	private void Awake()
	{
		UpdateHighlight(active: false);
		ResetBanners();
	}

	public void SetName(MTGALocalizedString text)
	{
		_packTitle.SetText(text);
	}

	public void SetPacketColors(string[] colors)
	{
		_colorDisplayView.SetColors(colors);
	}

	public void SetPacketArt(PacketArt packetArt)
	{
		Material[] materials = _cardBack.materials;
		Material[] array = materials;
		foreach (Material material in array)
		{
			if (material.name.Contains("ArtInFrame"))
			{
				material.mainTexture = packetArt.Texture;
				if (packetArt.Crop != null)
				{
					packetArt.Crop.ApplyToMaterial(material);
				}
			}
		}
		_cardBack.materials = materials;
	}

	public void UpdateHighlight(bool active)
	{
		_hoverHighlight?.UpdateActive(active);
	}

	public void SetAsSubmittedPacketHeader(uint submissionCount)
	{
		_bluePickTabText.SetText(new MTGALocalizedString
		{
			Key = "Events/Packets/Banner_Text",
			Parameters = new Dictionary<string, string> { 
			{
				"packetNum",
				submissionCount.ToString()
			} }
		});
		_bluePickTab?.UpdateActive(active: true);
		_orangePickTab?.UpdateActive(active: false);
	}

	public void SetSelectedPacketHeader(uint selectionCount)
	{
		_orangePickTabText.SetText(new MTGALocalizedString
		{
			Key = "Events/Packets/Banner_Text",
			Parameters = new Dictionary<string, string> { 
			{
				"packetNum",
				selectionCount.ToString()
			} }
		});
		_bluePickTab?.UpdateActive(active: false);
		_orangePickTab?.UpdateActive(active: true);
	}

	public void ResetBanners()
	{
		_bluePickTab?.UpdateActive(active: false);
		_orangePickTab?.UpdateActive(active: false);
	}
}

using UnityEngine;
using Wotc.Mtga.VFX;

public class AccessoryEventVFXGroup : MonoBehaviour
{
	[SerializeField]
	private string groupName;

	[SerializeField]
	public ParticleSystem[] vfx;

	[SerializeField]
	public VFXPrefabPlayer[] vfxPrefabPlayers;

	public void TogglePlay(bool play)
	{
		for (int i = 0; i < vfx.Length; i++)
		{
			if (!(vfx[i] == null))
			{
				if (play)
				{
					vfx[i].Play();
				}
				else
				{
					vfx[i].Stop();
				}
			}
		}
		for (int j = 0; j < vfxPrefabPlayers.Length; j++)
		{
			if (!(vfxPrefabPlayers[j] == null))
			{
				if (play)
				{
					vfxPrefabPlayers[j].Play();
				}
				else
				{
					vfxPrefabPlayers[j].Stop();
				}
			}
		}
	}

	public void PlayIndividualVFX(string vfxName = "")
	{
		if (string.IsNullOrEmpty(vfxName))
		{
			return;
		}
		for (int i = 0; i < vfx.Length; i++)
		{
			if (!(vfx[i] == null) && vfx[i].gameObject.name == vfxName)
			{
				vfx[i].Play();
			}
		}
		for (int j = 0; j < vfxPrefabPlayers.Length; j++)
		{
			if (!(vfxPrefabPlayers[j] == null) && vfxPrefabPlayers[j].gameObject.name == vfxName)
			{
				vfxPrefabPlayers[j].Play();
			}
		}
	}

	public void StopIndividualVFX(string vfxName = "")
	{
		if (string.IsNullOrEmpty(vfxName))
		{
			return;
		}
		for (int i = 0; i < vfx.Length; i++)
		{
			if (vfx[i].gameObject.name == vfxName)
			{
				vfx[i].Stop();
			}
		}
		for (int j = 0; j < vfxPrefabPlayers.Length; j++)
		{
			if (!(vfxPrefabPlayers[j] == null) && vfxPrefabPlayers[j].gameObject.name == vfxName)
			{
				vfxPrefabPlayers[j].Stop();
			}
		}
	}

	public void ClearIndividualVFX(string vfxName = "")
	{
		if (string.IsNullOrEmpty(vfxName))
		{
			return;
		}
		for (int i = 0; i < vfx.Length; i++)
		{
			if (vfx[i].gameObject.name == vfxName)
			{
				vfx[i].Clear();
			}
		}
		for (int j = 0; j < vfxPrefabPlayers.Length; j++)
		{
			if (!(vfxPrefabPlayers[j] == null) && vfxPrefabPlayers[j].gameObject.name == vfxName)
			{
				vfxPrefabPlayers[j].Clear();
			}
		}
	}

	public GameObject GetVFXObject(string vfxName)
	{
		GameObject result = null;
		if (!string.IsNullOrEmpty(vfxName))
		{
			for (int i = 0; i < vfx.Length; i++)
			{
				if (!(vfx[i] == null) && vfx[i].gameObject.name == vfxName)
				{
					result = vfx[i].gameObject;
					break;
				}
			}
		}
		return result;
	}
}

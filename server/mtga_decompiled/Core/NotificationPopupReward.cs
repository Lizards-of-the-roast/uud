using TMPro;
using UnityEngine;

public class NotificationPopupReward : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer _mainMesh;

	[SerializeField]
	private MeshRenderer _deckBoxBase;

	[SerializeField]
	private MeshRenderer _deckBoxLid;

	[SerializeField]
	private TextMeshProUGUI _countTxtField;

	[SerializeField]
	private GameObject _countTxtGameObject;

	private RendererReferenceLoader _mainMeshReferenceLoader;

	private RendererReferenceLoader _deckBoxBaseLoader;

	private RendererReferenceLoader _deckBoxLidLoader;

	public bool HackDeckBox;

	public void SetBackgroundTexture(string bgPath)
	{
		SetTexture(ref _mainMeshReferenceLoader, _mainMesh, "_MainTex", bgPath, 0);
		if (_deckBoxBase != null)
		{
			SetTexture(ref _deckBoxBaseLoader, _deckBoxBase, "_MainTex", bgPath, 1);
			SetTexture(ref _deckBoxLidLoader, _deckBoxLid, "_MainTex", bgPath, 1);
		}
	}

	public void SetForegroundTexture(string fgPath)
	{
		SetTexture(ref _mainMeshReferenceLoader, _mainMesh, "_Decal1", fgPath, 0);
	}

	private static void SetTexture(ref RendererReferenceLoader loader, MeshRenderer mesh, string blockProperty, string texturePath, int materialIndex)
	{
		if (!(mesh == null) && !string.IsNullOrEmpty(texturePath))
		{
			if (loader == null)
			{
				loader = new RendererReferenceLoader(mesh);
			}
			loader.SetAndApplyPropertyBlockTexture(materialIndex, blockProperty, texturePath);
		}
	}

	public void SetCount(int count)
	{
		if (_countTxtField != null && _countTxtGameObject != null)
		{
			if (count > 1)
			{
				_countTxtGameObject.SetActive(value: true);
				_countTxtField.text = "x" + count;
			}
			else
			{
				_countTxtGameObject.SetActive(value: false);
			}
		}
	}

	public void OnDestroy()
	{
		_mainMeshReferenceLoader?.Cleanup();
		_deckBoxBaseLoader?.Cleanup();
		_deckBoxLidLoader?.Cleanup();
	}
}

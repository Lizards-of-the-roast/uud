using UnityEngine;

[ExecuteInEditMode]
public class Accessory_LayerSetter : MonoBehaviour
{
	public enum prefabType
	{
		Wrapper,
		DuelScene
	}

	public prefabType typeOfPrefab;

	private void Update()
	{
		UpdateLayer((int)typeOfPrefab);
	}

	public void UpdateLayer(int selection)
	{
		switch (selection)
		{
		case 0:
			ChangeLayersRecursively(base.gameObject.transform, "UI");
			break;
		case 1:
			ChangeLayersRecursively(base.gameObject.transform, "Accessory");
			break;
		}
	}

	public void ChangeLayersRecursively(Transform trans, string name)
	{
		if (trans.gameObject.layer == LayerMask.NameToLayer(name))
		{
			return;
		}
		trans.gameObject.layer = LayerMask.NameToLayer(name);
		foreach (Transform tran in trans)
		{
			ChangeLayersRecursively(tran, name);
		}
	}
}

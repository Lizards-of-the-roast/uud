using UnityEngine;

public class BrowserCardHolderLayoutDataVariant : BrowserCardHolderLayoutData
{
	[HideInInspector]
	public BrowserCardHolderLayoutData Parent;

	public void Init(BrowserCardHolderLayoutData parent)
	{
		Parent = parent;
		RevertToParent();
	}

	public void RevertToParent()
	{
		if ((object)Parent == null)
		{
			Debug.LogError("ScriptableObject Variant is Missing its Parent");
		}
		else
		{
			CardHolderLayout.CopyFrom(Parent.CardHolderLayout);
		}
	}
}

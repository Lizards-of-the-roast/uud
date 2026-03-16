using UnityEngine;

public class CDCPart_PTBox : CDCPart
{
	[SerializeField]
	private GameObject _damagedRoot;

	protected override void HandleDestructionInternal()
	{
		if (!(_damagedRoot != null))
		{
			return;
		}
		if (_cachedDestroyed)
		{
			if (_damagedRoot.activeSelf)
			{
				_damagedRoot.SetActive(value: false);
			}
		}
		else
		{
			UpdateDamageRoot();
		}
		base.HandleDestructionInternal();
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		if (_damagedRoot.activeSelf)
		{
			_damagedRoot.SetActive(value: false);
		}
	}

	protected override void HandleUpdateInternal()
	{
		UpdateDamageRoot();
	}

	private void UpdateDamageRoot()
	{
		if (_damagedRoot != null && _damagedRoot.activeSelf != _cachedModel.Damaged)
		{
			_damagedRoot.SetActive(_cachedModel.Damaged);
		}
	}

	public void SetVisible(bool visible)
	{
		if (base.gameObject.activeSelf != visible)
		{
			base.gameObject.SetActive(visible);
		}
	}
}

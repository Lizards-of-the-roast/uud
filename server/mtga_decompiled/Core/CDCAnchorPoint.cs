using UnityEngine;

public class CDCAnchorPoint : MonoBehaviour
{
	[SerializeField]
	private CDCAnchorType _anchorType = CDCAnchorType.Invalid;

	[SerializeField]
	private bool _custom;

	[SerializeField]
	private string _customKey;

	public CDCAnchorType AnchorType
	{
		get
		{
			if (!_custom)
			{
				return _anchorType;
			}
			return CDCAnchorType.Invalid;
		}
	}

	public bool Custom => _custom;

	public string CustomKey
	{
		get
		{
			if (!_custom)
			{
				return null;
			}
			return _customKey;
		}
	}
}

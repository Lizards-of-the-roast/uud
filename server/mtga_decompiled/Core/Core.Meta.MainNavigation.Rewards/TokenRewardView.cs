using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Rewards;

public class TokenRewardView : MonoBehaviour
{
	[SerializeField]
	private Localize _titleLabel;

	[SerializeField]
	private Localize _descriptionLabel;

	[SerializeField]
	private GameObject _labelContainer;

	public void Refresh(string titleKey, int quantity, string descriptionKey)
	{
		if (_titleLabel != null)
		{
			_titleLabel.SetText(titleKey, new Dictionary<string, string> { 
			{
				"quantity",
				quantity.ToString()
			} });
		}
		if (_descriptionLabel != null)
		{
			_descriptionLabel.SetText(descriptionKey);
		}
		if (_labelContainer != null)
		{
			_labelContainer.SetActive(!string.IsNullOrEmpty(descriptionKey));
		}
	}
}

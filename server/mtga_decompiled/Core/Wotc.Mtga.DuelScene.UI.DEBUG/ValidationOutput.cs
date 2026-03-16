using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class ValidationOutput : MonoBehaviour
{
	private readonly StringBuilder _stringBuilder = new StringBuilder();

	[SerializeField]
	private ScrollRect _outputScroll;

	[SerializeField]
	private TMP_Text _errorText;

	[SerializeField]
	private TMP_Text _warningText;

	public void SetModel(IReadOnlyCollection<string> errors, IReadOnlyCollection<string> warnings)
	{
		bool flag = errors.Count > 0 || warnings.Count > 0;
		_outputScroll.gameObject.UpdateActive(flag);
		if (flag)
		{
			_outputScroll.normalizedPosition = Vector2.zero;
			_errorText.text = ConvertToOutputString(errors);
			_warningText.text = ConvertToOutputString(warnings);
		}
	}

	private string ConvertToOutputString(IEnumerable<string> reasons)
	{
		_stringBuilder.Clear();
		foreach (string reason in reasons)
		{
			_stringBuilder.AppendLine("• " + reason);
		}
		return _stringBuilder.ToString().TrimEnd();
	}
}

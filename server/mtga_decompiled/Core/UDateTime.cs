using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class UDateTime
{
	[HideInInspector]
	[SerializeField]
	private string _dateTime;

	public static implicit operator DateTime(UDateTime udt)
	{
		DateTime.TryParseExact(udt._dateTime, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
		return result;
	}

	public static implicit operator UDateTime(DateTime dt)
	{
		return new UDateTime
		{
			_dateTime = dt.ToString()
		};
	}
}

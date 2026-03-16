using System.Collections.Generic;
using UnityEngine;

public interface ICardLayout
{
	void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation);
}

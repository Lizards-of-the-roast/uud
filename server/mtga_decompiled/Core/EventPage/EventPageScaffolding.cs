using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EventPage;

public class EventPageScaffolding : MonoBehaviour
{
	[Serializable]
	public class LayoutGroupInfo
	{
		public ComponentLocation Location;

		public Transform LayoutGroup;
	}

	[SerializeField]
	private List<LayoutGroupInfo> _layoutGroups;

	[SerializeField]
	private Image _backgroundImage;

	public RectTransform safeArea;

	private AssetLoader.AssetTracker<Sprite> _backgroundImageSpriteTracker;

	public Dictionary<ComponentLocation, Transform> LayoutGroups
	{
		get
		{
			Dictionary<ComponentLocation, Transform> dictionary = new Dictionary<ComponentLocation, Transform>();
			if (_layoutGroups != null)
			{
				foreach (LayoutGroupInfo layoutGroup in _layoutGroups)
				{
					dictionary[layoutGroup.Location] = layoutGroup.LayoutGroup;
				}
			}
			return dictionary;
		}
	}

	public void SetBackgroundImage(string spritePath)
	{
		if (_backgroundImage != null)
		{
			if (_backgroundImageSpriteTracker == null)
			{
				_backgroundImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("EventPageBackgroundImageSprite");
			}
			AssetLoaderUtils.TrySetSprite(_backgroundImage, _backgroundImageSpriteTracker, spritePath);
		}
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_backgroundImage, _backgroundImageSpriteTracker);
	}
}

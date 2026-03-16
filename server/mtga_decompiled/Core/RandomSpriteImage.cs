using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RandomSpriteImage : MonoBehaviour
{
	[SerializeField]
	private TextAsset _fileList;

	private Image _image;

	private Sprite _loadedSprite;

	private void Awake()
	{
		_image = GetComponent<Image>();
	}

	private void Start()
	{
		AssignRandomSprite();
	}

	private void OnDestroy()
	{
		if (_loadedSprite != null)
		{
			_image.sprite = null;
			Resources.UnloadAsset(_loadedSprite);
			_loadedSprite = null;
		}
	}

	private void AssignRandomSprite()
	{
		if (!(_fileList != null))
		{
			return;
		}
		string[] array = _fileList.text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 0)
		{
			Sprite sprite = Resources.Load<Sprite>(array[UnityEngine.Random.Range(0, array.Length)]);
			if (sprite != null)
			{
				_image.sprite = sprite;
				_loadedSprite = sprite;
			}
		}
	}
}

using UnityEngine;

namespace StatsMonitor.Core;

public class Bitmap2D
{
	public Texture2D texture;

	public Color color;

	protected Rect _rect;

	public Bitmap2D(int width, int height, Color? color = null)
	{
		texture = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		texture.filterMode = FilterMode.Point;
		_rect = new Rect(0f, 0f, width, height);
		this.color = color ?? Color.black;
		Clear();
	}

	public Bitmap2D(float width, float height, Color? color = null)
	{
		texture = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, mipChain: false);
		texture.filterMode = FilterMode.Point;
		this.color = color ?? Color.black;
		Clear();
	}

	public void Resize(int width, int height)
	{
		texture.Reinitialize(width, height);
		texture.Apply();
	}

	public void Clear(Color? color = null)
	{
		Color color2 = color ?? this.color;
		Color[] pixels = texture.GetPixels();
		int num = 0;
		while (num < pixels.Length)
		{
			pixels[num++] = color2;
		}
		texture.SetPixels(pixels);
		texture.Apply();
	}

	public void FillRect(Rect? rect = null, Color? color = null)
	{
		Rect rect2 = rect ?? _rect;
		Color color2 = color ?? this.color;
		Color[] array = new Color[(int)(rect2.width * rect2.height)];
		int num = 0;
		while (num < array.Length)
		{
			array[num++] = color2;
		}
		texture.SetPixels((int)rect2.x, (int)rect2.y, (int)rect2.width, (int)rect2.height, array);
	}

	public void FillRect(int x, int y, int w, int h, Color? color = null)
	{
		Color color2 = color ?? this.color;
		Color[] array = new Color[w * h];
		int num = 0;
		while (num < array.Length)
		{
			array[num++] = color2;
		}
		texture.SetPixels(x, y, w, h, array);
	}

	public void FillColumn(int x, Color? color = null)
	{
		FillRect(new Rect(x, 0f, 1f, texture.height), color);
	}

	public void FillColumn(int x, int y, int height, Color? color = null)
	{
		FillRect(new Rect(x, y, 1f, height), color);
	}

	public void FillRow(int y, Color? color = null)
	{
		FillRect(new Rect(0f, y, texture.width, 1f), color);
	}

	public void SetPixel(int x, int y, Color color)
	{
		texture.SetPixel(x, y, color);
	}

	public void SetPixel(float x, float y, Color color)
	{
		texture.SetPixel((int)x, (int)y, color);
	}

	public void Scroll(int x, int y, Color? fillColor = null)
	{
		int x2 = 0;
		int x3 = x;
		int x4 = 0;
		if (x < 0)
		{
			x = (x2 = ~x + 1);
			x3 = 0;
			x4 = texture.width - x;
		}
		Color[] pixels = texture.GetPixels(x2, y, texture.width - x, texture.height - y);
		texture.SetPixels(x3, 0, texture.width - x, texture.height - y, pixels);
		FillRect(x4, 0, x, texture.height, fillColor);
	}

	public void Apply()
	{
		texture.Apply();
	}

	public void Dispose()
	{
		Object.Destroy(texture);
	}
}

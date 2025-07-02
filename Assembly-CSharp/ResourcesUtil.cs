using System;
using UnityEngine;

public class ResourcesUtil
{
	public static Texture2D GetUITexture(string resource)
	{
		return LoadResManager.LoadTex(resource.ToLower());
	}
}

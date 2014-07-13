using System;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
	[SerializeField] float screenScale = 1.0f;
	
	public float ScreenWidth
	{
		get
		{
			return Screen.width * this.screenScale;
		}
	}
	
	public float ScreenHeight
	{
		get
		{
			return Screen.height * this.screenScale;
		}
	}
	
	public float ScreenScale	
	{
		get
		{
			return this.screenScale;
		}
	}
}


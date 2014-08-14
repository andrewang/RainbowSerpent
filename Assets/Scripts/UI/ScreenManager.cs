using System;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
	[SerializeField] private float screenScale = 1.0f;
	[SerializeField] private bool screenRotated = false;
	
	public bool ScreenRotated
	{
		get
		{
			return this.screenRotated;
		}
	}
	
	public float Width
	{
		get
		{
			if (screenRotated)
			{
				return Screen.height * this.screenScale;
			}
			return Screen.width * this.screenScale;
		}
	}
	
	public float Height
	{
		get
		{
			if (screenRotated)
			{
				return Screen.height * this.screenScale;
			}
			return Screen.height * this.screenScale;
		}
	}
	
	public float Scale	
	{
		get
		{
			return this.screenScale;
		}
	}
}


using System;
using UnityEngine;

public class GameClock : MonoBehaviour
{
	private float time = 0.0f;
	public float Time	
	{
		get
		{
			return this.time;
		}
	}
	
	public void Update()
	{
		float deltaTime = UnityEngine.Time.deltaTime;
		if (deltaTime > 0.1f) { deltaTime = 0.1f; }
		this.time += deltaTime;
	}
	
}


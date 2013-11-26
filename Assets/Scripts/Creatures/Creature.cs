using System;
using UnityEngine;

public class Creature : MonoBehaviour
{
	// Side is used for player snake vs enemy snake vs frog (vs ???).  Interactions should be skipped between creatures of the same side.
	public int Side
	{
		get; set; 
	}


	public void Update()
	{
		// Check for interactions with other creatures (after moving).  This should be through a virtual function which can be override for snakes to check head to body interactions
	}
}


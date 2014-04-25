using UnityEngine;
using System.Collections;

public class SnakeBody : SnakeSegment 
{
	public float DistanceFromHead { get; set; }
	
	public override void ResetProperties()
	{
		base.ResetProperties();
		this.DistanceFromHead = 0.0f;
	}
	
}

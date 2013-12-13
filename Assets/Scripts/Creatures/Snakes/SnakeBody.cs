using UnityEngine;
using System.Collections;

public class SnakeBody : SnakeSegment 
{
	public float DistanceFromHead { get; set; }
	
	/*
	override public void UpdatePosition(SerpentConsts.Dir parentDirection, float displacement)
	{
		// Here the direction vector passed in is the direction of the segment ahead of this segment.
		// If we reach the current destination then we change direction to that one.		
		Vector3 toDest = this.CurrentDestination - this.transform.localPosition;
		if (displacement < toDest.sqrMagnitude)
		{
			// Have not reached current destionation so just move.
			this.transform.localPosition += (this.CurrentDirectionVector * displacement);
			// Pass message to next segment, and done.
			UpdateNextSegmentPosition( this.CurrentDirection, displacement );			
			return;
		}
				
		// Update position to exactly be the destination
		this.transform.localPosition = this.CurrentDestination;
		
		// Do remaining movement
		float distToDest = toDest.magnitude;		
		float remainingDisplacement = displacement - distToDest;
		this.CurrentDirection = this.NextDirection;
		this.NextDirection = parentDirection;
		
		// Update position by the remaining displacement and new direction		
		this.transform.localPosition += (this.CurrentDirectionVector * remainingDisplacement);
		
		// Pass message to next segment with the full displacement
		UpdateNextSegmentPosition( this.CurrentDirection, displacement );		
	}
	*/	
}

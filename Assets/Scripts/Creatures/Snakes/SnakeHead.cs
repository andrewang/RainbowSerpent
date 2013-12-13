using UnityEngine;
using System.Collections;

public class SnakeHead : SnakeSegment
{
	public bool UpdatePosition(float displacement, out float remainingDisplacement)
	{
		Vector3 toDest = this.CurrentDestination - this.transform.localPosition;
		if (displacement < toDest.sqrMagnitude)
		{
			// Have not reached current destiona`tion so just move.
			this.transform.localPosition += (this.CurrentDirectionVector * displacement);
			// Pass message to next segment
			//UpdateNextSegmentPosition( this.CurrentDirection, displacement );			
			
			// Did not arrive at destination
			remainingDisplacement = 0.0f;
			return false;
		}
					
		// Update the head position to exactly be the destination
		this.transform.localPosition = this.CurrentDestination;
		
		float distToDest = toDest.magnitude;		
		//UpdateNextSegmentPosition( this.CurrentDirection, distToDest );
		
		remainingDisplacement = displacement - distToDest;
		return true;
	}

	
}

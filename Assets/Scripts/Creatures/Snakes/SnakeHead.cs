using UnityEngine;
using System.Collections;

public class SnakeHead : SnakeSegment
{
	/// <summary>
	/// Updates the position of the head
	/// </summary>
	/// <returns><c>true</c>, if head arrived at its destionation, <c>false</c> otherwise.</returns>
	/// <param name="displacement">Displacement.</param>
	/// <param name="remainingDisplacement">Remaining displacement.</param>
	public bool MoveForward(float displacement, out float remainingDisplacement)
	{
		Vector3 toDest = this.CurrentDestination - this.transform.localPosition;
		if (displacement <= toDest.sqrMagnitude)
		{
			// Have not reached current destionation so just move.
			this.transform.localPosition += (this.CurrentDirectionVector * displacement);
			
			// Did not arrive at destination
			remainingDisplacement = 0.0f;
			return false;
		}
					
		// Update the head position to exactly be the destination
		this.transform.localPosition = this.CurrentDestination;
		
		float distToDest = toDest.magnitude;		
		
		remainingDisplacement = displacement - distToDest;
		return true;
	}

	
}

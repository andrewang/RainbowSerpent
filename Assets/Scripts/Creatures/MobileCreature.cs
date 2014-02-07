using System;
using UnityEngine;

public class MobileCreature : Creature
{
	
	public CreatureController Controller
	{
		get; set;
	}
	
	/// <summary>
	/// The current direction.  Creatures can only change direction at the centre of tiles
	/// </summary>
	private SerpentConsts.Dir currentDirection = SerpentConsts.Dir.None;
	protected SerpentConsts.Dir CurrentDirection
	{
		get
		{
			return currentDirection;
		}
		set
		{
			this.currentDirection = value;
			this.currentDirectionVector = SerpentConsts.DirectionVector3[ (int)value ];
		}
	}
	
	private Vector3 currentDirectionVector;
	protected Vector3 CurrentDirectionVector
	{
		get
		{
			return this.currentDirectionVector;
		}
	}
	
	protected Vector3 CurrentDestination
	{
		get; set; 
	}
	
		
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


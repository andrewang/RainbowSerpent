using System;
using UnityEngine;

public class MobileCreature : Creature
{
	public CreatureController Controller { get; set; }
	
	public float Speed { get; set; }	
	
	/// <summary>
	/// The current direction.  Creatures can only change direction at the centre of tiles
	/// </summary>
	private SerpentConsts.Dir currentDirection = SerpentConsts.Dir.None;
	public SerpentConsts.Dir CurrentDirection
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
	public Vector3 CurrentDirectionVector
	{
		get
		{
			return this.currentDirectionVector;
		}
	}
	
	public Vector3 CurrentDestination
	{
		get; set; 
	}
	
	public MobileCreature()
	{
		this.Speed = 20.0f;
	}
		
	public virtual void Update()
	{
		/*
		if (this.Visible == false || this.Dead) 
		{
			return; 
		}
		*/
		
		// Update position of head based on speed and direction.  When we reach the centre of a tile, make a callback
		// to the Controller.
		if (this.CurrentDirection != SerpentConsts.Dir.None)
		{
			UpdatePosition();			
		}
	}
	
	protected void UpdatePosition()
	{
		float displacement = this.Speed * Time.smoothDeltaTime;
		
		float remainingDisplacement = 0.0f;
		bool arrived = this.MoveForward( displacement, out remainingDisplacement );
		if (arrived == false) { return; }
		
		ArrivedAtDestination(remainingDisplacement);		
	}
		
	public virtual bool MoveForward(float displacement, out float remainingDisplacement)
	{
		Vector3 toDest = this.CurrentDestination - this.transform.localPosition;		
		Vector3 nextPos = this.transform.localPosition + (this.CurrentDirectionVector * displacement);
		Vector3 afterMoveToDest = this.CurrentDestination - nextPos;
		
		// possibly this could be optimized with this.CurrentDirectionVector
		
		if (Vector3.Dot(toDest, afterMoveToDest) > 0)
			
		//Vector3 toDest = this.CurrentDestination - this.transform.localPosition;
		//if (displacement <= toDest.sqrMagnitude)
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
	
	public virtual void ArrivedAtDestination(float remainingDisplacement)
	{		
		// Inform the controller we arrived, and receive the new direction to go in.
		SerpentConsts.Dir newDirection = this.Controller.NewDirectionUponArrival();
		if (newDirection == SerpentConsts.Dir.None) 
		{ 
			this.CurrentDirection = SerpentConsts.Dir.None;
			return; 
		}
		
		bool directionChanged = (newDirection != this.CurrentDirection);
		
		// Change the current direction if it's possible to go that way.  If not, check the current direction - keep moving
		// in that direction if possible, or stop if it's blocked.
		if (!IsMotionBlocked( newDirection ))
		{
			this.CurrentDirection = newDirection;	
		}
		else if (IsMotionBlocked( this.CurrentDirection ))
		{
			// stop moving
			this.CurrentDirection = SerpentConsts.Dir.None;
			return;
		}
		
		// Open any door between the current position and the next one. 
		OpenDoor( this.CurrentDirection );
		
		if (directionChanged)
		{			
			OnDirectionChange();
		}
		
		UpdateDestination();
		
		float dummyOutput = 0.0f;
		MoveForward( remainingDisplacement, out dummyOutput );
		
		// Close any door that can now be closed.
		CloseDoor();		
	}	
	
	public virtual void StartMoving(SerpentConsts.Dir direction)
	{		
		if (IsMotionBlocked( direction ))
		{
			return;
		}
		
		// Make sure any door that needs to open, opens
		OpenDoor( direction );
		
		// Note that we changed direction at this point
		this.CurrentDirection = direction;		
		OnDirectionChange();
		
		UpdateDestination();
	}
	
	/// <summary>
	/// Updates the destination of the creature in the direction its facing
	/// </summary>
	protected virtual void UpdateDestination()
	{
		Vector3 newPos = this.MazeController.GetNextCellCentre( this.transform.localPosition, this.CurrentDirection );
		this.CurrentDestination = newPos;		
	}
	
	protected virtual bool IsMotionBlocked( SerpentConsts.Dir direction )
	{
		return this.MazeController.IsMotionBlocked( GetPosition(), direction );
	}
	
	protected virtual void OnDirectionChange()
	{
		// nothing to do.  special case for some creatures
	}
	
	protected virtual void OpenDoor( SerpentConsts.Dir direction )
	{
		// by default creatures don't open doors
	}
	
	protected virtual void CloseDoor()
	{
		// by default creatures don't close doors
	}
}


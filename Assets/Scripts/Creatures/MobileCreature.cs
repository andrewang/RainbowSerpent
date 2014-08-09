using System;
using UnityEngine;
using Serpent;

public class MobileCreature : Creature
{
	public CreatureController Controller { get; set; }
	
	public float Speed { get; set; }	
	
	/// <summary>
	/// The current direction.  Creatures can only change direction at the centre of tiles
	/// </summary>
	private Direction currentDirection = Direction.None;
	public Direction CurrentDirection
	{
		get
		{
			return currentDirection;
		}
		set
		{
			this.currentDirection = value;
			this.directionVector = SerpentConsts.DirectionVector3[ (int)value ];
		}
	}
	
	private Vector3 directionVector;
	public Vector3 DirectionVector
	{
		get
		{
			return this.directionVector;
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
		// Update position of head based on speed and direction.  When we reach the centre of a tile, make a callback
		// to the Controller.
		if (this.CurrentDirection != Direction.None)
		{
			UpdatePosition();			
			
			// Close any door that can now be closed.
			CloseDoors();
		}
	}
	
	protected void UpdatePosition()
	{
		float displacement = this.Speed * Time.smoothDeltaTime * Managers.GameState.GameSpeed;
		
		float remainingDisplacement = 0.0f;
		bool arrived = this.MoveForward( displacement, out remainingDisplacement );
		if (arrived == false) { return; }
		
		ArrivedAtDestination(remainingDisplacement);		
	}
		
	public virtual bool MoveForward(float displacement, out float remainingDisplacement)
	{
		Vector3 toDest = this.CurrentDestination - this.transform.localPosition;		
		Vector3 nextPos = this.transform.localPosition + (this.directionVector * displacement);
		Vector3 afterMoveToDest = this.CurrentDestination - nextPos;
		
		// possibly this could be optimized with this.CurrentSerpentConsts.DirectionVector
		
		if (Vector3.Dot(toDest, afterMoveToDest) > 0)
		{
			// Have not reached current destionation so just move.
			this.transform.localPosition += (this.directionVector * displacement);
			
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
		Direction newDirection = this.Controller.NewDirectionUponArrival();
		if (newDirection == Direction.None) 
		{ 
			this.CurrentDirection = Direction.None;
			CloseDoors();			
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
			this.CurrentDirection = Direction.None;
			CloseDoors();			
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
		
		CloseDoors();
	}	
	
	public virtual void StartMoving(Direction direction)
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
	
	protected virtual bool IsMotionBlocked( Direction direction )
	{
		return this.MazeController.IsMotionBlocked( GetPosition(), direction );
	}
	
	protected virtual void OnDirectionChange()
	{
		// nothing to do.  special case for some creatures
	}
	
	protected virtual void OpenDoor( Direction direction )
	{
		// by default creatures don't open doors
	}
	
	protected virtual void CloseDoors()
	{
		// by default creatures don't close doors
	}
}


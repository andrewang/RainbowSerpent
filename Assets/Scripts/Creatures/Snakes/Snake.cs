using System;
using UnityEngine;
using System.Collections.Generic;

public class Snake : Creature
{
	public int NumSegments
	{
		get
		{
			int numSegments = 0;
			SnakeSegment segment = this.head;
			while (segment != null)
			{
				++numSegments;
				segment = segment.NextSegment;
			}
			return numSegments;
		}
	}
	
	public float Speed { get; set; }

	private SnakeSegment lastSegment
	{
		get
		{
			SnakeSegment segment = this.head;
			while (segment.NextSegment != null)
			{
				segment = segment.NextSegment;
			}
			return segment;
		}
	}

	private SnakeConfig config;
	private SnakeHead head;
	private SnakePositioning positioning;
	
	public void SetUp(MazeController mazeController, SnakeConfig config, int numSegments, bool playerControlled)
	{
		base.SetUp(mazeController);
		
		this.positioning = new SnakePositioning();
		
		// temp
		this.Speed = 80;
		
		this.config = config;
		for (int i = 0; i < numSegments; ++i)
		{
			AddSegment();
		}

		if (playerControlled)
		{
			this.Controller = new PlayerSnakeController(this, mazeController);
		}
		else
		{
			this.Controller = new AISnakeController( this, mazeController);
		}
	}
	
	public void SetInitialLocation(Vector3 position, SerpentConsts.Dir facingDirection)
	{
		// Set head location to the desired position and facing
		// Each body segment should be laid out in opposite direction, with the same facing.
		this.head.transform.localPosition = position;
		
		SerpentConsts.Dir oppositeDirection = SerpentConsts.OppositeDirection[ (int) facingDirection ];
		Vector3 oppositeVector = SerpentConsts.DirectionVector3[ (int) oppositeDirection ];
		Vector3 displacement = oppositeVector * 500.0f; // TODO FIX
		Vector3 tailPos = position + displacement;
		
		this.positioning.AddPosition( tailPos ); 

		this.positioning.UpdateHeadPosition(position);
		PositionBodySegments();
	}
	
	private void PositionBodySegments()
	{
		SnakeHead head = this.head;
		SnakeBody bodySegment = head.NextSegment;
		while( bodySegment != null )
		{
			float dist = bodySegment.DistanceFromHead;
			Vector3 pos = this.positioning.GetSegmentPosition( dist );
			bodySegment.transform.localPosition = pos;
			bodySegment = bodySegment.NextSegment;
		}
	}

	public void AddSegment()
	{
		// This method should add a new segment at the end of the snake.  It can be in the same position
		// as the last segment and will appear when the now next-to-last segment moves away from
		// its current position.
		if (this.head == null)
		{
			this.head = SerpentUtils.SerpentInstantiate<SnakeHead>(this.config.HeadPrefab, this.transform);
			this.head.Colour = this.config.Colour;
		}
		else
		{
			SnakeBody newSegment = SerpentUtils.SerpentInstantiate<SnakeBody>(this.config.BodyPrefab, this.transform);
			newSegment.Colour = this.config.Colour;
			
			SnakeSegment last = this.lastSegment;
			
			last.NextSegment = newSegment;
			
			// NOTE, assuming that the segments are the same width and height here.
			float distance = last.Height * 0.5f + newSegment.Height * 0.5f;
			if (last is SnakeBody)
			{
				SnakeBody lastBodySegment = last as SnakeBody;
				distance += lastBodySegment.DistanceFromHead;
			}
			newSegment.DistanceFromHead = distance;			
		}

	}

	public void CheckValidity()
	{
		int numSegments = this.NumSegments;
		if (numSegments == 1)		
		{
			// The body of the snake is all gone.  Time to die!
		}

	}
	
	public override void Update()
	{
		// Update position of head based on speed and direction.  When we reach the centre of a tile, make a callback
		// to the Controller.
		if (this.CurrentDirection != SerpentConsts.Dir.None)
		{
			UpdatePosition();			
		}
		else
		{
			// testing.
			PositionBodySegments();			
		}
		
		// If we reach the centre of a tile, after making the callback, if movement in the current direction is 
		// impossible, stop.
		
		// Check for interactions with other creatures (after moving). 
		
	}	
	
	private void UpdatePosition()
	{
		float displacement =  this.Speed * Time.smoothDeltaTime;
		
		float remainingDisplacement = 0.0f;
		bool arrived = this.head.UpdatePosition( displacement, out remainingDisplacement );
		this.positioning.UpdateHeadPosition(this.head.transform.localPosition);
		PositionBodySegments();
		if (arrived == false) { return; }
		
		ArrivedAtDestination(remainingDisplacement);		
	}
	
	public void ArrivedAtDestination(float remainingDisplacement)
	{		
		// Inform the controller we arrived, and receive the new direction to go in.
		SerpentConsts.Dir newDirection = this.Controller.OnArrival();
		if (newDirection == SerpentConsts.Dir.None) 
		{ 
			this.CurrentDirection = SerpentConsts.Dir.None;
			return; 
		}
		
		bool directionChanged = (newDirection != this.CurrentDirection);
		
		// Change the current direction if it's possible to go that way.
		if (!MotionBlocked( newDirection ))
		{
			this.CurrentDirection = newDirection;
		}
		else if (MotionBlocked( this.CurrentDirection ))
		{
			// stop moving
			this.CurrentDirection = SerpentConsts.Dir.None;
			return;
		}
		
		if (directionChanged)
		{			
			// Note that we changed direction at this point
			this.positioning.AddPosition(this.head.transform.localPosition);
		}
		
		this.head.CurrentDirection = this.CurrentDirection;
		UpdateDestination();
		
		float dummyOutput = 0.0f;
		this.head.UpdatePosition( remainingDisplacement, out dummyOutput );	
		this.positioning.UpdateHeadPosition(this.head.transform.localPosition);		
		PositionBodySegments();
	}
	
	public void ChangeDirection(SerpentConsts.Dir direction)
	{
		// Most of the time we'll ignore this, and wait until we reach an intersection, then ask the controller
		// for the next direction to go in.
		bool process = false;
		
		if (this.CurrentDirection == SerpentConsts.Dir.None)
		{
			process = true;
		}
		else if (direction == SerpentConsts.OppositeDirection[ (int) this.CurrentDirection ])
		{
			// always handle immediately
			process = true;
		}
		
		if (process == false) { return; }
		
		if (MotionBlocked( direction ))
		{
			return;
		}
	
		// Note that we changed direction at this point
		this.positioning.AddPosition(this.head.transform.localPosition);
				
		this.head.CurrentDirection = direction;
		this.CurrentDirection = direction;		
		
		UpdateDestination();
	}
	
	private bool MotionBlocked( SerpentConsts.Dir direction )
	{
		return this.MazeController.MotionBlocked( this.head.transform.localPosition, direction );		
	}
	
	private void UpdateDestination()
	{
		Vector3 newPos = this.MazeController.GetNextCellCentre( this.head.transform.localPosition, this.CurrentDirection );
		this.CurrentDestination = newPos;		
		this.head.CurrentDestination = newPos;
	}
	
	/*
	#region Gizmos
	
	public void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		
		List<SnakePositioning.SnakePosition> positions = this.positioning.Positions;
		
		int i = 0;
		SnakePositioning.SnakePosition headPos = positions[0];
		Vector3 v = headPos.Position;
		v.x = v.x / Screen.width;
		v.y = v.y / Screen.height;
		Gizmos.DrawSphere(v, 0.03f);

		Gizmos.color = Color.yellow;
		
		for( i = 1; i < positions.Count; ++i )
		{
			SnakePositioning.SnakePosition pos = positions[i];
			v = pos.Position;
			v.x = v.x / Screen.width;
			v.y = v.y / Screen.height;
			Gizmos.DrawSphere(v, 0.03f);
		}
		
	
	}
	
	#endregion Gizmos
	*/
}


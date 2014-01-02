using System;
using UnityEngine;
using System.Collections.Generic;

public class Snake : Creature
{
	private bool visible;
	public bool Visible 
	{
		get
		{
			return this.visible;
		}
		
		set
		{
			this.visible = value;
			SnakeSegment segment = this.head;
			while (segment != null)
			{
				segment.Visible = value;
				segment = segment.NextSegment;
			}
		}
	}
	
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
	private Color colour;
	
	private SnakeHead head;
	public SnakeHead Head
	{
		get
		{
			return this.head;
		}
	}
	
	private SnakeTrail trail;
	
	public event Action SnakeSegmentsChanged = null;
	
	#region Set Up
	
	public void SetUp(MazeController mazeController, SnakeConfig config, int numSegments)
	{
		base.SetUp(mazeController);
		
		this.Visible = false;
		
		this.trail = new SnakeTrail();
		
		this.config = config;
		for (int i = 0; i < numSegments; ++i)
		{
			AddSegment();
		}

		if (config.Player)
		{
			this.Controller = new PlayerSnakeController(this, mazeController);			
		}
		else
		{
			this.Controller = new AISnakeController(this, mazeController);
		}		
	}
	
	private void UpdateSpeed()
	{
		this.Speed = this.config.BaseSpeed - this.config.SpeedPenaltyPerSegment * this.NumSegments;
	}
	
	public void SetInitialLocation(Vector3 position, SerpentConsts.Dir facingDirection)
	{
		Vector3 rotation = SerpentConsts.RotationVector3[ (int) facingDirection ];
		SetSegmentsRotation(rotation);
		
		// Set head location to the desired position
		// Each body segment should be laid out in opposite direction
		this.head.transform.localPosition = position;
		
		SerpentConsts.Dir oppositeDirection = SerpentConsts.OppositeDirection[ (int) facingDirection ];
		Vector3 oppositeVector = SerpentConsts.DirectionVector3[ (int) oppositeDirection ];
		Vector3 displacement = oppositeVector * 500.0f; // TODO FIX
		Vector3 tailPos = position + displacement;
		
		this.trail.AddPosition( tailPos ); 

		this.trail.UpdateHeadPosition(position);
		PositionBodySegments();
	}
	
	private void SetSegmentsRotation(Vector3 rotation)
	{
		SnakeSegment segment = this.head;
		while (segment != null)
		{
			segment.transform.eulerAngles = rotation;
			segment = segment.NextSegment;
		}
	}
	
	#endregion Set Up
	
	#region Segments
	
	public void AddSegment()
	{
		// This method should add a new segment at the end of the snake.  It can be in the same position
		// as the last segment and will appear when the now next-to-last segment moves away from
		// its current position.
		if (this.head == null)
		{
			this.head = SerpentUtils.SerpentInstantiate<SnakeHead>(this.config.HeadPrefab, this.transform);
			this.head.Colour = this.colour;
			this.head.Visible = this.visible;
		}
		else
		{
			SnakeBody newSegment = SerpentUtils.SerpentInstantiate<SnakeBody>(this.config.BodyPrefab, this.transform);
			newSegment.Colour = this.colour;
			newSegment.Visible = this.visible;
			
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
		
		if (this.SnakeSegmentsChanged != null)
		{
			SnakeSegmentsChanged();
		}
		UpdateSpeed();
	}
	
	public void ChangeColour(Color newColour)
	{
		this.colour = newColour;
		
		SnakeSegment segment = this.Head;
		while (segment != null)
		{
			segment.Colour = newColour;
			segment = segment.NextSegment;
		}
	}
	
	/// <summary>
	/// Severs snake at segment.
	/// </summary>
	/// <returns><c>true</c>, if the snake should now die <c>false</c> otherwise.</returns>
	/// <param name="segment">Segment.</param>
	public bool SeverAtSegment( SnakeSegment segment )
	{
		SnakeSegment seg = this.head.NextSegment;
		SnakeSegment previousSeg;
		
		bool willDie = (segment == this.head || segment == seg);
		if (willDie)
		{
			seg = this.head;		
			previousSeg = seg;	
		}
		else
		{
			do
			{
				previousSeg = seg;			
				seg = seg.NextSegment;
				if (seg == segment)
				{
					break;
				}
			} while (seg != null);
		}
		
		if (seg == null)
		{
			// error!
			return false;
		}
		
		// destroy this and all subsequent segments
		SnakeSegment nextSegment;
		do
		{
			nextSegment = seg.NextSegment;
			Destroy( seg );
			seg = nextSegment;
		} while (seg != null );	

		// Sever connection to destroyed segment
		previousSeg.NextSegment = null;		
		
		if (this.SnakeSegmentsChanged != null)
		{
			SnakeSegmentsChanged();
		}
		UpdateSpeed();
		
		return willDie;
		
	}
	
	private void Die()
	{
		Destroy(this);
	}

	#endregion Segments
	
	#region Update
	
	public override void Update()
	{
		if (this.Visible == false ) { return; }
		
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
	
	private void PositionBodySegments()
	{
		SnakeHead head = this.head;
		SnakeBody bodySegment = head.NextSegment;
		while( bodySegment != null )
		{
			float dist = bodySegment.DistanceFromHead;
			Vector3 pos = this.trail.GetSegmentPosition( dist );
			bodySegment.transform.localPosition = pos;
			bodySegment = bodySegment.NextSegment;
		}
	}
	
	private void UpdatePosition()
	{
		float displacement =  this.Speed * Time.smoothDeltaTime;
		
		float remainingDisplacement = 0.0f;
		bool arrived = this.head.MoveForward( displacement, out remainingDisplacement );
		this.trail.UpdateHeadPosition(this.head.transform.localPosition);
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
		
		if (directionChanged)
		{			
			// Note that we changed direction at this point
			this.trail.AddPosition(this.head.transform.localPosition);
		}
		
		this.head.CurrentDirection = this.CurrentDirection;
		UpdateDestination();
		
		float dummyOutput = 0.0f;
		this.head.MoveForward( remainingDisplacement, out dummyOutput );	
		this.trail.UpdateHeadPosition(this.head.transform.localPosition);		
		PositionBodySegments();
	}	
	
	#endregion Update
	
	/// <summary>
	/// This method handles direct input to move in a specified direction immediately
	/// rather than when a snake reaches an intersection..
	/// </summary>
	/// <param name="direction">Direction.</param>
	public void StartMoving(SerpentConsts.Dir direction)
	{
		// Most of the time we'll ignore this call, and wait until we reach an intersection, then ask the controller
		// for the next direction to go in.
		
		SerpentConsts.Dir oppositeDirection = SerpentConsts.OppositeDirection[ (int) this.CurrentDirection ];
		if (this.CurrentDirection != SerpentConsts.Dir.None && direction != oppositeDirection)
		{
			return;
		}
		
		if (IsMotionBlocked( direction ))
		{
			return;
		}
	
		// Note that we changed direction at this point
		this.trail.AddPosition(this.head.transform.localPosition);
				
		this.head.CurrentDirection = direction;
		this.CurrentDirection = direction;		
		
		UpdateDestination();
	}
	
	/// <summary>
	/// Determines whether motion is blocked in the specified direction.
	/// </summary>
	/// <returns><c>true</c> if motion is blocked in the specified direction; otherwise, <c>false</c>.</returns>
	/// <param name="direction">Direction.</param>
	private bool IsMotionBlocked( SerpentConsts.Dir direction )
	{
		return this.MazeController.IsMotionBlocked( this.head.transform.localPosition, direction );		
	}
	
	
	/// <summary>
	/// Updates the destination of the snake in the direction it's facing
	/// </summary>
	private void UpdateDestination()
	{
		Vector3 newPos = this.MazeController.GetNextCellCentre( this.head.transform.localPosition, this.CurrentDirection );
		this.CurrentDestination = newPos;		
		this.head.CurrentDestination = newPos;
	}

	#region Interaction with other creatures
	
	public override bool TestForInteraction(Creature otherCreature)
	{
		// get position of this snake's head
		Snake otherSnake = otherCreature as Snake;
		if (otherSnake != null)	
		{
			return TestForInteraction( otherSnake );
		}
		
		// otherwise - check if it's a frog or an egg.
		return false;
	}
	
	private bool TestForInteraction(Snake otherSnake)
	{	
		SnakeHead head = this.head;
		if (CanBiteHead(otherSnake))
		{
			otherSnake.SeverAtSegment(otherSnake.Head);
			AddSegment();
			if (this.Controller is PlayerSnakeController)
			{
				Managers.GameState.Score += 300;
			}
			return true;		
		}
		
		SnakeSegment otherSegment = otherSnake.Head.NextSegment;
		while( otherSegment != null )
		{
			if (head.TouchesSegment( otherSegment ))
			{
				// Sever the snake at that point.
				bool willDie = otherSnake.SeverAtSegment(otherSegment);
				if (willDie)
				{
					AddSegment();
				}
				
				if (this.Controller is PlayerSnakeController)
				{
					Managers.GameState.Score += 100;
				}				
				return willDie;
			}
			
			otherSegment = otherSegment.NextSegment;
		}
		
		return false;
	}
	
	private bool CanBiteHead( Snake otherSnake )
	{
		SnakeHead otherHead = otherSnake.Head;
		if (head.TouchesSegment(otherHead))
		{
			// If this is a head-head collision, check if we can bite this snake.  If not, do nothing
			int numSegments = this.NumSegments;
			int otherSegments = otherSnake.NumSegments;
			if (numSegments < otherSegments)				
			{
				return false;
			}
			else if (numSegments == otherSnake.NumSegments)
			{
				// Note that player snakes lose a tied biting contest
				if (this.Controller is PlayerSnakeController)
				{
					return false;
				}
			}
			return true;
		}
		
		return false;
	}
	
	#endregion Interaction with other creatures
	
}


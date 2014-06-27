using System;
using UnityEngine;
using System.Collections.Generic;
using SerpentExtensions;

public class Snake : MobileCreature
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
	
	private int initialNumSegments;
	
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
	

	public SnakeBody Tail
	{
		get
		{
			SnakeSegment segment = this.head;
			while (segment.NextSegment != null)
			{
				segment = segment.NextSegment;
			}
			return segment as SnakeBody;
		}
	}

	private SnakeConfig config;
	
	// Snake colour now applies only to enemy snakes.
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
	
	public event Action<Snake> SnakeSegmentsChanged = null;
	public event Action<Vector3> SnakeSegmentEaten = null;
	
	protected override Vector3 GetPosition()	
	{
		return this.head.transform.localPosition;
	}
	
	
	#region Set Up
	
	public void SetUp(MazeController mazeController, SnakeConfig config, int numSegments)
	{
		base.SetUp(mazeController);
		
		this.Visible = false;
		this.Dead = false;
		
		this.trail = new SnakeTrail();
		
		this.config = config;
		this.initialNumSegments = numSegments;
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
	
	public void Reset()
	{
		this.Visible = false;
		this.Dead = false;
		
		this.trail.Reset();
		this.Controller.Reset();
		
		this.CurrentDirection = SerpentConsts.Dir.None;
		
		SnakeBody tail = this.Tail;
		if (tail != null)
		{
			tail.RemoveEgg();
		}

		// Restore segments
		int numSegments = this.NumSegments;
		for (int i = numSegments; i < this.initialNumSegments; ++i)	
		{
			Debug.Log ("Adding snake segment to " + this);
			AddSegment();
		}
		// Make sure the head's gameObject is active
		this.head.gameObject.SetActive(true);
		
		// NOTE: if an enemy snake ate the player then it will have an extra segment.  Do we need to remove it?		
	}
	
	override public void Die()
	{
		base.Die();
	}
	
	
	private void UpdateSpeed()
	{
		// Should level factor into this?
		this.Speed = this.config.BaseSpeed - this.config.SpeedPenaltyPerSegment * this.NumSegments;
	}
	
	public override void SetInitialLocation(Vector3 position, SerpentConsts.Dir facingDirection)
	{
		Vector3 rotation = SerpentConsts.RotationVector3[ (int) facingDirection ];
		SetSegmentsRotation(rotation);
		
		// Set head location to the desired position
		// Each body segment should be laid out in opposite direction
		this.head.transform.localPosition = position;
		
		SerpentConsts.Dir oppositeDirection = SerpentConsts.OppositeDirection[ (int) facingDirection ];
		Vector3 oppositeVector = SerpentConsts.DirectionVector3[ (int) oppositeDirection ];
		
		// Add a fake position so the snake segments can be positioned.
		Vector3 displacement = oppositeVector * this.NumSegments * SerpentConsts.CellWidth;
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
		SnakeSegment newSegment = null;
		
		if (this.head == null)
		{
			this.head = SerpentUtils.Instantiate<SnakeHead>(this.config.HeadPrefab, this.transform);
			newSegment = this.head;		
			newSegment.Snake = this;
		}
		else
		{
			SnakeBody newBodySegment = Managers.SnakeBodyCache.GetObject<SnakeBody>();
			newSegment = newBodySegment;
			
			newBodySegment.ResetProperties();			
			newBodySegment.SetParent(this);			
			newBodySegment.Snake = this;
			
			// Configure sprite with prefab.
			newBodySegment.SetSpriteName(this.config.BodySprite.spriteName);
			
			SnakeSegment last = this.Tail;
			if (last == null) { last = this.head; }
			
			last.NextSegment = newBodySegment;
			
			// NOTE, assuming that the segments are the same width and height here.
			SnakeBody lastBodySegment = last as SnakeBody;
			float distance = last.Height * 0.5f + newBodySegment.Height * 0.5f;
			if (lastBodySegment != null)
			{
				distance += lastBodySegment.DistanceFromHead;
			}
			newBodySegment.DistanceFromHead = distance;
			
			if (lastBodySegment != null)
			{
				lastBodySegment.MoveEgg(newBodySegment);
			}
		}
		
		newSegment.Visible = this.visible;		
		
		if (this.config.Player)
		{		
			int firstColourIndex = (this.NumSegments - 1) % (SerpentConsts.PlayerSegmentColours.Length);
			Color firstColor = SerpentConsts.PlayerSegmentColours[firstColourIndex];
			
			int secondColourIndex = (firstColourIndex + 1) % (SerpentConsts.PlayerSegmentColours.Length);
			Color secondColor = SerpentConsts.PlayerSegmentColours[secondColourIndex];			

			newSegment.SetGradatedColour(firstColor, secondColor);			
		}
		else
		{
			newSegment.Colour = this.colour;
		}
		
		if (this.SnakeSegmentsChanged != null)
		{
			this.SnakeSegmentsChanged(this);
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
			this.head.gameObject.SetActive(false);
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

		// Return eaten/destroyed segments to the cache.				
		SnakeSegment nextSegment = seg.NextSegment;
		
		// Sever connection to destroyed segments prior to calling destroy but AFTER hanging a pointer to the next 
		// segment, in case previousSeg and seg are the same.
		previousSeg.NextSegment = null;		
		
		// NOTE: Can't return snake head to the snake body cache.
		do
		{
			if (this.SnakeSegmentEaten != null)
			{
				this.SnakeSegmentEaten(seg.gameObject.transform.localPosition);
			}
			
			if (seg != this.head)
			{
				Managers.SnakeBodyCache.ReturnObject<SnakeBody>(seg.gameObject);
				Debug.Log("Returned segment to snake body cache");
			}
			seg = nextSegment;
			if (seg == null)
			{
				break;
			}
			nextSegment = seg.NextSegment;
		} while ( true );

		if ( this.SnakeSegmentsChanged != null )
		{
			this.SnakeSegmentsChanged( this );
		}
		UpdateSpeed();
		
		return willDie;
	}
	
	// Method to call on player snake when transitioning levels
	public void ReturnToCache()
	{
		// Return eaten/destroyed segments to the cache.				
		SnakeSegment seg = this.head;
		SnakeSegment previousSeg = seg;		
		SnakeSegment nextSegment = seg.NextSegment;
		
		// Sever connection to destroyed segments prior to calling destroy but AFTER hanging a pointer to the next 
		// segment, in case previousSeg and seg are the same.
		previousSeg.NextSegment = null;		
		
		// NOTE: Can't return snake head to the snake body cache.
		do
		{
			if (seg != this.head)
			{
				Managers.SnakeBodyCache.ReturnObject<SnakeBody>(seg.gameObject);
				Debug.Log("Returned segment to snake body cache");
			}
			seg = nextSegment;
			if (seg == null)
			{
				break;
			}
			nextSegment = seg.NextSegment;
		} while ( true );
	
		this.head = null;
		
	//	this.head.gameObject.SetActive(false);
		UpdateSpeed();
		
	}
	
	#endregion Segments
	
	#region Update
	
	public override void Update()
	{
		if (this.Visible == false || this.Dead) 
		{
			return; 
		}
		
		// Update position of head based on speed and direction.  When we reach the centre of a tile, make a callback
		// to the Controller.
		if (this.CurrentDirection != SerpentConsts.Dir.None)
		{
			UpdatePosition();			
		}
		else
		{
			PositionBodySegments();			
		}
	}		
	
	/*
	private void UpdatePosition()
	{
		float displacement =  this.Speed * Time.smoothDeltaTime;
		
		float remainingDisplacement = 0.0f;
		bool arrived = MoveForward( displacement, out remainingDisplacement );
		
		if (arrived == false) { return; }
		
		ArrivedAtDestination(remainingDisplacement);		
	}
	*/
	
	public override bool MoveForward( float displacement, out float remainingDisplacement )
	{
		bool arrived = this.head.MoveForward( displacement, out remainingDisplacement );			
		this.trail.UpdateHeadPosition( this.head.transform.localPosition );
		PositionBodySegments();
		
		return arrived;
	}
	
	protected override void OnDirectionChange()
	{
		this.head.CurrentDirection = this.CurrentDirection;
		this.trail.AddPosition(this.head.transform.localPosition);
	}
	
	private void PositionBodySegments()
	{
		this.trail.PositionSegments( this.head );
	}
	
	#endregion Update
	
	/// <summary>
	/// This method handles direct input to move in a specified direction immediately
	/// rather than when a snake reaches an intersection..
	/// </summary>
	/// <param name="direction">Direction.</param>
	public override void StartMoving(SerpentConsts.Dir direction)
	{
		// Most of the time we'll ignore this call, and wait until we reach an intersection, then ask the controller
		// for the next direction to go in.
		
		SerpentConsts.Dir oppositeDirection = SerpentConsts.OppositeDirection[ (int) this.CurrentDirection ];
		if (this.CurrentDirection != SerpentConsts.Dir.None && direction != oppositeDirection)
		{
			return;
		}
		
		base.StartMoving(direction);
		
		/*
		if (IsMotionBlocked( direction ))
		{
			return;
		}
		
		// Make sure any door that needs to open, opens
		OpenDoor( direction );
	
		// Note that we changed direction at this point
		this.trail.AddPosition(this.head.transform.localPosition);
				
		this.head.CurrentDirection = direction;
		this.CurrentDirection = direction;		
		
		UpdateDestination();
		*/
	}
	
	protected override void OpenDoor( SerpentConsts.Dir direction )
	{
		this.MazeController.OpenDoor( GetPosition(), direction );
	}
	
	protected override void CloseDoor()
	{
		SnakeSegment tail = this.Tail;
		SerpentConsts.Dir tailDir = tail.CurrentDirection;
		if (tailDir == SerpentConsts.Dir.None)
		{
			return;
		}
		SerpentConsts.Dir behindDir = SerpentConsts.OppositeDirection[ (int) tailDir ];
		//SerpentConsts.Dir tailDir = DetermineTailDirection();
		this.MazeController.CloseDoor( tail.transform.localPosition, behindDir );
	}
	
	private SerpentConsts.Dir DetermineTailDirection()
	{
		if (this.head.NextSegment == null)
		{
			Debug.Log("Snake without a body!");
		}
		SnakeSegment previousSegment = this.head;
		SnakeSegment segment = previousSegment.NextSegment;
 			while (segment.NextSegment != null)
		{
			previousSegment = segment;
			segment = segment.NextSegment;
		}
		
		Vector3 toTail = segment.transform.localPosition - previousSegment.transform.localPosition;
		float xDelta = toTail.x;
		float yDelta = toTail.y;
		if ( Mathf.Abs(xDelta) > Mathf.Abs(yDelta) )
		{
			if (xDelta < 0.0f)
			{
				return SerpentConsts.Dir.W;
			}
			else
			{
				return SerpentConsts.Dir.E;
			}
		}
		else
		{
			if (yDelta < 0.0f)
			{
				return SerpentConsts.Dir.S;				
			}
			else
			{
				return SerpentConsts.Dir.N;
			}
		}
	}
	
	/// <summary>
	/// Updates the destination of the snake in the direction its facing
	/// </summary>
	protected override void UpdateDestination()
	{
		Vector3 newPos = this.MazeController.GetNextCellCentre( this.head.transform.localPosition, this.CurrentDirection );
		this.CurrentDestination = newPos;		
		this.head.CurrentDestination = newPos;
	}

	#region Interaction with other creatures
	
	public override bool TestForInteraction(Creature otherCreature)
	{
		Snake otherSnake = otherCreature as Snake;
		if (otherSnake != null)	
		{
			return TestForInteraction( otherSnake );
		}
		
		// Otherwise test with the snake's head versus the other creature's own transform position
		if (this.head.TouchesCreature(otherCreature))
		{
			if (otherCreature is Egg || otherCreature is Frog)
			{
				this.AddSegment();
			}
			otherCreature.Die();
			
			// returning true may now be irrelevant.
			return true;
		}
		
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
			otherSnake.Die();
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
					otherSnake.Die();					
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
	
	#region Eggs
	
	public GameObject GetEggPrefab()
	{
		return this.config.EggPrefab;
	}
	
	#endregion Eggs
}


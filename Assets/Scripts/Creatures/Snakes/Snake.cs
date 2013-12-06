using System;
using UnityEngine;

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
	
	public void SetUp(MazeController mazeController, SnakeConfig config, int numSegments, bool playerControlled)
	{
		base.SetUp(mazeController);
		
		this.config = config;
		for (int i = 0; i < numSegments; ++i)
		{
			AddSegment();
		}

		if (playerControlled)
		{
			this.Controller = new PlayerSnakeController();
		}
		else
		{
			this.Controller = new AISnakeController();
		}
	}
	
	public void SetLocation(Vector3 position, SerpentConsts.Dir facingDirection)
	{
		// Set head location to the desired position and facing
		// Each body segment should be laid out in opposite direction, with the same facing.
		// For now just the head TODO FIX
		
		this.head.transform.localPosition = position;
	}

	public void AddSegment()
	{
		// This method should add a new segment at the end of the snake.  It can be in the same position
		// as the last segment and will appear when the now next-to-last segment moves away from
		// its current position.
		if (this.head == null)
		{
			GameObject newObj = (GameObject) Instantiate(this.config.HeadPrefab, new Vector3(0,0,0), Quaternion.identity);

			this.head = newObj.GetComponent<SnakeHead>();
			// how do we set head position...?
		}
		else
		{
			GameObject newObj = (GameObject) Instantiate(this.config.BodyPrefab, new Vector3(0,0,0), Quaternion.identity);
			SnakeSegment newSegment = newObj.GetComponent<SnakeSegment>();

			SnakeSegment lastSegment = this.lastSegment;
			lastSegment.NextSegment = newSegment;

			newSegment.transform.localPosition = lastSegment.transform.localPosition;
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
		
		// If we reach the centre of a tile, after making the callback, if movement in the current direction is 
		// impossible, stop.
		
		// Check for interactions with other creatures (after moving). 
		
	}	
	
	private void UpdatePosition()
	{
		float displacement =  this.Speed * Time.smoothDeltaTime;
		
		float remainingDisplacement;
		bool arrived = this.head.UpdatePosition( displacement, out remainingDisplacement );
		if (arrived == false) { return; }
		
		// Inform the controller we arrived, and receive the new direction to go in.
		this.CurrentDirection = this.Controller.OnArrival();		
		// TODO update new destination position
		
		if (this.CurrentDirection == SerpentConsts.Dir.None) { return; }
		this.head.CurrentDirection = this.CurrentDirection;

		float dummyOutput = 0.0f;
		this.head.UpdatePosition( remainingDisplacement, out dummyOutput );	
	}
}


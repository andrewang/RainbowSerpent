	using System;

public class PlayerSnakeController : SnakeController
{
	/// <summary>
	/// The desired direction.  If that direction is currently blocked, the snake will turn in that direction
	/// at the next possible opportunity.
	/// </summary>
	private SerpentConsts.Dir desiredDirection = SerpentConsts.Dir.None;
	
	public bool PlayerControlled { get; set; }
	
	// Destination for snake when not player controlled?
	
	public PlayerSnakeController( Creature creature, MazeController mazeController ) : base( creature, mazeController )
	{
		this.PlayerControlled = true;
	}
	
	public override void Reset()
	{
		this.desiredDirection = SerpentConsts.Dir.None;
	}
	
	public override void StartMoving(SerpentConsts.Dir direction)
	{
		if (direction == this.desiredDirection) { return; }
		this.snake.StartMoving(direction);

		// Else we'll turn to that direction at the next intersection.
	
		// Either way, store the desired direction.
		this.desiredDirection = direction;
	}
	
	/// <summary>
	/// Handles the arrival event - the creature arriving at the centre of a tile.  Returns the
	/// direction to travel in from this point on.
	/// </summary>
	public override SerpentConsts.Dir OnArrival()	
	{
		if (this.PlayerControlled == false)
		{
		}
		
		return this.desiredDirection;
	}
	
}


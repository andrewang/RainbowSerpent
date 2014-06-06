	using System;
using UnityEngine;
using System.Collections.Generic;

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
		if (Managers.GameState.LevelState == SerpentConsts.LevelState.LevelEnd)
		{
			return GoToPlayerZone();
		}
		
		if (Managers.GameState.LevelState == SerpentConsts.LevelState.LevelStart)
		{
			return ExitPlayerZone();
		}
		
		return this.desiredDirection;
		
	}
	
	private SerpentConsts.Dir GoToPlayerZone()
	{
		Maze maze = this.mazeController.Maze;
		IntVector2 targetPos = maze.PlayerZoneEntrance;
		return HeadTowards(targetPos);
	}
	
	private SerpentConsts.Dir ExitPlayerZone()
	{
		Maze maze = this.mazeController.Maze;
		IntVector2 targetPos = maze.PlayerZoneExit;
		return HeadTowards(targetPos);
	}
	
	private SerpentConsts.Dir HeadTowards(IntVector2 targetPos)
	{
		// Check if it's possible to turn.  If not, return the current direction
		SerpentConsts.Dir currentDirection = this.snake.Head.CurrentDirection;
		SerpentConsts.Dir oppositeDirection = SerpentConsts.OppositeDirection[ (int) currentDirection ];
		
		List<SerpentConsts.Dir> availableDirections = GetAvailableDirections();
		availableDirections.Remove(oppositeDirection);
		
		if (availableDirections.Count == 0) 
		{
			return this.desiredDirection;
		}
		if (availableDirections.Count == 1)
		{
			return availableDirections[0];
		}
			
		// Try a y direction choice first.  Otherwise pick between x choices.  If all else fails go in opposite y.		
		SnakeHead head = this.snake.Head;
		Vector3 headPos = head.transform.localPosition;
		MazeCell headMazeCell = this.mazeController.GetCellForPosition( headPos );
		
		SerpentConsts.Dir bestYDir = GetBestYDirection(headMazeCell.Y, targetPos.y);
		SerpentConsts.Dir bestXDir = GetBestXDirection(headMazeCell.X, targetPos.x);
		
		if (availableDirections.Contains(bestYDir))
		{
			return bestYDir;
		}
		else if (availableDirections.Contains(bestXDir))
		{
			return bestXDir;
		}
		else
		{
			// pick something at random...
			int randomIndex = UnityEngine.Random.Range(0, availableDirections.Count);			
			return availableDirections[randomIndex];
		}
		
		/*
		if (headMazeCell.Y > targetPos.y && !this.mazeController.IsMotionBlocked( headPos, SerpentConsts.Dir.S))
		{
			return SerpentConsts.Dir.S;
		}
		else if (headMazeCell.Y < targetPos.y && !this.mazeController.IsMotionBlocked( headPos, SerpentConsts.Dir.N))
		{
			return SerpentConsts.Dir.N;
		}
		
		if (headMazeCell.X > targetPos.x && !this.mazeController.IsMotionBlocked( headPos, SerpentConsts.Dir.W ))
		{
			return SerpentConsts.Dir.W;
		}
		else if (headMazeCell.X < targetPos.x && !this.mazeController.IsMotionBlocked( headPos, SerpentConsts.Dir.E ))
		{
			return SerpentConsts.Dir.E;
		}
		else if (!this.mazeController.IsMotionBlocked( headPos, SerpentConsts.Dir.W))
		{
			return SerpentConsts.Dir.W;
		}
		else if (!this.mazeController.IsMotionBlocked( headPos, SerpentConsts.Dir.E))
		{
			return SerpentConsts.Dir.E;
		}
		
		if (headMazeCell.Y < targetPos.y && !this.mazeController.IsMotionBlocked( headPos, SerpentConsts.Dir.N))
		{
			return SerpentConsts.Dir.N;
		}
		else if (headMazeCell.Y > targetPos.y && !this.mazeController.IsMotionBlocked( headPos, SerpentConsts.Dir.S))
		{
			return SerpentConsts.Dir.S;
		}
				
		return SerpentConsts.Dir.None;
		*/
	}
	
	private SerpentConsts.Dir GetBestYDirection(int headY, int targetY)
	{
		if (headY > targetY)
		{
			return SerpentConsts.Dir.S;
		}
		else if (headY != targetY)
		{
			return SerpentConsts.Dir.N;
		}
		else
		{
			return SerpentConsts.Dir.None;
		}
	}
	
	private SerpentConsts.Dir GetBestXDirection(int headX, int targetX)
	{
		if (headX > targetX)
		{
			return SerpentConsts.Dir.W;
		}
		else if (headX != targetX)
		{
			return SerpentConsts.Dir.E;
		}
		else
		{
			return SerpentConsts.Dir.None;
		}
	}
}


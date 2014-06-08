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
	private bool reachedPlayerZone = false;
	
	public bool PlayerControlled { get; set; }
	
	public event Action<Snake> SnakeReturnedToStart = null;
	
	// Destination for snake when not player controlled?
	
	public PlayerSnakeController( Creature creature, MazeController mazeController ) : base( creature, mazeController )
	{
		this.PlayerControlled = true;
	}
	
	public override void Reset()
	{
		this.desiredDirection = SerpentConsts.Dir.None;
		this.reachedPlayerZone = false;
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
		
		SerpentConsts.Dir dir = SerpentConsts.Dir.None;
		if (this.reachedPlayerZone == false)
		{
			dir = HeadTowards(targetPos);
			if (dir == SerpentConsts.Dir.None)
			{
				this.reachedPlayerZone = true;
			}
		}
		
		if (this.reachedPlayerZone == true)
		{
			dir = HeadTowards(maze.PlayerZoneExit);
			if (dir == SerpentConsts.Dir.None)
			{
				// trigger game manager, and for now keep same direction
				// need to trigger... something...
				SnakeReturnedToStart(this.snake);
				// return "no direction" so that snake code doesn't execute anything else.
				dir = SerpentConsts.Dir.None;
			}
		}
		return dir;
	}
	
	private SerpentConsts.Dir ExitPlayerZone()
	{
		Maze maze = this.mazeController.Maze;
		IntVector2 targetPos = maze.PlayerZoneExit;
		SerpentConsts.Dir dir = HeadTowards(targetPos);
		if (dir == SerpentConsts.Dir.None)
		{
			// keep same direction
			dir = this.desiredDirection;
		}
		return dir;
	}
	
	private SerpentConsts.Dir HeadTowards(IntVector2 targetPos)
	{
		// Is the snake's head already there?
		MazeCell headMazeCell = GetCellForHeadPosition();
		if (headMazeCell == null || (headMazeCell.X == targetPos.x && headMazeCell.Y == targetPos.y))
		{
			return SerpentConsts.Dir.None;
		}
		
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
		
		SerpentConsts.Dir bestYDir = GetBestYDirection(headMazeCell.Y, targetPos.y);
		SerpentConsts.Dir bestXDir = GetBestXDirection(headMazeCell.X, targetPos.x);
		
		SerpentConsts.Dir bestDir;
		SerpentConsts.Dir secondBestDir;
		
		if (bestYDir == SerpentConsts.Dir.None)
		{
			bestDir = bestXDir;
			secondBestDir = bestYDir;
		}
		else if (bestXDir == SerpentConsts.Dir.None)
		{
			bestDir = bestYDir;
			secondBestDir = bestXDir;
		}
		else if (UnityEngine.Random.Range(0,2) == 0)
		{
			bestDir = bestYDir;
			secondBestDir = bestXDir;
		}
		else
		{
			bestDir = bestXDir;
			secondBestDir = bestYDir;
		}
				
		if (availableDirections.Contains(bestDir))
		{
			return bestDir;
		}
		else if (secondBestDir != SerpentConsts.Dir.None && availableDirections.Contains(secondBestDir))
		{
			return secondBestDir;
		}
		else
		{
			// pick something at random...
			int randomIndex = UnityEngine.Random.Range(0, availableDirections.Count);			
			return availableDirections[randomIndex];
		}
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
	
	private MazeCell GetCellForHeadPosition()
	{
		SnakeHead head = this.snake.Head;
		Vector3 headPos = head.transform.localPosition;
		MazeCell headMazeCell = this.mazeController.GetCellForPosition(headPos);
		return headMazeCell;
	}
}


using System;
using UnityEngine;
using System.Collections.Generic;
using Serpent;

public class PlayerSnakeController : SnakeController
{
	/// <summary>
	/// The desired direction.  If that direction is currently blocked, the snake will turn in that direction
	/// at the next possible opportunity.
	/// </summary>
	private Direction desiredDirection = Direction.None;
	private bool reachedPlayerZone = false;
	
	public bool PlayerControlled { get; set; }
	
	public event Action<Snake> SnakeReturnedToStart = null;
	public event Action<Snake> SnakeExitedStart = null;
	
	
	public PlayerSnakeController( Creature creature, MazeController mazeController ) : base( creature, mazeController )
	{
		this.PlayerControlled = true;
	}
	
	public override void Reset()
	{
		this.desiredDirection = Direction.None;
		this.reachedPlayerZone = false;
	}
	
	public void SetDesiredDirection(Direction direction)
	{
		// This method sets the desired direction but nothing else, so it's only used in the situation where the player doesn't have
		// control of their snake.
		this.desiredDirection = direction;
	}
	
	public override void StartMoving(Direction direction)
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
	public override Direction NewDirectionUponArrival()	
	{
		if (Managers.GameState.LevelState == LevelState.LevelEnd)
		{
			return GoToPlayerZone();
		}
		
		if (Managers.GameState.LevelState == LevelState.LevelStart)
		{
			// When the player has exited the player zone then change the level state and give the player control.
			MazeCell headMazeCell = GetCellForHeadPosition();
			if (headMazeCell.InPlayerZone == false)
			{
				this.PlayerControlled = true;
				if (this.SnakeExitedStart != null)
				{
					SnakeExitedStart(this.snake);
				}
			}
			else
			{			
				return ExitPlayerZone();
			}
		}
		
		return this.desiredDirection;
		
	}
	
	private Direction GoToPlayerZone()
	{
		Maze maze = this.mazeController.Maze;
		IntVector2 targetPos = maze.PlayerStartZoneEntrance;
		
		Direction dir = Direction.None;
		if (this.reachedPlayerZone == false)
		{
			dir = HeadTowards(targetPos);
			if (dir == Direction.None)
			{
				this.reachedPlayerZone = true;
			}
		}
		
		if (this.reachedPlayerZone == true)
		{
			dir = HeadTowards(maze.PlayerStartZoneExit);
			if (dir == Direction.None)
			{
				// trigger event for return.
				if (this.SnakeReturnedToStart != null)
				{
					SnakeReturnedToStart(this.snake);
				}
				// return "no direction" so that snake code doesn't execute anything else.
				dir = Direction.None;
			}
		}
		return dir;
	}
	
	private Direction ExitPlayerZone()
	{
		Maze maze = this.mazeController.Maze;
		IntVector2 targetPos = maze.PlayerStartZoneExit;
		Direction dir = HeadTowards(targetPos);
		if (dir == Direction.None)
		{
			// keep same direction
			dir = this.desiredDirection;
		}
		return dir;
	}
	
	private Direction HeadTowards(IntVector2 targetPos)
	{
		// Is the snake's head already there?
		MazeCell headMazeCell = GetCellForHeadPosition();
		if (headMazeCell == null || (headMazeCell.X == targetPos.x && headMazeCell.Y == targetPos.y))
		{
			return Direction.None;
		}
		
		Direction currentDirection = this.snake.Head.CurrentDirection;
		Direction oppositeDirection = SerpentConsts.OppositeDirection[ (int) currentDirection ];
		
		List<Direction> availableDirections = GetAvailableDirections();
		availableDirections.Remove(oppositeDirection);
		
		// TODO REFACTOR, THIS IS CLUNKY!

		// Handle no-turn situations.		
		if (availableDirections.Count == 0) 
		{
			return this.desiredDirection;
		}
		if (availableDirections.Count == 1)
		{
			return availableDirections[0];
		}
		
		// We're at an intersection with a chioce.
				
		Direction bestYDir = GetBestYDirection(headMazeCell.Y, targetPos.y);
		Direction bestXDir = GetBestXDirection(headMazeCell.X, targetPos.x);
		
		Direction bestDir;
		Direction secondBestDir;
		
		// Figure out our preferred and second-preferred directions
		if (bestYDir == Direction.None)
		{
			// Can't turn towards where we want to go on the Y axis, so take the X-axis direction.
			bestDir = bestXDir;
			secondBestDir = bestYDir;
		}
		else if (bestXDir == Direction.None)
		{
			// Can't turn towards where we want to go on the X axis, so take the Y-axis direction.
			bestDir = bestYDir;
			secondBestDir = bestXDir;
		}		
		// Prefer x choice over y, if both are available.  This assumes there's a clear path along the
		// leftmost column to the player start (so it's sort of cheating?)
		else
		{
			bestDir = bestXDir;
			secondBestDir = bestYDir;
		}
			
		if (availableDirections.Contains(bestDir))
		{
			return bestDir;
		}
		else if (secondBestDir != Direction.None && availableDirections.Contains(secondBestDir))
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
	
	private MazeCell GetCellForHeadPosition()
	{
		SnakeHead head = this.snake.Head;
		Vector3 headPos = head.transform.localPosition;
		MazeCell headMazeCell = this.mazeController.GetCellForPosition(headPos);
		return headMazeCell;
	}
}


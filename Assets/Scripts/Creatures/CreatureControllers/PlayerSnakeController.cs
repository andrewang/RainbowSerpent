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
	public event Action<Snake> SnakeExitedStart = null;
	
	
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
	public override SerpentConsts.Dir NewDirectionUponArrival()	
	{
		if (Managers.GameState.LevelState == SerpentConsts.LevelState.LevelEnd)
		{
			return GoToPlayerZone();
		}
		
		if (Managers.GameState.LevelState == SerpentConsts.LevelState.LevelStart)
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
	
	private SerpentConsts.Dir GoToPlayerZone()
	{
		Maze maze = this.mazeController.Maze;
		IntVector2 targetPos = maze.PlayerStartZoneEntrance;
		
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
			dir = HeadTowards(maze.PlayerStartZoneExit);
			if (dir == SerpentConsts.Dir.None)
			{
				// trigger event for return.
				if (this.SnakeReturnedToStart != null)
				{
					SnakeReturnedToStart(this.snake);
				}
				// return "no direction" so that snake code doesn't execute anything else.
				dir = SerpentConsts.Dir.None;
			}
		}
		return dir;
	}
	
	private SerpentConsts.Dir ExitPlayerZone()
	{
		Maze maze = this.mazeController.Maze;
		IntVector2 targetPos = maze.PlayerStartZoneExit;
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
		
		SerpentConsts.Dir currentDirection = this.snake.Head.CurrentDirection;
		SerpentConsts.Dir oppositeDirection = SerpentConsts.OppositeDirection[ (int) currentDirection ];
		
		List<SerpentConsts.Dir> availableDirections = GetAvailableDirections();
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
				
		SerpentConsts.Dir bestYDir = GetBestYDirection(headMazeCell.Y, targetPos.y);
		SerpentConsts.Dir bestXDir = GetBestXDirection(headMazeCell.X, targetPos.x);
		
		SerpentConsts.Dir bestDir;
		SerpentConsts.Dir secondBestDir;
		
		// Figure out our preferred and second-preferred directions
		if (bestYDir == SerpentConsts.Dir.None)
		{
			// Can't turn towards where we want to go on the Y axis, so take the X-axis direction.
			bestDir = bestXDir;
			secondBestDir = bestYDir;
		}
		else if (bestXDir == SerpentConsts.Dir.None)
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
	
	private MazeCell GetCellForHeadPosition()
	{
		SnakeHead head = this.snake.Head;
		Vector3 headPos = head.transform.localPosition;
		MazeCell headMazeCell = this.mazeController.GetCellForPosition(headPos);
		return headMazeCell;
	}
}


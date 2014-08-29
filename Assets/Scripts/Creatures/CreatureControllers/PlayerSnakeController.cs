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
	private bool insidePlayerZone = false;
	
	private bool playerControlled;
	public bool PlayerControlled 
	{
		get
		{
			return this.playerControlled;
		}
		set
		{
			this.playerControlled = value; 
			if (value == false)
			{
				if (this.snake.CurrentDirection == Direction.None)
				{
					// Not moving so start moving in a new direction
					Direction newDir = GoToPlayerZone();
					StartMoving(newDir);
				}
			}
		}		
	}
	
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
		this.insidePlayerZone = false;
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
			dir = HeadTowards(maze.PlayerStartZoneCentre);
			if (dir == Direction.None)
			{
				this.insidePlayerZone = true;
			}
		}
		
		if (this.insidePlayerZone == true)
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
		
		// Handle no-turn situations.		
		if (availableDirections.Count == 0) 
		{
			return this.desiredDirection;
		}
		if (availableDirections.Count == 1)
		{
			return availableDirections[0];
		}
		
		return DecideAtIntersection(targetPos, availableDirections);
	}
	
	private Direction DecideAtIntersection(IntVector2 targetPos, List<Direction> availableDirections)
	{
		MazeCell headMazeCell = GetCellForHeadPosition();
		
		Direction[] prioritizedDirections = new Direction[(int)Direction.Count];
		
		// We're at an intersection with a choice.
		Direction bestDirection = GetBestDirection(headMazeCell.X, headMazeCell.Y, targetPos.x, targetPos.y);
		Direction worstDirection = SerpentConsts.OppositeDirection[ (int)bestDirection ];
		
		prioritizedDirections[0] = bestDirection;
		prioritizedDirections[(int)(Direction.Count - 1)] = worstDirection;
		
		// Look at the axis which isn't equal to best now, and fill in indexes 1 and 2
		if (bestDirection == Direction.N || bestDirection == Direction.S)
		{
			// W/E
			Direction bestXDir = GetBestXDirection(headMazeCell.X, targetPos.x);
			if (bestXDir == Direction.None)
			{
				// X values are equal, so do a random selection
				int random = UnityEngine.Random.Range(0,2);
				if (random == 0)
				{
					bestXDir = Direction.W;
				}
				else
				{
					bestXDir = Direction.E;
				}
			}
			prioritizedDirections[1] = bestXDir;
			prioritizedDirections[2] = SerpentConsts.OppositeDirection[ (int)bestXDir ];
		}
		else
		{
			// N/S
			Direction bestYDir = GetBestYDirection(headMazeCell.Y, targetPos.y);
			if (bestYDir == Direction.None)
			{
				// Y values are equal, so do a random selection
				int random = UnityEngine.Random.Range(0,2);
				if (random == 0)
				{
					bestYDir = Direction.N;
				}
				else
				{
					bestYDir = Direction.S;
				}
			}
			prioritizedDirections[1] = bestYDir;
			prioritizedDirections[2] = SerpentConsts.OppositeDirection[ (int)bestYDir ];
		}
		
		for (int index = 0; index < prioritizedDirections.Length; ++index)
		{
			if (availableDirections.Contains(prioritizedDirections[index]))
			{
				return prioritizedDirections[index];
			}
		}
		
		return Direction.None;
	}
	
	private MazeCell GetCellForHeadPosition()
	{
		SnakeHead head = this.snake.Head;
		Vector3 headPos = head.transform.localPosition;
		MazeCell headMazeCell = this.mazeController.GetCellForPosition(headPos);
		return headMazeCell;
	}
}


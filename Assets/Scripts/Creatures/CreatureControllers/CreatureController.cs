using System;
using UnityEngine;
using System.Collections.Generic;
using Serpent;

public class CreatureController
{
	protected Creature creature;
	protected MazeController mazeController;
	
	public CreatureController( Creature creature, MazeController mazeController )
	{
		this.creature = creature;
		this.mazeController = mazeController;
	}
	
	public virtual void Reset()
	{
	}
	
	public virtual void StartMoving(Direction direction)
	{
	}

	/// <summary>
	/// Handles the arrival event - the creature arriving at the centre of a tile.  Returns the
	/// direction to travel in from this point on.
	/// </summary>
	public virtual Direction NewDirectionUponArrival()	
	{
		return Direction.None;
	}
	
	protected virtual List<Direction> GetAvailableDirections()
	{
		Vector3 position = this.creature.transform.localPosition;
		MazeCell cell = this.mazeController.GetCellForPosition( position );
		
		List<Direction> availableDirections = cell.UnblockedDirections;		
		return availableDirections;
	}
	
	protected Direction GetBestDirection(int currX, int currY, int targetX, int targetY)
	{
		int diffX = Math.Abs(targetX - currX);
		int diffY = Math.Abs(targetY - currY);
		if (diffX > diffY)
		{
			return GetBestXDirection(currX, targetX);
		}
		else if (diffX < diffY)
		{
			return GetBestYDirection(currY, targetY);
		}
		else
		{
			// random selection
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				return GetBestXDirection(currX, targetX);				
			}
			else
			{
				return GetBestYDirection(currY, targetY);				
			}
		}
	}
		
	protected Direction GetBestYDirection(int currY, int targetY)
	{
		if (currY > targetY)
		{
			return Direction.S;
		}
		else if (currY != targetY)
		{
			return Direction.N;
		}
		else
		{
			return Direction.None;
		}
	}
	
	protected Direction GetBestXDirection(int currX, int targetX)
	{
		if (currX > targetX)
		{
			return Direction.W;
		}
		else if (currX != targetX)
		{
			return Direction.E;
		}
		else
		{
			return Direction.None;
		}
	}
}

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

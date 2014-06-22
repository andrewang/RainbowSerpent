using System;
using UnityEngine;
using System.Collections.Generic;

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
	
	public virtual void StartMoving(SerpentConsts.Dir direction)
	{
	}

	/// <summary>
	/// Handles the arrival event - the creature arriving at the centre of a tile.  Returns the
	/// direction to travel in from this point on.
	/// </summary>
	public virtual SerpentConsts.Dir NewDirectionUponArrival()	
	{
		return SerpentConsts.Dir.None;
	}
	
	protected virtual List<SerpentConsts.Dir> GetAvailableDirections()
	{
		Vector3 position = this.creature.transform.localPosition;
		MazeCell cell = this.mazeController.GetCellForPosition( position );
		
		List<SerpentConsts.Dir> availableDirections = cell.UnblockedDirections;		
		return availableDirections;
	}
}

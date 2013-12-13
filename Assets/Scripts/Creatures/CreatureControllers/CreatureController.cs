using System;

public class CreatureController
{
	protected Creature creature;
	protected MazeController mazeController;
	
	public CreatureController( Creature creature, MazeController mazeController )
	{
		this.creature = creature;
		this.mazeController = mazeController;
	}
	
	public virtual void ChangeDirection(SerpentConsts.Dir direction)
	{
		//this.Creature
	}

	/// <summary>
	/// Handles the arrival event - the creature arriving at the centre of a tile.  Returns the
	/// direction to travel in from this point on.
	/// </summary>
	public virtual SerpentConsts.Dir OnArrival()	
	{
		return SerpentConsts.Dir.None;
	}
}

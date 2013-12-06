using System;

public class CreatureController
{
	private Creature creature;
	
	public void SetUp( Creature creature, Maze mazeController )
	{
		this.creature = creature;
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

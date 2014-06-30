using System;
using UnityEngine;
using System.Collections.Generic;
using Serpent;

public class AISnakeController : SnakeController
{
	public AISnakeController( Creature creature, MazeController mazeController ) : base( creature, mazeController )
	{
	}	
	
	/// <summary>
	/// Handles the arrival event - the creature arriving at the centre of a tile.  Returns the
	/// direction to travel in from this point on.
	/// </summary>
	public override Direction NewDirectionUponArrival()	
	{
		// Get our current direction first.  We want to turn in a random direction but not do a reverse turn.
		// So assemble a list of all the directions, excluding the opposite direction to the current, and also
		// excluding directions where motion is blocked.		
		Direction currentDirection = this.snake.Head.CurrentDirection;
		Direction oppositeDirection = SerpentConsts.OppositeDirection[ (int) currentDirection ];
		
		List<Direction> availableDirections = GetAvailableDirections();
		availableDirections.Remove(oppositeDirection);
			
		if (availableDirections.Count == 0) 
		{
			return Direction.N; 
		}
		
		int randomIndex = UnityEngine.Random.Range(0, availableDirections.Count);
		Direction randomDir = availableDirections[randomIndex];
		return randomDir;
	}
	
	
	
}



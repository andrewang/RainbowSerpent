using System;
using System.Collections.Generic;
using UnityEngine;
using Serpent;

public class SnakeController : CreatureController
{	
	protected Snake snake;
	
	public SnakeController( Creature creature, MazeController mazeController ) : base( creature, mazeController )
	{
		this.snake = (Snake) creature;
	}
	
	public override void StartMoving(Direction direction)
	{
		this.snake.StartMoving(direction);
	}
	
	protected override List<Direction> GetAvailableDirections()
	{
		SnakeHead head = this.snake.Head;
		Vector3 position = head.CurrentDestination;
		MazeCell cell = this.mazeController.GetCellForPosition( position );
		
		List<Direction> availableDirections = cell.UnblockedDirections;		
		return availableDirections;
	}
	
}



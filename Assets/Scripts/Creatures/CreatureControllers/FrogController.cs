using System.Collections.Generic;
using UnityEngine;
using Serpent;

public class FrogController : CreatureController
{
	const int LongJumpDistance = 2;
	const int SnakeDangerDistance = 4;
	
	private GameManager gameManager;
	private Frog frog;
	
	public FrogController( Creature creature, GameManager gameManager, MazeController mazeController ) : base(creature, mazeController)
	{
		this.frog = creature as Frog;
		this.gameManager = gameManager;
	}
	
	/// <summary>
	/// Handles the arrival event - the creature arriving at the centre of a tile.  Returns the
	/// direction to travel in from this point on.
	/// </summary>
	public override Direction NewDirectionUponArrival()	
	{
		// Don't move again immediately after arriving.
		return Direction.None;
	}
	
	public void Hop()
	{
		Egg nearestEgg = GetNearestEgg();
		bool snakeNearby = SnakeIsNearby();
		if (nearestEgg == null || snakeNearby)
		{
			MoveRandomly();
			return;
		}
		
		MoveTowardsEgg(nearestEgg);
	}
	
	private void MoveRandomly()
	{
		// Random move?  NOTE: this pays attention to walls...
		List<Direction> availableDirections = this.GetAvailableDirections();
		if (availableDirections.Count == 0) { return; }
		
		int roll = UnityEngine.Random.Range(0, availableDirections.Count);		
		Direction chosenDir = availableDirections[roll];
		StartMoving(chosenDir);		
		TakeLongJump(chosenDir);		
	}
	
	public override void StartMoving(Direction dir)
	{
		this.frog.StartMoving(dir);
	}
	
	private void MoveTowardsEgg(Egg e)
	{
		Vector3 eggPosition = e.transform.localPosition;
		MazeCell cell = this.mazeController.GetCellForPosition( eggPosition );
		Vector3 eggCellCentre = this.mazeController.GetCellCentre( cell.X, cell.Y );
		List<Direction> availableDirections = GetAvailableDirectionsTowards( eggCellCentre );
		if (availableDirections.Count == 0) 
		{
			// Do a random jump instead
			MoveRandomly();
			return;
		}
		
		int roll = UnityEngine.Random.Range(0, availableDirections.Count);		
		Direction chosenDir = availableDirections[roll];
		StartMoving(chosenDir);
		
		// Test whether we want to do a long jump towards the egg.  i.e. don't if we would overshoot.
		if (this.frog.CurrentDestination.x == eggCellCentre.x) 
		{
			// avoid jumps that would go past this column. i.e. west and east
			if (chosenDir == Direction.W || chosenDir == Direction.E)
			{
				return; 
			}
		}
		
		if (this.frog.CurrentDestination.y == eggCellCentre.y) 
		{
			// avoid jumps that would go past this row. i.e. north and south
			if (chosenDir == Direction.N || chosenDir == Direction.S)
			{	
				return;
			}
		}
		
		TakeLongJump(chosenDir);
	}
	
	#region Utilities
	
	private bool SnakeIsNearby()
	{
		Snake s = GetNearestSnake();
		if (s == null) 
		{
			return false; 
		}
		if (s.Head == null)
		{
			Debug.Log("Nearest snake to frog has no head!");
			return false;
		}
		
		if (this.frog == null)
		{
			Debug.Log ("Frog controller still executing but frog is gone!?!");
			return false;
		}
		
		Vector3 distance2D = s.Head.transform.localPosition - this.frog.transform.localPosition;
		float cellDistance = Mathf.Abs(distance2D.x) / SerpentConsts.CellWidth + Mathf.Abs(distance2D.y) / SerpentConsts.CellHeight;
		if (cellDistance < SnakeDangerDistance)
		{
			return true;
		}
		return false;
	}
	
	private Snake GetNearestSnake()
	{		
		List<Snake> snakes = this.gameManager.GetSnakes();
		List<Creature> creatures = new List<Creature>();
		foreach( Snake s in snakes )
		{
			creatures.Add( s );
		}
		return this.frog.GetNearestCreature( creatures ) as Snake;
	}
	
	private Egg GetNearestEgg()
	{
		List<Creature> eggs = this.gameManager.GetEggs();
		return this.frog.GetNearestCreature( eggs ) as Egg;
	}
	
	protected override List<Direction> GetAvailableDirections()
	{
		// Frogs ignore the maze since they can jump over walls.  They just need to take into account the edge of the map.
		// Except that I do want them to be able to leave the map sometimes.
		Vector3 position = this.frog.transform.localPosition;
		MazeCell cell = this.mazeController.GetCellForPosition( position );
				
		List<Direction> availableDirections = new List<Direction>();
		
		for (Direction dir = Direction.First; dir != Direction.Last; ++dir)
		{
			if (CanGo( dir, cell.X, cell.Y ))
			{
				availableDirections.Add( dir );
			}
		}
		
		
		return availableDirections;
	}
	
	protected List<Direction> GetAvailableDirectionsTowards(Vector3 destination)
	{
		Vector3 position = this.frog.transform.localPosition;
		MazeCell cell = this.mazeController.GetCellForPosition( position );
		List<Direction> availableDirections = new List<Direction>();
		
		if (destination.x < position.x) 
		{ 
			if (CanGo(Direction.W, cell.X, cell.Y))
			{
				availableDirections.Add( Direction.W ); 
			}
		}
		
		if (destination.x > position.x) 
		{
			if (CanGo(Direction.E, cell.X, cell.Y))
			{
				availableDirections.Add( Direction.E ); 
			}
		}
		
		if (destination.y < position.y) 
		{
			if (CanGo(Direction.S, cell.X, cell.Y))
			{
				availableDirections.Add( Direction.S ); 
			}
		}
		
		if (destination.y > position.y) 
		{
			if (CanGo(Direction.N, cell.X, cell.Y))
			{   
				availableDirections.Add( Direction.N ); 
			}
		}
		
		return availableDirections;
	}
	
	private bool CanGo( Direction direction, int fromX, int fromY )
	{
		int x = fromX + SerpentConsts.DirectionVector[ (int)direction ].x;
		int y = fromY + SerpentConsts.DirectionVector[ (int)direction ].y;
		return AcceptableDestination( x, y );
	}
	
	private bool AcceptableDestination(int x, int y)
	{
		if (x < 0 || x >= this.mazeController.Maze.Width || y < 0 || y >= this.mazeController.Maze.Height)
		{
			return false;
		}
		
		MazeCell cell = this.mazeController.Maze.Cells[x,y];
		if (cell == null || cell.InPlayerZone) { return false; }
		
		return true;
	}
	
	private void TakeLongJump(Direction dir)
	{
		Vector3 position = this.frog.transform.localPosition;
		MazeCell cell = this.mazeController.GetCellForPosition( position );
		
		IntVector2 nextPos = new IntVector2(cell.X, cell.Y);
		for (int i = 0; i < LongJumpDistance; ++i)
		{
			nextPos = nextPos + SerpentConsts.DirectionVector[ (int) dir ];
			if (AcceptableDestination(nextPos.x, nextPos.y) == false)
			{
				// abort
				return;
			}
		}
		
		// Change the frog's CurrentDestination to the cell centre of nextPos.
		Vector3 newDest = this.mazeController.GetCellCentre(nextPos.x, nextPos.y);
		this.frog.CurrentDestination = newDest;
	}
	
	/*
	private bool CanTakeLongJump(Direction dir)
	{
		Vector3 position = this.frog.transform.localPosition;
		MazeCell cell = this.mazeController.GetCellForPosition( position );
		
		IntVector2 nextPos = new IntVector2(cell.X, cell.Y);
		for (int i = 0; i < LongJumpDistance; ++i)
		{
			nextPos = nextPos + SerpentConsts.DirectionVector[ (int) dir ];
			if (AcceptableDestination(nextPos.x, nextPos.y) == false)
			{
				return false;
			}
		}
		
		return true;
	}
	*/
	
	/*
	private IntVector2 GetLongJumpDestination(Direction dir)
	{
		
	}
	*/
	#endregion Utilities
}



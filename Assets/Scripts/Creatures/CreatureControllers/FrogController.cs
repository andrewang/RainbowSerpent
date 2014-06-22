using System.Collections.Generic;
using UnityEngine;

public class FrogController : CreatureController
{
	private float		movementDelay;		// should this be in SerpentConsts?
	private float		currentMovementDelay;
	private GameManager gameManager;
	
	public FrogController( Creature creature, MazeController mazeController ) : base(creature, mazeController)
	{
	}
	
	public void SetUp( GameManager gameManager )
	{
		this.gameManager = gameManager;
	}
	
	/// <summary>
	/// Handles the arrival event - the creature arriving at the centre of a tile.  Returns the
	/// direction to travel in from this point on.
	/// </summary>
	public override SerpentConsts.Dir NewDirectionUponArrival()	
	{
		// Execute a delay before moving	
		this.currentMovementDelay = this.movementDelay;
		
		return SerpentConsts.Dir.None;
	}
	
	public void Update()
	{
		// Decrement movement delay and don't move if the delay hasn't expired
		if (this.currentMovementDelay > 0.0f)
		{
			this.currentMovementDelay -= RealTime.deltaTime;
			if (this.currentMovementDelay > 0.0f)
			{
				return;
			}
		}
		
		DecideOnHop();
	}
	
	private void DecideOnHop()
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
		// Random move?
		/*
		List<SerpentConsts.Dir> availableDirections = this.GetAvailableDirections();
		if (availableDirections.Count == 0) { return; }
		
		int roll = UnityEngine.Random.Range(0, availableDirections.Count);		
		this.CurrentDirection = availableDirections[roll];
		
		Vector3 dest = this.MazeController.GetNextCellCentre(this.transform.localPosition, this.CurrentDirection);
		this.CurrentDestination = dest;
		*/
	}
	
	private void MoveTowardsEgg(Egg e)
	{
	}
	
	#region Utilities
	
	private bool SnakeIsNearby()
	{
		Snake s = GetNearestSnake();
		Vector3 distance2D = s.Head.transform.localPosition - this.creature.transform.localPosition;
		float cellDistance = Mathf.Abs(distance2D.x) / SerpentConsts.CellWidth + Mathf.Abs(distance2D.y) / SerpentConsts.CellHeight;
		if (cellDistance < 4) // TODO FIX MAGIC NUMBER
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
		return this.creature.GetNearestCreature( creatures ) as Snake;
	}
	
	private Egg GetNearestEgg()
	{
		List<Creature> eggs = this.gameManager.GetEggs();		
		return this.creature.GetNearestCreature( eggs ) as Egg;
	}
	
	#endregion Utilities
}



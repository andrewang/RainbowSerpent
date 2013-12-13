using System;
using UnityEngine;

public class Creature : MonoBehaviour
{
	// Faction is used for player snake vs enemy snake vs frog (vs ???).  Interactions should be 
	// skipped between creatures of the same faction.
	public int Faction
	{
		get; set; 
	}
	
	public CreatureController Controller
	{
		get; set;
	}
	
	public MazeController MazeController
	{
		get; set; 
	}
	
	/// <summary>
	/// The current direction.  Creatures can only change direction at the centre of tiles
	/// </summary>
	private SerpentConsts.Dir currentDirection = SerpentConsts.Dir.None;
	protected SerpentConsts.Dir CurrentDirection
	{
		get
		{
			return currentDirection;
		}
		set
		{
			this.currentDirection = value;
			this.currentDirectionVector = SerpentConsts.DirectionVector3[ (int)value ];
		}
	}
	
	private Vector3 currentDirectionVector;
	protected Vector3 CurrentDirectionVector
	{
		get
		{
			return this.currentDirectionVector;
		}
	}
	
	protected Vector3 CurrentDestination
	{
		get; set; 
	}
	
	public void SetUp(MazeController mazeController)
	{
		this.MazeController = mazeController;
	}

	public virtual void Update()
	{
		// Check for interactions with other creatures (after moving).  This should be through 
		// a virtual function which can be override for snakes to check head to body interactions.
		
		// Update position based on speed and direction.  When we reach the centre of a tile, make a callback to the
		// Controller
		
		// If we reach the centre of a tile, after making the callback, if movement in the current direction is 
		// impossible, stop.
	}
}


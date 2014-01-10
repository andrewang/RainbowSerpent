using System;
using UnityEngine;

public class MobileCreature : Creature
{
	
	public CreatureController Controller
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
}


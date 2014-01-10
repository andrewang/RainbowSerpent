using System;
using UnityEngine;

public class Creature : MonoBehaviour
{
	/*
	public enum CreatureCategory
	{
		PlayerSnake,
		EnemySnake,
		Frog
	}
	
	// Faction is used for player snake vs enemy snake vs frog (vs ???).  Interactions should be 
	// skipped between creatures of the same faction.
	public CreatureCategory Category
	{
		get; set; 
	}
	*/
	
	public float Radius
	{
		get
		{
			// Average width and height and then halve again in order to get a radius value.
			return 1.0f;
		}
	}
	
	public MazeController MazeController
	{
		get; set; 
	}
	
	public void SetUp(MazeController mazeController)
	{
		this.MazeController = mazeController;
	}

	/// <summary>
	/// Tests for interaction.
	/// </summary>
	/// <returns><c>true</c>, if the other creature should die, <c>false</c> otherwise.</returns>
	/// <param name="otherCreature">Other creature.</param>
	public virtual bool TestForInteraction(Creature otherCreature)
	{		
		return false;
	}
	
	public virtual void SetInitialLocation(Vector3 position, SerpentConsts.Dir facingDirection)
	{
		Vector3 rotation = SerpentConsts.RotationVector3[ (int) facingDirection ];
		
		this.transform.eulerAngles = rotation;		
		this.transform.localPosition = position;
				
	}
	
}


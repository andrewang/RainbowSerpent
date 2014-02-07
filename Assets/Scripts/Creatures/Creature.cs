using System;
using UnityEngine;
using System.Collections.Generic;

public class Creature : MonoBehaviour
{
	// Many subclasses of creature have a single sprite, although the snake class does not.
	[SerializeField] protected UISprite sprite;	
	
	public float Radius
	{
		get
		{
			// Average width and height and then halve this average diameter in order to get a radius.
			return (this.sprite.height + this.sprite.width) * 0.25f;
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
	
	private float DistanceSqToCreature( Creature otherCreature )
	{
		Vector3 positionDiff = this.transform.localPosition - otherCreature.transform.localPosition;
		float distanceSq = positionDiff.sqrMagnitude;
		return distanceSq;		
	}
	
	protected Creature GetNearestCreature( List<Creature> creatures )
	{		
		if (creatures.Count == 0) { return null; }
		
		Creature nearestCreature = creatures[0]; 
		float nearestDistSq = DistanceSqToCreature( nearestCreature );
		
		for (int i = 1; i < creatures.Count; ++i)
		{
			float distSq = DistanceSqToCreature( creatures[i] );
			if (distSq < nearestDistSq)
			{
				nearestCreature = creatures[i];
				nearestDistSq = distSq;				
			}
		}
		
		return nearestCreature;
	}
}


using System;
using UnityEngine;
using System.Collections.Generic;
using Serpent;

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
	
	public Side Side { get; set; }
	
	public bool Dead { get; set; }
		
	public event Action<Creature> CreatureDied;
		
	protected MazeController mazeController;
	
	// Default creature behavior uses the creature object's own transform as its position.  Not the case with snakes
	protected virtual Vector3 GetPosition()	
	{
		return this.transform.localPosition;
	}
	
	public void SetUp(MazeController mazeController)
	{
		this.mazeController = mazeController;
		
		// default side is enemy
		this.Side = Side.Enemy;
		
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
	
	public virtual void Die()
	{	
		if (this.CreatureDied != null)
		{
			this.CreatureDied(this);
		}
	}
	
	public virtual void SetInitialLocation(Vector3 position, Direction facingDirection, bool withinTile = false)
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
	
	public Creature GetNearestCreature( List<Creature> creatures )
	{		
		if (creatures.Count == 0) { return null; }
		
		Creature nearestCreature = null;
		float nearestDistSq = 0.0f;
		
		for (int i = 0; i < creatures.Count; ++i)
		{
			if (creatures[i] == null) { continue; }
			
			if (nearestCreature == null)
			{
				nearestCreature = creatures[i]; 
				nearestDistSq = DistanceSqToCreature( nearestCreature );
				continue;
			}
			
			float distSq = DistanceSqToCreature( creatures[i] );
			if (distSq < nearestDistSq)
			{
				nearestCreature = creatures[i];
				nearestDistSq = distSq;				
			}
		}
		
		return nearestCreature;
	}
	
	public bool TouchesCreature( Creature otherCreature )
	{
		Vector3 positionDiff = this.transform.localPosition - otherCreature.transform.localPosition;
		float distanceSq = positionDiff.sqrMagnitude;
		float radii = this.Radius + otherCreature.Radius;
		float radiiSq = radii * radii;
		return (distanceSq <= radiiSq);		
	}
	
	#region Sprite
	
	public virtual void SetSpriteDepth(int depth)
	{
		this.sprite.depth = depth;
	}
	
	#endregion Sprite	
}


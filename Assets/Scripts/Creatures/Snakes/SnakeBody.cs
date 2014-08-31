using UnityEngine;
using System.Collections;
using System;
using SerpentExtensions;

public class SnakeBody : SnakeSegment 
{
	public float DistanceFromHead { get; set; }
	
	// The final segment of a snake may have an egg that it is creating.
	public Egg Egg { get; set; }
		
	public override void Reset()
	{
		base.Reset();
		RemoveEgg();
		this.DistanceFromHead = 0.0f;
	}
	
	public void RemoveEgg()
	{
		if (this.Egg)
		{
			this.Egg.Die();
			this.Egg = null; 
		}	
	}
	
	public void MoveEgg(SnakeBody otherSegment)
	{
		// When this method is called, if this segment has an egg attached,
		// the egg should be transferred to the other segment
		if (this.Egg == null) { return; }
				
		otherSegment.BeginToCreateEgg(this.Egg);
		this.Egg.FullyGrown -= this.EggFullyGrown;		
		this.Egg = null;
	}
	
	public void BeginToCreateEgg(Egg egg)
	{
		egg.SetParent(this);
		
		// Make sure egg is displayed on top of the segment
		egg.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		egg.SetSpriteDepth(GetSpriteDepth() + 1);
		
		this.Egg = egg;
		egg.FullyGrown += this.EggFullyGrown;
	}
	
	public void EggFullyGrown(Egg e)
	{		
		// Check for null in case of conflict between timer and snake+egg death.
		if (this.Egg == null) 
		{ 
			return; 
		}
		
		// set the depth of the egg sprite so that the egg is just below this snake.
		this.Egg.SetSpriteDepth( this.sprite.depth - 1 ); 
		
		this.Egg.FullyGrown -= this.EggFullyGrown;
		this.Egg = null;
		
		// this snake segment needs to be removed from its snake.		
		this.Snake.SeverAtSegment(this);
	}
	
	public override void OnDestroy()
	{
		base.OnDestroy();

		// If a snake dies, partway through laying an egg, we need to destroy the egg object too.
		if (this.Egg != null)
		{
			this.Egg.Die();
			this.Egg = null;
		}
	}
	
	override protected void ShrinkComplete()
	{
		Managers.SnakeBodyCache.ReturnObject<SnakeBody>(this.gameObject);		
	}
}
